namespace TravelMemories.Contracts.Storage
{
    public class ImageData
    {
        public string TripTitle { get; set; }

        public float Lat { get; set; }

        public float Lon { get; set; }

        public int Year { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
