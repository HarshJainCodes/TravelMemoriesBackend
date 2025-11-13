using Azure.Storage.Blobs;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using TravelMemories.Contracts.Data;
using TravelMemoriesBackend.Contracts.Storage;
using TravelMemories.Controllers.Authentication;
using TravelMemories.Database;
using TravelMemories.Utilities.Request;
using TravelMemories.Utilities.Storage;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Controllers.Storage
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ImageUploadController : ControllerBase
    {
        IBlobStorageService _blobStorageService;
        IImageCompressService _imageCompressService;
        IRequestContextProvider _requestContextProvider;
        ILogger<ImageUploadController> _logger;

        private readonly ImageMetadataDBContext _imageMetadataDBContext;

        public ImageUploadController(ImageMetadataDBContext imageMetadataDBContext, 
            IBlobStorageService blobStorageService, 
            IImageCompressService imageCompressService, 
            ILogger<ImageUploadController> logger,
            IRequestContextProvider requestContextProvider)
        {
            _imageMetadataDBContext = imageMetadataDBContext;
            _blobStorageService = blobStorageService;
            _imageCompressService = imageCompressService;
            _requestContextProvider = requestContextProvider;
            _logger = logger;
        }

        [HttpGet("CheckLogin")]

        public async Task<IActionResult> CheckLogin()
        {
            JwtSecurityToken jwtToken = _requestContextProvider.GetJWTToken();

            string userEmail = jwtToken.Claims.Where(c => c.Type == "email").First().Value;
            UserInfo user = _imageMetadataDBContext.UserInfo.Where(u => u.Email == userEmail).First();

            string profilePicUrl = user.ProfilePictureURL;
            string userName = user.Name;

            return Ok(new { userEmail, profilePicUrl, userName });
        }

        /// <summary>
        /// Uploads the images to Blob Storage after compressing them
        /// Also updates the metadata database with the image details like year, trip name, lat, lon etc.
        /// </summary>
        /// <param name="images"></param>
        /// <param name="tripTitle"></param>
        /// <param name="year"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImageToBlobAsync(List<IFormFile> images, string tripTitle, int year, float lat, float lon)
        {
            if (images.Count == 0)
            {
                return BadRequest("Please attach atleast 1 image");
            }

            var jpegOptions = new JpegEncoder { Quality = 50, SkipMetadata = true };

            JwtSecurityToken jwtToken = _requestContextProvider.GetJWTToken();
            string userEmail = jwtToken.Claims.Where(cl => cl.Type == "email").FirstOrDefault().Value;
            SubscriptionDetails userSubDetail = _imageMetadataDBContext.SubscriptionDetails.Where(x => x.UserEmail == userEmail).FirstOrDefault();

            // if the storage of user is full, then don't allow user to upload more images
            if (userSubDetail.StorageUsedInGB >= userSubDetail.StorageCapacityInGB)
            {
                return BadRequest("You have exhausted your storage limit. Please upgrade your plan to upload more images.");
            }

            float storageUsedInBytes = 0;

            foreach (IFormFile image in images)
            {
                MemoryStream compressedStream = _imageCompressService.CompressImage(image, jpegOptions);
                storageUsedInBytes += compressedStream.Length;

                _logger.LogInformation($"Uploading {image.FileName} to Blob Storage");
                await _blobStorageService.UploadBlobAsync(Path.Combine(userEmail, year.ToString(), tripTitle, image.FileName), compressedStream);
                _logger.LogInformation($"Done Uploading {image.FileName} to Blob Storage");

                // metadata for the same file
                _imageMetadataDBContext.ImageMetadata.Add(new TravelMemoriesBackend.Contracts.Data.ImageMetadata
                {
                    Year = year,
                    ImageName = image.FileName,
                    TripName = tripTitle,
                    X = lat,
                    Y = lon,
                    UploadedByEmail = userEmail,
                });
            }

            userSubDetail.StorageUsedInGB += storageUsedInBytes / (1024f * 1024f * 1024f);

            _logger.LogInformation($"{userEmail} uploaded a total of {storageUsedInBytes / (1024f * 1024f * 1024f)} MB");

            await _imageMetadataDBContext.SaveChangesAsync();
            return Ok("Image uploaded successfully");
        }

        [HttpGet("AllTripData")]
        public async Task<List<ImageData>> GetImagesFromBlob()
        {
            List<ImageData> allData = await _blobStorageService.GetImagesFromBlob();
            _logger.LogInformation($"Got the data for all the trips, you have total {allData.Count} trips so far");
            return allData;
        }

        // This was used to move the blobs to another folder for internal business logic
        /*[HttpPost("Move")]
        public async Task<ActionResult> MoveALLBlobs(string destinationFolderName)
        {
            await _blobStorageService.MoveAllBlobs(destinationFolderName);
            return Ok();
        }*/
    }
}
