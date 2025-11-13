using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TravelMemories.Database;
using TravelMemories.Utilities.Request;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Controllers.Subscription
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ILogger<SubscriptionController> _logger;
        private readonly IRequestContextProvider _requestContextProvider;
        private readonly ImageMetadataDBContext _imageMetadataDBContext;

        public SubscriptionController(
            ILogger<SubscriptionController> logger,
            IRequestContextProvider requestContextProvider,
            ImageMetadataDBContext imageMetadataDBContext)
        {
            _logger = logger;
            _requestContextProvider = requestContextProvider;
            _imageMetadataDBContext = imageMetadataDBContext;
        }

        [HttpGet("GetSubscriptionDetails")]
        public async Task<SubscriptionDetails> GetSubscriptionDetails()
        {
            JwtSecurityToken jwtToken = _requestContextProvider.GetJWTToken();
            string email = jwtToken.Claims.Where(c => c.Type == "email").First().Value;

            SubscriptionDetails subscriptionDetails = _imageMetadataDBContext.SubscriptionDetails.Where(s => s.UserEmail == email).FirstOrDefault();
            return subscriptionDetails;
        }

        [HttpPost]
        [Route("UpdateSubscription/AdminBypass")]
        public async Task<IActionResult> UpdateSubscription([FromBody] ActivateSubscriptionDTO activateSubscriptionDTO)
        {
            // we believe that the user has made the payment and we will activate the subscription
            JwtSecurityToken jwtToken = _requestContextProvider.GetJWTToken();
            string email = jwtToken.Claims.Where(c => c.Type == "email").First().Value;

            SubscriptionDetails subscriptionDetails = _imageMetadataDBContext.SubscriptionDetails.Where(s => s.UserEmail == email).FirstOrDefault();

            subscriptionDetails.PlanType = activateSubscriptionDTO.PlanType;
            subscriptionDetails.SubscriptionType = activateSubscriptionDTO.SubscriptionType;

            if (activateSubscriptionDTO.SubscriptionType == SubscriptionType.Basic)
            {
                subscriptionDetails.StorageCapacityInGB = 30;
            } else if (activateSubscriptionDTO.SubscriptionType == SubscriptionType.Pro)
            {
                subscriptionDetails.StorageCapacityInGB = 100;
            }

            if (activateSubscriptionDTO.PlanType == PlanType.Monthly)
            {
                subscriptionDetails.PlanStartDate = DateTime.UtcNow;
                subscriptionDetails.PlanEndDate = DateTime.UtcNow.AddMonths(1);
            } else if (activateSubscriptionDTO.PlanType == PlanType.Yearly)
            {
                subscriptionDetails.PlanStartDate = DateTime.UtcNow;
                subscriptionDetails.PlanEndDate = DateTime.UtcNow.AddYears(1);
            }

            await _imageMetadataDBContext.SaveChangesAsync();

            return Ok();
        }
    }

    public class ActivateSubscriptionDTO
    {
        public SubscriptionType SubscriptionType { get; set; }

        public PlanType PlanType { get; set; }

    }
}
