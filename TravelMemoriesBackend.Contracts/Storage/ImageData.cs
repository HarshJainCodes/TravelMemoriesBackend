using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Storage
{
    public class ImageData
    {
        public string TripTitle { get; set; }
        public string Email { get; set; }

        public float Lat { get; set; }

        public float Lon { get; set; }

        public int Year { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
