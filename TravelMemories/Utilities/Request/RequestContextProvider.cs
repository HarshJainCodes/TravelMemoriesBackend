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
            string JWTTokenFromCookie = _httpContextAccessor.HttpContext.Request.Cookies["travelMemoriestoken"];
            string JWTFromAuthHeader = null;

            var JwtAuthHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(JwtAuthHeader))
            {
                JWTFromAuthHeader = JwtAuthHeader.ToString().Substring("Bearer ".Length);
            }

            if (JWTTokenFromCookie == null && JWTFromAuthHeader == null)
            {
                throw new ArgumentException("This is not a valid request");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(JWTFromAuthHeader ?? JWTTokenFromCookie);

            return jwtToken;
        }
    }

    public interface IRequestContextProvider
    {
        public JwtSecurityToken GetJWTToken();
    }
}
