namespace TravelMemories.Contracts.Data
{
    public class UserInfo
    {
        public int UserID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        // this is usefull for foreign key, metadata table will reference this
        public ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
    }
}
