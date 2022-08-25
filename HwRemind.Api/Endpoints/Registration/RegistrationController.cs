using Microsoft.AspNetCore.Mvc;
using HwRemind.Endpoints.Authentication.Models;
using HwRemind.Endpoints.Authentication.Repositories;
using HwRemind.API.Endpoints.Authentication.Services;
using HwRemind.Endpoints.Authentication.Services;

namespace HwRemind.Endpoints.Registration
{
    [ApiController]
    [Route("api/register")]

    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly IAuthRepository _authRepo;
        private readonly IPasswordService _pswdService;
        private readonly IJWTService _jwtService;

        public RegistrationController(ILogger<RegistrationController> logger, IAuthRepository authRepo,
            IPasswordService pswdService, IJWTService jwtService)
        {
            _logger = logger;
            _authRepo = authRepo;
            _jwtService = jwtService;
            _pswdService = pswdService;
        }


        [HttpPost, Route("")]

        public async Task<IActionResult> Register([FromBody] BaseLogin login)
        {
            var existingLogin = await _authRepo.GetLogin(login.email);

            //If account already exists, user needs to login
            if(existingLogin != null) { return StatusCode(400); }

            var hashSalt = _pswdService.HashPassword(login.password);

            var newLogin = new Login
            {
                email = login.email,
                password = hashSalt[0],
                salt = hashSalt[1]
            };

            await _authRepo.AddLogin(newLogin);

            _logger.LogInformation("Added login for user with email: " + login.email);

            var accessToken = await _jwtService.GenerateAccessToken(existingLogin.id);
            var refreshToken = await _jwtService.GenerateRefreshToken(existingLogin.id);

            await _authRepo.AddRefreshToken(refreshToken);

            return Ok(new AuthenticationRequest { accesstoken = accessToken, refreshToken = refreshToken.token });
        }


    }
}
