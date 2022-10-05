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


        public async Task<string> GenerateAccessToken(int loginId)
        {
            var claims = new Claim[]
            {
                new Claim("id", loginId.ToString()),
                new Claim(JwtClaimTypes.IssuedAt, DateTime.UtcNow.ToEpochTime().ToString()),
            };

            return await generateAccessToken(claims);
        }

        public async Task<string> GenerateAccessToken(int loginId, int userId)
        {
            var claims = new Claim[]
            {
                new Claim("id", loginId.ToString()),
                new Claim("userId", userId.ToString()),
                new Claim(JwtClaimTypes.IssuedAt, DateTime.UtcNow.ToEpochTime().ToString()),
            };

            return await generateAccessToken(claims);
        }

        private Task<string> generateAccessToken(Claim[] claims)
        {
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
                var claims = _tokenHandler.ValidateToken(token, validationParams, out validToken);

                if(validToken != null)
                {

                    var claim = claims.Claims.Where(c => c.Type.Equals("id")).FirstOrDefault();

                    return Task.FromResult(claim != null);
                }

                return Task.FromResult(false);
            }
            catch (Exception)
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

        public Task<bool> ValidateToken(string token, TokenValidationParameters validationParams = null, IEnumerable<string> claimTypes = null)
        {
            if(validationParams == null) { validationParams = TokenValidationParams; }
            if(claimTypes == null) { claimTypes = Enumerable.Empty<string>(); }

            try
            {
                SecurityToken validToken = null;

                var principal = _tokenHandler.ValidateToken(token, validationParams, out validToken);

                //Check if all the required claims are present
                foreach(string claim in claimTypes)
                {
                    if (!principal.Claims.Any(c => c.Type.Equals(claim)))
                    {
                        return Task.FromResult(false);
                    } 
                }

                return Task.FromResult(validToken != null);
            }
            catch (SecurityTokenException)
            {
                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

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
