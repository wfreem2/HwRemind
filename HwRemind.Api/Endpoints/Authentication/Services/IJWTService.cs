using HwRemind.Endpoints.Authentication.Models;
using Microsoft.IdentityModel.Tokens;

namespace HwRemind.Endpoints.Authentication.Services
{
    public interface IJWTService
    {
        public Task<string> GenerateAccessToken(int loginId);
        public Task<string> GenerateAccessToken(int loginId, int userId);
        public Task<RefreshToken> GenerateRefreshToken(int loginId); 
        public Task<bool> IsTokenExpired(string token);
        public Task<bool> IsExpiredAccessTokenValid(string token);  

        public TokenValidationParameters TokenValidationParams { get; }

    }
}
