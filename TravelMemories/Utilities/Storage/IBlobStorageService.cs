using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System.Reflection.Metadata;
using TravelMemories.Contracts.Storage;
using TravelMemories.Database;

namespace TravelMemories.Utilities.Storage
{
    public interface IBlobStorageService
    {
        public Task<string> UploadBlobAsync(string fileName, Stream fileStream);

        public Task<List<ImageData>> GetImagesFromBlob();
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly ImageMetadataDBContext _imageMetadataDBContext;
        private readonly BlobContainerClient _containerClient;
        private readonly IConfiguration _configuration;

        public BlobStorageService(IConfiguration configuration, ImageMetadataDBContext imageMetadataDBContext)
        {
            _configuration = configuration;
            var connString = configuration["BlobStorage_ConnectionString"];
            var containerName = configuration["BlobStorage_ContainerName"];

            _containerClient = new BlobContainerClient(connString, containerName);
            _imageMetadataDBContext = imageMetadataDBContext;
        }

        public async Task<string> UploadBlobAsync(string fileName, Stream fileStream)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, true);

            return blobClient.Uri.ToString();
        }

        public async Task<List<ImageData>> GetImagesFromBlob()
        {
            var allBlobs = _containerClient.GetBlobs();

            var groupedRes = _imageMetadataDBContext.ImageMetadata.GroupBy(x => x.TripName).ToList();

            List<ImageData> allTripDetails = new List<ImageData>();

            foreach (var trip in groupedRes )
            {
                ImageData imageData = new ImageData();
                foreach (var tripDetail in trip)
                {
                    string blobName = Path.Combine(tripDetail.Year.ToString(), tripDetail.TripName, tripDetail.ImageName);
                    var blobClient = _containerClient.GetBlobClient(blobName);

                    if (blobClient.CanGenerateSasUri)
                    {
                        BlobSasBuilder sasBuilder = new BlobSasBuilder
                        {
                            BlobContainerName = _configuration["BlobStorage_ContainerName"],
                            BlobName = blobName,
                            Resource = "b",
                            ExpiresOn = DateTime.UtcNow.AddHours(1),
                        };

                        sasBuilder.SetPermissions(BlobSasPermissions.Read);

                        Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

                        imageData.TripTitle = tripDetail.TripName;
                        imageData.Year = tripDetail.Year;
                        imageData.Lat = tripDetail.X;
                        imageData.Lon = tripDetail.Y;
                        imageData.ImageUrls.Add(sasUri.AbsoluteUri);
                    }
                }
                allTripDetails.Add(imageData);
            }

            return allTripDetails;
        }
    }
}
