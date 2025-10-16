using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Storage
{
    public class ImageUpload
    {
        public int Year { get; set; }
        public DateTime VisitedDate { get; set; }
        public string Location { get; set; }
        public LocationCoords LocationCoords { get; set; }
    }

    public class LocationCoords
    {
        public float X { get; set; }

        public float Y { get; set; }
    }
}
