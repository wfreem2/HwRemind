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
            var existingLogin = await _authRepo.GetLoginByEmail(login.email);

            //If account already exists, user needs to login
            if(existingLogin != null) { return BadRequest(); }

            var hashWithSalt = _pswdService.HashPassword(login.password);

            var newLogin = new Login
            {
                email = login.email,
                password = hashWithSalt[0],
                salt = hashWithSalt[1]
            };

            newLogin = await _authRepo.AddLogin(newLogin);

            _logger.LogInformation("Added login for user with email: " + login.email);

            var accessToken = await _jwtService.GenerateAccessToken(newLogin.id);
            var refreshToken = await _jwtService.GenerateRefreshToken(newLogin.id);

            await _authRepo.AddOrUpdateRefreshToken(refreshToken);

            return Ok(new AuthenticationRequest { accessToken = accessToken, refreshToken = refreshToken.token });
        }


    }
}
