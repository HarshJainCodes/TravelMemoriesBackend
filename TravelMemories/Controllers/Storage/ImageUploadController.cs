using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TravelMemories.Contracts.Storage;
using TravelMemories.Database;
using TravelMemories.Utilities.Storage;

namespace TravelMemories.Controllers.Storage
{
    [ApiController]
    [Route("[controller]")]
    public class ImageUploadController : ControllerBase
    {
        IBlobStorageService _blobStorageService;
        IImageCompressService _imageCompressService;
        ILogger<ImageUploadController> _logger;

        private readonly ImageMetadataDBContext _imageMetadataDBContext;

        public ImageUploadController(ImageMetadataDBContext imageMetadataDBContext, IBlobStorageService blobStorageService, IImageCompressService imageCompressService, ILogger<ImageUploadController> logger)
        {
            _imageMetadataDBContext = imageMetadataDBContext;
            _blobStorageService = blobStorageService;
            _imageCompressService = imageCompressService;
            _logger = logger;
        }

        // there will be two methods, one that will upload image
        // second method will upload the image metadata
        [HttpPost]
        public async Task<IActionResult> UploadImageToBlobAsync(List<IFormFile> images, string tripTitle, int year, float lat, float lon)
        {
            if (images == null)
            {
                return BadRequest("Please attach a valid image");
            }

            var jpegOptions = new JpegEncoder { Quality = 50, SkipMetadata = true };

            foreach (IFormFile image in images)
            {
                MemoryStream compressedStream = _imageCompressService.CompressImage(image, jpegOptions);

                _logger.LogInformation($"Uploading {image.FileName} to Blob Storage");
                await _blobStorageService.UploadBlobAsync(Path.Combine(year.ToString(), tripTitle, image.FileName), compressedStream);
                _logger.LogInformation($"Done Uploading {image.FileName} to Blob Storage");

                // metadat for the same file
                _imageMetadataDBContext.ImageMetadata.Add(new Contracts.Data.ImageMetadata
                {
                    Year = year,
                    ImageName = image.FileName,
                    TripName = tripTitle,
                    X = lat,
                    Y = lon
                });
            }

            _imageMetadataDBContext.SaveChanges();
            return Ok("Image uploaded successfully");
        }

        [HttpGet("AllTripData")]
        public async Task<List<ImageData>> GetImagesFromBlob()
        {
            List<ImageData> allData = await _blobStorageService.GetImagesFromBlob();
            _logger.LogInformation($"Got the data for all the trips, you have total {allData.Count} trips so far");
            return allData;
        }
    }
}
