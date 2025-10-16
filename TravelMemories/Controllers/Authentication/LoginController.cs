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
using TravelMemoriesBackend.Contracts.Data;

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
                maybeUser.ProfilePictureURL = pictureURL;   // this will update the profile picture if the user has changed their profile picture on google
                await _imageMetadataDbContext.SaveChangesAsync();
            }
            else
            {
                _imageMetadataDbContext.UserInfo.Add(new UserInfo
                {
                    Name = userName,
                    Email = userEmail,
                    ProfilePictureURL = pictureURL,
                });

                await _imageMetadataDbContext.SaveChangesAsync();
            }

            GenerateJWTToken(new JWTInputs
            {
                Name = userName,
                Email = userEmail,
            }, HttpContext);

            return Ok(new LoginResponse
            {
                UserName = userName,
                Email = userEmail,
                ProfilePictureURL = pictureURL,
                IsError = false,
            });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> HandleManualLogin(ManualLogin request)
        {
            UserInfo user = _imageMetadataDbContext.UserInfo.Where(user => user.Email == request.Email).FirstOrDefault();

            if (user == null)
            {
                return NotFound("User With This Email Does Not Exist");
            }

            if (user.IsManualLogin)
            {
                if (user.Password == request.Password)
                {
                    GenerateJWTToken(new JWTInputs
                    {
                        Name = request.UserName,
                        Email = request.Email,
                    }, HttpContext);

                    return Ok(new LoginResponse
                    {
                        UserName = request.UserName,
                        Email = request.Email,
                        IsError = false,
                    });
                }
                else
                {
                    return BadRequest("Incorrect Email or Password");
                }
            }
            return BadRequest("You Previously signed in with google with this email Id");
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult> HandleManualRegister(ManualLogin request)
        {
            // check if the email already exists
            UserInfo user = _imageMetadataDbContext.UserInfo.Where(user => user.Email == request.Email).FirstOrDefault();
            if (user != null)
            {
                return BadRequest("User With This Email Already Exists");
            }

            _imageMetadataDbContext.UserInfo.Add(new UserInfo
            {
                Email = request.Email,
                Name = request.UserName,
                IsManualLogin = true,
                Password = request.Password,
            });

            await _imageMetadataDbContext.SaveChangesAsync();

            GenerateJWTToken(new JWTInputs
            {
                Name = request.UserName,
                Email = request.Email,
            }, HttpContext);

            return Ok(new LoginResponse
            {
                UserName = request.UserName,
                Email = request.Email,
                IsError = false,
            });
        }

        [NonAction]
        public void GenerateJWTToken(JWTInputs jwtInputs, HttpContext httpContext)
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

            httpContext.Response.Cookies.Append("travelMemoriestoken", generatedToken, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1f),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Domain = _configuration["CookiesDomain"],
            });
        }

        [HttpGet("Logout")]
        public async Task<ActionResult> Logout()
        {
            // this will invalidate the token from the frontend but the users might still be able to make requests with the 'stolen' tokenhhhhhhh
            HttpContext.Response.Cookies.Append("travelMemoriestoken", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Domain = _configuration["CookiesDomain"],
            });

            return Ok();
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

        public string Email { get; set; }

        public bool IsError { get; set; }

        public string ProfilePictureURL { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class ManualLogin
    {
        public string UserName { get; set; }

        public string Email {  set; get; }

        public string Password { set; get; }
    }
}
