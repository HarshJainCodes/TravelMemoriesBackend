﻿namespace TravelMemories.Contracts.Data
{
    public class ImageMetadata
    {
        public int ID { get; set; }

        public int Year {  get; set; }

        public string UploadedByEmail { get; set; }

        public string TripName { get; set; }

        public string ImageName { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        // foreign key reference
        public UserInfo UploadedBy { get; set; }
    }
}
