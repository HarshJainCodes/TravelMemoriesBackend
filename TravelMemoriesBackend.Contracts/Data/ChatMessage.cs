using ModelContextProtocol.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data
{
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
        /// The content of the message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Whether this message was provided by user or is a response from LLM
        /// </summary>
        public Role MessageRole {  get; set; }

        /// <summary>
        /// The time at which this message was created
        /// </summary>
        public DateTime CreatedAt {  get; set; }

        /// <summary>
        /// This message is a part of which conversation
        /// </summary>
        public ChatConversation ChatConversation { get; set; }
    }
}
