using Azure.Core;
using CommunityToolkit.HighPerformance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TravelMemories.Controllers.Authentication;
using TravelMemories.Database;
using TravelMemoriesBackend.Contracts.Data;

namespace TravelMemories.Controllers.OAuthVerify
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class OAuthVerifyController : ControllerBase
    {
        private ImageMetadataDBContext _imageMetadataDBContext;
        private IConfiguration _configuration;
        private ILogger<OAuthVerifyController> _logger;

        public OAuthVerifyController(ImageMetadataDBContext imageMetadataDBContext, IConfiguration configuration, ILogger<OAuthVerifyController> logger)
        {
            _imageMetadataDBContext = imageMetadataDBContext;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("Token")]
        public async Task<IActionResult> TokenForCodeExchhange([FromForm] TokenForCodeExchangeRequest tokenForCodeExchangeRequest)
        {
            _logger.LogInformation("Exchanging code for token");
            if (tokenForCodeExchangeRequest.grant_type != "authorization_code")
            {
                _logger.LogError("Invalid grant", tokenForCodeExchangeRequest.grant_type);
                return BadRequest("invalid_grant");
            }

            // get the code
            OAuthCodeStore codeStore = _imageMetadataDBContext.OAuthCodeStores.Where(x => x.Code == tokenForCodeExchangeRequest.code).FirstOrDefault();

            if (codeStore == null)
            {
                _logger.LogError("invalid code");
                _logger.LogError("code sent in request" + tokenForCodeExchangeRequest.code);
                _logger.LogError("code verifier sent in request" + tokenForCodeExchangeRequest.code_verifier);
                _logger.LogError("grant types", tokenForCodeExchangeRequest.grant_type);
                _logger.LogError("redirect uri", tokenForCodeExchangeRequest.redirect_uri);
                return BadRequest("invalid_code");
            }

            if (!VerifyPkce(tokenForCodeExchangeRequest.code_verifier, codeStore.LoginChallenge))
            {
                _logger.LogError("pkce verification failed" + tokenForCodeExchangeRequest.code_verifier + codeStore.LoginChallenge);
                return BadRequest("invalid_verifier");
            }

            UserInfo user = _imageMetadataDBContext.UserInfo.Where(x => x.Email == codeStore.Email).FirstOrDefault();
            
            string token = GenerateJWTToken(new JWTInputs
            {
                Name = user.Name,
                Email = user.Email,
            }, HttpContext);

            return Ok(new TokenExchangeResponse
            {
                access_token = token,
                expires_in = 3600,
                token_type = "Bearer"
            });
        }

        [NonAction]
        public string GenerateJWTToken(JWTInputs jwtInputs, HttpContext httpContext)
        {
            var signingKeySecretText = Encoding.ASCII.GetBytes(_configuration["IssuerSigningKeySecretText"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, jwtInputs.Name),
                    new Claim(ClaimTypes.Email, jwtInputs.Email),
                }),
                Expires = DateTime.Now.AddDays(20f),
                SigningCredentials = new(new SymmetricSecurityKey(signingKeySecretText), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var generatedToken = tokenHandler.WriteToken(token);

            return generatedToken;
            //httpContext.Response.Cookies.Append("travelMemoriestoken", generatedToken, new CookieOptions
            //{
            //    Expires = DateTime.Now.AddDays(1f),
            //    HttpOnly = true,
            //    Secure = true,
            //    IsEssential = true,
            //    SameSite = SameSiteMode.None,
            //    Domain = _configuration["CookiesDomain"],
            //});
        }

        [NonAction]
        private bool VerifyPkce(string verifier, string storedChallenge)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(verifier));

            var challenge = Base64UrlEncode(hash);

            return challenge == storedChallenge;
        }

        private string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }

    public class TokenExchangeResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class TokenForCodeExchangeRequest
    {
        public string code { get; set; }

        public string grant_type { get; set; }

        public string redirect_uri { get; set; }

        public string client_id { get; set; }

        public string code_verifier { get; set; }
    }
}
