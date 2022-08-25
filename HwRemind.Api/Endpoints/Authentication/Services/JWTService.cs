using HwRemind.Configs;
using HwRemind.Endpoints.Authentication.Models;
using IdentityModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HwRemind.Endpoints.Authentication.Services
{
    public class JWTService : IJWTService
    {
        private readonly JWTConfig _jwtConfig;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JWTService(IOptions<JWTConfig> config)
        {
            _jwtConfig = config.Value;
            _tokenHandler = new JwtSecurityTokenHandler();
        }


        public Task<string> GenerateAccessToken(int loginId)
        {
            var claims = new Claim[]
            {
                new Claim("id", loginId.ToString()),
                new Claim(JwtClaimTypes.IssuedAt, DateTime.UtcNow.ToEpochTime().ToString()),
            };

            var credentials = GetSigningCredentials();

            var tokenOptions = new JwtSecurityToken(
                claims: claims,
                issuer: _jwtConfig.iss,
                audience: _jwtConfig.aud,
                signingCredentials: credentials,
                expires: DateTime.Now.AddMinutes(_jwtConfig.exp)
            );

            return Task.FromResult(_tokenHandler.WriteToken(tokenOptions));
        }

        public Task<RefreshToken> GenerateRefreshToken(int loginId)
        {
            var randomNumber = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var refreshToken = new RefreshToken
            {
                loginId = loginId,
                token = Convert.ToBase64String(randomNumber),
                expiration = GetRefreshTokenExpiration()
            };

            return Task.FromResult(refreshToken);
        }

        public Task<bool> IsExpiredAccessTokenValid(string token)
        {
            try
            {
                SecurityToken validToken = null;

                var validationParams = TokenValidationParams;
                _tokenHandler.ValidateToken(token, validationParams, out validToken);

                return Task.FromResult(validToken != null);
            }
            catch (SecurityTokenException)
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> IsTokenExpired(string token)
        {
            var validToken = _tokenHandler.ReadJwtToken(token);

            if (validToken == null) { return Task.FromResult(false); }

            return Task.FromResult(validToken.ValidTo < DateTime.UtcNow);
        }


        private DateTime GetRefreshTokenExpiration() => DateTime.UtcNow.AddDays(_jwtConfig.refreshExp);
        private SigningCredentials GetSigningCredentials() => new SigningCredentials(GetSignature(), _jwtConfig.alg);
        private SymmetricSecurityKey GetSignature() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.secret));
        public TokenValidationParameters TokenValidationParams
        {
            get
            {

                return new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateLifetime = false, //For refreshing expired access tokens
                    ValidateAudience = true,

                    ValidIssuer = _jwtConfig.iss,
                    ValidAudience = _jwtConfig.aud,
                    ValidAlgorithms = new[] { _jwtConfig.alg },

                    IssuerSigningKey = GetSignature(),
                };
            }

        }
    }
}
