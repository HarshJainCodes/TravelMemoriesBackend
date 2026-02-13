using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.SemanticKernel.Extensions;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TravelMemories.Database;
using TravelMemories.Utilities.Request;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Controllers.AI
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AIChatController : ControllerBase
    {

        private readonly IRequestContextProvider _requestContextProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ImageMetadataDBContext _imageMetadataDBContext;


        public AIChatController(IRequestContextProvider requestContextProvider, 
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            ImageMetadataDBContext imageMetadataDBContext)
        {
            _requestContextProvider = requestContextProvider;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _imageMetadataDBContext = imageMetadataDBContext;
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<ActionResult> DeleteConversation([FromQuery] Guid conversationId)
        {
            var token = _requestContextProvider.GetJWTToken();
            string userEmail = token.Claims.Where(c => c.Type == "email").First().Value;

            // user should be the owner of the conversation
            ChatConversation conv = _imageMetadataDBContext.ChatbotConversations.Where(conversation => conversation.ConversationId == conversationId).FirstOrDefault();
            if (conv == null)
            {
                return BadRequest("please provide a valid conversation id");
            }
            if (conv.UserEmail != userEmail)
            {
                return Unauthorized("You do not have access to this conversation");
            }

            await _imageMetadataDBContext.ChatbotConversations.Where(conversation => conversation.ConversationId == conversationId).ExecuteDeleteAsync();

            return NoContent();
        }

        [HttpGet]
        [Route("GetMessages")]
        public async Task<ActionResult<List<MessageFromConversationDTO>>> GetConversationMessages([FromQuery] Guid conversationId)
        {
            var token = _requestContextProvider.GetJWTToken();
            string userEmail = token.Claims.Where(c => c.Type == "email").First().Value;

            // user should be the owner of the conversation
            ChatConversation conv = _imageMetadataDBContext.ChatbotConversations.Where(conversation => conversation.ConversationId == conversationId).FirstOrDefault();
            if (conv == null)
            {
                return BadRequest("please provide a valid conversation id");
            }
            if (conv.UserEmail != userEmail)
            {
                return Unauthorized("You do not have access to this conversation");
            }

            var messages = await _imageMetadataDBContext.ChatMessages.Where(message => message.ConversationId == conversationId).Select(message => new MessageFromConversationDTO
            {
                Message = message.Message,
                MessageId = message.MessageId,
                CreatedAt = message.CreatedAt,
                Role = message.MessageRole,
            }).ToListAsync();

            return Ok(messages);
        }

        [HttpGet]
        [Route("Conversations")]
        public async Task<List<SideConversationsResponseDTO>> GetSideConversations()
        {
            var token = _requestContextProvider.GetJWTToken();
            string userEmail = token.Claims.Where(c => c.Type == "email").First().Value;

            return await _imageMetadataDBContext.ChatbotConversations.Where(conversation => conversation.UserEmail == userEmail).Select(conversation => new SideConversationsResponseDTO
            {
                ConversationId = conversation.ConversationId,
                ConversationName = conversation.ConversationName,
                CreatedAt = conversation.CreatedAt,
            }).ToListAsync();
        }

        [HttpGet]
        [Route("StreamResponse")]
        public async Task GetConversationResponse([FromQuery] Guid conversationId)
        {
            JwtSecurityToken jwtToken = _requestContextProvider.GetJWTToken();
            string userEmail = jwtToken.Claims.Where(c => c.Type == "email").First().Value;

            IKernelBuilder kernelBuilder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion("gpt-4o", "https://travel-memories-bot.openai.azure.com/", _configuration["AzureOpenAIKey"]);
            Kernel kernel = kernelBuilder.Build();

            string mcpServerName = GenerateUniqueString(Guid.NewGuid().ToString());

            HttpClient mcpHttpClient = _httpClientFactory.CreateClient();
            mcpHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken.RawData);

            KernelPlugin kernelPlugin = await kernel.Plugins.AddMcpFunctionsFromSseServerAsync(
                mcpServerName,
                new Uri(_configuration["MCPServerUrl"]),
                httpClient: mcpHttpClient
            );

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
            var chatHistory = new ChatHistory();

            List<ChatMessage> messagesOfThisConversation = _imageMetadataDBContext.ChatMessages.Where(message => message.ConversationId == conversationId).OrderBy(message => message.CreatedAt).ToList();
            messagesOfThisConversation.ForEach(message =>
            {
                if (message.MessageRole == ModelContextProtocol.Protocol.Role.User)
                {
                    chatHistory.AddUserMessage(message.Message);
                }
                else
                {
                    chatHistory.AddAssistantMessage(message.Message);
                }
            });

            string assistantGeneratedMessage = "";
            var result2 = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings: openAIPromptExecutionSettings, kernel: kernel);

            HttpContext.Response.Headers["Content-Type"] = "text/event-stream";
            HttpContext.Response.Headers["Cache-Control"] = "no-cache";

            var fullMessageBuilder = new StringBuilder();

            async Task WriteSseAsync(string text)
            {
                // SSE multi-line support: prefix each line with "data: "
                var payload = "data: " + text.Replace("\n", "\ndata: ") + "\n\n";
                var bytes = Encoding.UTF8.GetBytes(payload);
                await HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                await HttpContext.Response.Body.FlushAsync();
            }

            await foreach (var response in result2)
            {
                if (string.IsNullOrEmpty(response.Content)) continue;
                assistantGeneratedMessage += response.Content;
                await WriteSseAsync(response.Content);
            }

            var doneEvent = Encoding.UTF8.GetBytes("event: done\ndata: [DONE]\n\n");
            await HttpContext.Response.Body.WriteAsync(doneEvent, 0, doneEvent.Length);
            await HttpContext.Response.Body.FlushAsync();

            // add this message to the db
            _imageMetadataDBContext.ChatMessages.Add(new ChatMessage
            {
                MessageId = Guid.NewGuid(),
                ConversationId = conversationId,
                Message = assistantGeneratedMessage,
                MessageRole = ModelContextProtocol.Protocol.Role.Assistant,
                CreatedAt = DateTime.UtcNow,
            });

            await _imageMetadataDBContext.SaveChangesAsync();
        }

        [HttpPost]
        public async Task<ChatbotResponse> GetAIResponse(UserPrmopt userPrompt)
        {
            JwtSecurityToken jwtToken = _requestContextProvider.GetJWTToken();
            string userEmail = jwtToken.Claims.Where(c => c.Type == "email").First().Value;
            string mcpServerName = GenerateUniqueString(Guid.NewGuid().ToString());

            IKernelBuilder kernelBuilder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion("gpt-4o", "https://travel-memories-bot.openai.azure.com/", _configuration["AzureOpenAIKey"]);

            Kernel kernel = kernelBuilder.Build();

            HttpClient mcpHttpClient = _httpClientFactory.CreateClient();
            mcpHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken.RawData);

            KernelPlugin kernelPlugin = await kernel.Plugins.AddMcpFunctionsFromSseServerAsync(
                mcpServerName,
                new Uri(_configuration["MCPServerUrl"]),
                httpClient: mcpHttpClient
            );

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var chatHistory = new ChatHistory();

            Guid responseConversationId;
            if (userPrompt.ConversationId == null)
            {
                // create a new conversation
                Guid currentConvId = Guid.NewGuid();
                responseConversationId = currentConvId;
                ChatConversation currentConv = new ChatConversation
                {
                    ConversationId = currentConvId,
                    UserEmail = userEmail,
                    ConversationName = "Default Name",
                    CreatedAt = DateTime.UtcNow,
                };

                _imageMetadataDBContext.ChatMessages.Add(new ChatMessage
                {
                    MessageId = Guid.NewGuid(),
                    ConversationId = currentConvId,
                    Message = userPrompt.Prompt,
                    MessageRole = ModelContextProtocol.Protocol.Role.User,
                    CreatedAt = DateTime.UtcNow,
                });

                // get a name for this conversation
                var tempChatHistory = new ChatHistory();
                tempChatHistory.AddUserMessage($"This is the prompt the user has provided: ${userPrompt.Prompt}. Suggest a conversation name based on this prompt. Only give the name as output nothing else. Do not include quotes");
                var newConvName = await chatCompletionService.GetChatMessageContentAsync(tempChatHistory, executionSettings: openAIPromptExecutionSettings, kernel: kernel);
                currentConv.ConversationName = newConvName.Content;
                _imageMetadataDBContext.ChatbotConversations.Add(currentConv);

            }
            else
            {
                responseConversationId = userPrompt.ConversationId.GetValueOrDefault();

                _imageMetadataDBContext.ChatMessages.Add(new ChatMessage
                {
                    MessageId = Guid.NewGuid(),
                    ConversationId = responseConversationId,
                    Message = userPrompt.Prompt,
                    MessageRole = ModelContextProtocol.Protocol.Role.User,
                    CreatedAt = DateTime.UtcNow,
                });

                await _imageMetadataDBContext.SaveChangesAsync();
            }

            await _imageMetadataDBContext.SaveChangesAsync();

            return new ChatbotResponse
            {
                //Content = result.Content,
                ConversationId = responseConversationId
            };

        }

        [NonAction]
        public static string GenerateUniqueString(string email)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(email));
                var hexString = Convert.ToHexString(hashBytes);


                return $"TM_{hexString.Substring(0, 25)}";
            }
        }
    }

    public class UserPrmopt
    {
        public Guid? ConversationId { get; set; }
        public string Prompt { get; set; }
    }

    public class ChatbotResponse
    {
        public string Content { get; set; }

        public Guid ConversationId { get; set; }
    }

    public class SideConversationsResponseDTO
    {
        public Guid ConversationId { get; set; }

        public string ConversationName { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class MessageFromConversationDTO
    {
        public Guid MessageId { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public Role Role { get; set; }
    }
}
