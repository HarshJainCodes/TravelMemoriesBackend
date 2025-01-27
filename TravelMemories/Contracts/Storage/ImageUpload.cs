namespace TravelMemories.Contracts.Storage
{
    // we will get this info from the frontend
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
