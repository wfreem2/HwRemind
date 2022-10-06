using HwRemind.Endpoints.Authentication.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace HwRemind.Endpoints.Authentication.Services
{
    public interface IJWTService
    {
        public Task<string> GenerateAccessToken(int loginId);
        public Task<string> GenerateAccessToken(int loginId, int userId);
        public Task<string> GenerateAccessToken(IEnumerable<Claim> claims);
        public Task<RefreshToken> GenerateRefreshToken(int loginId); 

        public Task<bool> IsTokenExpired(string token);
        public Task<bool> IsExpiredAccessTokenValid(string token);
        public Task<bool> ValidateToken(string token, TokenValidationParameters validationParameters = null, IEnumerable<string> claimValueTypes = null);

        public TokenValidationParameters TokenValidationParams { get; }

    }
}
