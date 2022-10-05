﻿using HwRemind.Api.Configs;
using HwRemind.Api.Endpoints.Authentication.Filters;
using HwRemind.Api.Endpoints.Users.Repositories;
using HwRemind.API.Endpoints.Authentication.Services;
using HwRemind.Endpoints.Authentication.Models;
using HwRemind.Endpoints.Authentication.Repositories;
using HwRemind.Endpoints.Authentication.Services;
using HwRemind.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HwRemind.Endpoints.Authentication
{
    [ApiController]
    [Route("api/authenticate")]
    
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IJWTService _jwtService;
        private readonly IAuthRepository _authRepo;
        private readonly IUserRepository _userRepo;
        private readonly IDistributedCache _cache;


        public AuthController(ILogger<AuthController> logger, IJWTService JWTService, IAuthRepository authRepository, 
            IUserRepository userRepository, IDistributedCache cache)
        {
            _cache = cache;
            _logger = logger;
            _jwtService = JWTService;
            _authRepo = authRepository;
            _userRepo = userRepository;
        }

        [HttpPost, Route("")]
        public async Task<IActionResult> Login([FromBody] BaseLogin login,
            [FromServices] IPasswordService pswdService)
        {

            var existingLogin = await _authRepo.GetLoginByEmail(login.email);
            if(existingLogin == null) { return BadRequest(); }

            var isPswdVerified = pswdService.IsMatch(login.password, existingLogin.hashedPassword, existingLogin.salt);
            if (!isPswdVerified) { return Unauthorized(); }

            //If there is an existing user generate token with login ID claim 
            var accessToken = await _jwtService.GenerateAccessToken(existingLogin.id);
            var refreshToken = await _jwtService.GenerateRefreshToken(existingLogin.id);

            await _authRepo.AddOrUpdateRefreshToken(refreshToken);

            _logger.LogInformation("Generating JWT");

            return Ok(new AuthenticationRequest { refreshToken = refreshToken.ToString(), accessToken = accessToken });
        }


        [ExpiredTokenWithLoginAtrribute]
        [HttpPost, Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
        {
            _logger.LogInformation("Refreshing JWT");

            var existingRefreshToken = await _authRepo.GetRefreshToken(token: refreshRequest.refreshToken);

            //If refresh token is expired or the token does not exist, client needs to login
            if(existingRefreshToken == null || existingRefreshToken.expiration < DateTime.Now)
            { return BadRequest(); }

            //Rotate refresh token
            var rotatedRefreshToken = await _jwtService.GenerateRefreshToken(existingRefreshToken.loginId);

            //If the refresh request came from login with user, add userid and loginid claim else just loginid
            var accessToken = existingRefreshToken.userId == null ?
                await _jwtService.GenerateAccessToken(existingRefreshToken.loginId) :
                await _jwtService.GenerateAccessToken(existingRefreshToken.loginId, (int) existingRefreshToken.userId);

            await _authRepo.AddOrUpdateRefreshToken(rotatedRefreshToken);
            
            return Ok(new AuthenticationRequest { accessToken = accessToken, refreshToken = rotatedRefreshToken.ToString() });
        }

        [HttpPost, Route("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest revokeRequest)
        {
            _logger.LogInformation("Revoking JWT");

            if (string.IsNullOrEmpty(revokeRequest.token)) { return BadRequest();  }

            var types = new string[] { "id" };

            var tokenParams = _jwtService.TokenValidationParams;
                tokenParams.ValidateLifetime = true;

            var isValid = await _jwtService.ValidateToken(revokeRequest.token, tokenParams, types);

            if (!isValid)
            {
                _logger.LogWarning($"Unable to validate provided token" + revokeRequest.token);
                return BadRequest();
            }

            SecurityToken token;
            var handler = new JwtSecurityTokenHandler();
            token = handler.ReadJwtToken(revokeRequest.token);

            await _cache.SetAsync(
                key: revokeRequest.token, 
                value: Encoding.UTF8.GetBytes(revokeRequest.token), 
                options: GetOptions(token.ValidTo)
            );
            
            return NoContent();
        }
        
        private DistributedCacheEntryOptions GetOptions(DateTime expiration)
        {
            return new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = expiration,
            };
        }

    }

    
}
