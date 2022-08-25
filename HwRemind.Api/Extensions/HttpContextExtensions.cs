using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HwRemind.Extensions
{
    public static class HttpContextExtensions
    {
        private static JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        public static string GetJWT(this HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString();
            
            return token.Split(' ').LastOrDefault();
        }

        public static IEnumerable<Claim> GetClaims(this HttpContext context)
        {
            var token = GetJWT(context);
            var decoded = _tokenHandler.ReadJwtToken(token);

            return decoded.Claims;
        }


    }
}
