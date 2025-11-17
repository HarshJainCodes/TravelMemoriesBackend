using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data
{
    public class UserInfo
    {
        /// <summary>
        /// The unique user id, this is automatically assigned and is currently a integer?
        /// </summary>
        public int UserID { get; set; }
        
        /// <summary>
        /// The name of the user, for the user who did google sign in, we get this name from google
        /// If the user used the send OTP flow, than we dont have the user name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The user name
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Not used anymore, we depricated the Password flow, why take the trouble to store and encrypt the user password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// We also get this from google
        /// </summary>
        public string ProfilePictureURL { get; set; }

        /// <summary>
        /// Not needed anymore, this was used in the old password flow
        /// </summary>
        public bool IsManualLogin { get; set; }

        // this is usefull for foreign key, metadata table will reference this
        public ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();

        // every user will have only one subscription details
        public SubscriptionDetails SubscriptionDetails { get; set; }

        /// <summary>
        /// A user can start various conversations
        /// </summary>
        public ICollection<ChatConversation> ChatConversations { get; set; } = new List<ChatConversation>();
    }
}
