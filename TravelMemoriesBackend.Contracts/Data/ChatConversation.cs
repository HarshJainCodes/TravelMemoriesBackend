using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data
{
    public class ChatConversation
    {
        /// <summary>
        /// The unique conversation id for each conversation
        /// </summary>
        [Key]
        public Guid ConversationId { get; set; }

        /// <summary>
        /// The user who initiated the conversation
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// The name of the conversation that appears on the left side panel
        /// </summary>
        public string ConversationName { get; set; }

        /// <summary>
        /// The first time this conversation was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // foreign key referernce for the user who has started this conversation
        public UserInfo UserInfo { get; set; }

        /// <summary>
        /// All the messages this conversation holds
        /// </summary>
        public  ICollection<ChatMessage> ConversationMessages = new List<ChatMessage>();
    }
}
