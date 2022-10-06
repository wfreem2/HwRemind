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
            
            if (string.IsNullOrEmpty(token)) { return Enumerable.Empty<Claim>(); }

            var decoded = _tokenHandler.ReadJwtToken(token);

            return decoded.Claims;
        }

        public static int GetLoginId(this HttpContext context)
        {
            return getClaim("id", context);
        }

        public static int GetUserId(this HttpContext context)
        {
            return getClaim("userId", context);
        }

        private static int getClaim(string claimType, HttpContext context)
        {
            var claims = GetClaims(context);
            var claim = claims.Where(c => c.Type.Equals(claimType)).FirstOrDefault();

            int value = -1;

            int.TryParse(claim.Value, out value);

            return value;
        }
    }
}
