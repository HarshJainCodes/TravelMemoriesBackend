using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data
{
    public class UserInfo
    {
        public int UserID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ProfilePictureURL { get; set; }

        public bool IsManualLogin { get; set; }

        // this is usefull for foreign key, metadata table will reference this
        public ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
    }
}
