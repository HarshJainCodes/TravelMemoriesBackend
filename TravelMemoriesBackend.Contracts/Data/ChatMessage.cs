using ModelContextProtocol.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageRole
    {
        [JsonPropertyName("user")] User,
        [JsonPropertyName("assistant")] Assistant,
        [JsonPropertyName("system")] System,
        [JsonPropertyName("tool")] Tool
    }

    public enum MessageType
    {
        [JsonPropertyName("root")] Root,
        [JsonPropertyName ("text")] Text,
        [JsonPropertyName("think")] Think,
        [JsonPropertyName("system")] System,
    }

    public class ChatMessage
    {
        /// <summary>
        /// The unique message id for each message
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// This is a reference to the conversation id
        /// </summary>
        public Guid ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the role associated with the message.
        /// </summary>
        public MessageRole Role { get; set; }

        public MessageType Type { get; set; }

        public string Content { get; set; }

        public DateTime Timestamp { get; set; }

        public string? ReasoningContent { get; set; }

        /// <summary>
        /// Serialized Json Array
        /// </summary>
        public string? ToolCalls { get; set; }

        public string? ToolCallId { get; set; }

        public string? Model { get; set; }


        /// <summary>
        /// This message is a part of which conversation
        /// </summary>
        public ChatConversation ChatConversation { get; set; }
    }
}
