using System.IdentityModel.Tokens.Jwt;

namespace TravelMemories.Utilities.Request
{
    public class RequestContextProvider : IRequestContextProvider
    {
        IHttpContextAccessor _httpContextAccessor;

        public RequestContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public JwtSecurityToken GetJWTToken()
        {
            string JWTTokenFromCookie = _httpContextAccessor.HttpContext.Request.Cookies["token"];

            if (JWTTokenFromCookie == null)
            {
                throw new ArgumentException("This is not a valid request");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(JWTTokenFromCookie);

            return jwtToken;
        }
    }

    public interface IRequestContextProvider
    {
        public JwtSecurityToken GetJWTToken();
    }
}
