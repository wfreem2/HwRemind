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

        public static int GetLoginIdFromClaim(this HttpContext context)
        {
            var claims = GetClaims(context);
            var loginId = claims.Where(c => c.Type.Equals("id")).FirstOrDefault();

            return int.Parse(loginId.Value);
        }

        public static int GetUserIdFromClaim(this HttpContext context)
        {
            var claims = GetClaims(context);
            var userId = claims.Where(c => c.Type.Equals("userId")).FirstOrDefault();

            return int.Parse(userId.Value);
        }

    }
}
