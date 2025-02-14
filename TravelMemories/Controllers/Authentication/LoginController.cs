using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using TravelMemories.Contracts.Data;
using TravelMemories.Database;

namespace TravelMemories.Controllers.Authentication
{
    [AllowAnonymous]
    [ApiController]
    [Route("auth")]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;
        private ImageMetadataDBContext _imageMetadataDbContext;

        public LoginController(IConfiguration configuration, ImageMetadataDBContext imageMetadataDBContext)
        {
            _configuration = configuration;
            _imageMetadataDbContext = imageMetadataDBContext;
        }

        [HttpPost]
        [Route("googleLogin")]
        public async Task<ActionResult> HandleLoginWithGoogle(HandleWithGoogleRequest request)
        {
            var payload = await ValidateGoogleTokenV2(request.idToken);
            string userName = payload.GetValue("name").ToString();
            string userEmail = payload.GetValue("email").ToString();
            string pictureURL = payload.GetValue("picture").ToString();

            // check if is an existing user
            UserInfo maybeUser = _imageMetadataDbContext.UserInfo.Where(user => user.Email == userEmail).FirstOrDefault();

            if (maybeUser != null)
            {
            }
            else
            {
                _imageMetadataDbContext.UserInfo.Add(new UserInfo
                {
                    Name = userName,
                    Email = userEmail,
                });

                await _imageMetadataDbContext.SaveChangesAsync();
            }

            GenerateJWTToken(new JWTInputs
            {
                Name = userName,
                Email = userEmail,
            });

            return Ok(new LoginResponse
            {
                UserName = userName,
                IsError = false,
            });
        }

        [NonAction]
        private void GenerateJWTToken(JWTInputs jwtInputs)
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
                Expires = DateTime.Now.AddMinutes(15),
                SigningCredentials = new(new SymmetricSecurityKey(signingKeySecretText), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var generatedToken = tokenHandler.WriteToken(token);

            HttpContext.Response.Cookies.Append("token", generatedToken, new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(15),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None,
            });
        }

        [NonAction]
        private async Task<JObject> ValidateGoogleTokenV2(string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Cannot reach googleapis");
                }

                // Parse the JSON response to extract user info
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var userInfo = JObject.Parse(jsonResponse);

                return userInfo;
            }
        }
    }

    public class HandleWithGoogleRequest
    {
        public string idToken { get; set; }
    }

    public class JWTInputs
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class LoginResponse
    {
        public string UserName { get; set; }

        public bool IsError { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
