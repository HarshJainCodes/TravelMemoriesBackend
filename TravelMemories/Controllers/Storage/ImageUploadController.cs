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
using TravelMemories.Contracts.Storage;
using TravelMemories.Controllers.Authentication;
using TravelMemories.Database;
using TravelMemories.Utilities.Request;
using TravelMemories.Utilities.Storage;

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
            string profilePicUrl = _imageMetadataDBContext.UserInfo.Where(u => u.Email == userEmail).First().ProfilePictureURL;

            return Ok(new { userEmail, profilePicUrl });
        }

        // there will be two methods, one that will upload image
        // second method will upload the image metadata
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadImageToBlobAsync(List<IFormFile> images, string tripTitle, int year, float lat, float lon)
        {
            if (images.Count == 0)
            {
                return BadRequest("Please attach atleast 1 image");
            }

            var jpegOptions = new JpegEncoder { Quality = 50, SkipMetadata = true };

            JwtSecurityToken jwtToken = _requestContextProvider.GetJWTToken();
            string userEmail = jwtToken.Claims.Where(cl => cl.Type == "email").FirstOrDefault().Value;

            foreach (IFormFile image in images)
            {
                MemoryStream compressedStream = _imageCompressService.CompressImage(image, jpegOptions);

                _logger.LogInformation($"Uploading {image.FileName} to Blob Storage");
                await _blobStorageService.UploadBlobAsync(Path.Combine(userEmail, year.ToString(), tripTitle, image.FileName), compressedStream);
                _logger.LogInformation($"Done Uploading {image.FileName} to Blob Storage");

                // metadata for the same file
                _imageMetadataDBContext.ImageMetadata.Add(new Contracts.Data.ImageMetadata
                {
                    Year = year,
                    ImageName = image.FileName,
                    TripName = tripTitle,
                    X = lat,
                    Y = lon,
                    UploadedByEmail = userEmail,
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

        // This was used to move the blobs to another folder for internal business logic
        /*[HttpPost("Move")]
        public async Task<ActionResult> MoveALLBlobs(string destinationFolderName)
        {
            await _blobStorageService.MoveAllBlobs(destinationFolderName);
            return Ok();
        }*/
    }
}
