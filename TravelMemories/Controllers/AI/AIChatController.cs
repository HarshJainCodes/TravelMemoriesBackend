using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using TravelMemories.Utilities.Request;

namespace TravelMemories.Controllers.AI
{
    [ApiController]
    [Route("[controller]")]
    public class AIChatController : ControllerBase
    {

        private readonly IRequestContextProvider _requestContextProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;


        public AIChatController(IRequestContextProvider requestContextProvider, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _requestContextProvider = requestContextProvider;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<string> GetAIResponse(UserPrmopt userPrompt)
        {
            JwtSecurityToken jwtToken = _requestContextProvider.GetJWTToken();
            string userEmail = jwtToken.Claims.Where(c => c.Type == "email").First().Value;
            string mcpServerName = GenerateUniqueString(userEmail);

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

            var history = new ChatHistory();
            history.AddUserMessage(userPrompt.Prompt);

            var result = await chatCompletionService.GetChatMessageContentAsync(history, executionSettings: openAIPromptExecutionSettings, kernel: kernel);
            return result.Content;

        }

        [NonAction]
        public static string GenerateUniqueString(string email)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(email));
                var base64 = Convert.ToBase64String(hashBytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "");

                // Take first 16 characters for uniqueness, prefix with "TM_"
                return $"TM_{base64.Substring(0, 16)}"; // Total: 19
            }
        }
    }

    public class UserPrmopt
    {
        public string Prompt { get; set; }
    }
}
