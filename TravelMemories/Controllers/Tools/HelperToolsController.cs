using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using TravelMemories.Database;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Controllers.Tools
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class HelperToolsController : ControllerBase
    {
        private readonly ImageMetadataDBContext _imageMetadataDBContext;
        private readonly BlobContainerClient _containerClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HelperToolsController> _logger;


        public HelperToolsController(
            ImageMetadataDBContext imageMetadataDBContext,
            IConfiguration configuration,
            ILogger<HelperToolsController> logger)
        {
            _imageMetadataDBContext = imageMetadataDBContext;
            _configuration = configuration;

            var connString = configuration["BlobStorage_ConnectionString"];
            var containerName = configuration["BlobStorage_ContainerName"];

            _containerClient = new BlobContainerClient(connString, containerName);
            _logger = logger;
        }

        /// <summary>
        /// For the newly created subscription table it fills the table with already registered users with all having FREE plan
        /// </summary>
        /// <returns></returns>
        [HttpGet("FillSubscriptionTableWithExistingUsers")]
        [Obsolete]
        public async Task<IActionResult> FillSubscriptionTableWithExistingUsers()
        {
            // grab all users
            List<UserInfo> users = _imageMetadataDBContext.UserInfo.ToList();

            foreach (var user in users)
            {
                _imageMetadataDBContext.SubscriptionDetails.Add(new SubscriptionDetails
                {
                    UserEmail = user.Email,
                    PlanType = PlanType.Free,
                    SubscriptionType = SubscriptionType.Free,
                });

                await _imageMetadataDBContext.SaveChangesAsync();
            }
            return Ok($"You have {users.Count} Users");
        }

        [HttpGet("CalculateUsedStorage")]
        public async Task<IActionResult> CalculateUsedStorage()
        {
            // grab all users
            List<UserInfo> users = _imageMetadataDBContext.UserInfo.ToList();
            List<SubscriptionDetails> subscriptionDetails = _imageMetadataDBContext.SubscriptionDetails.ToList();

            foreach (var user in users)
            {
                long totalBytes = 0;
                await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync(prefix: $"{user.Email}/"))
                {
                    totalBytes += blobItem.Properties.ContentLength ?? 0;
                }

                float storageInGB = totalBytes / (1024f * 1024f * 1024f);

                // update subscription details
                subscriptionDetails.Where(x => x.UserEmail == user.Email).FirstOrDefault().StorageUsedInGB = storageInGB;
            }

            await _imageMetadataDBContext.SaveChangesAsync();
            return Ok();
        }
    }
}
