using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data
{
    public class OAuthCodeStore
    {
        public string Code { get; set; }

        public string LoginChallenge { get; set; }

        /// <summary>
        /// This will be the foreign key to the user info table
        /// </summary>
        public string Email { get; set; }

        public DateTime IssuedAt { get; set; }
    }
}
