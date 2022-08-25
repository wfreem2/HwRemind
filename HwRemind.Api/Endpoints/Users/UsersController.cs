using HwRemind.Api.Endpoints.Users.Models;
using HwRemind.Api.Endpoints.Users.Repositories;
using HwRemind.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HwRemind.Api.Endpoints.Users
{
    [Authorize]
    [ApiController]
    [Route("api/users")]

    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpPost, Route("add")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            _logger.LogInformation($"Adding user {user.firstName}");


            var claims = HttpContext.GetClaims();
            var loginIdClaim = claims.Where(c => c.Type.Equals("id")).FirstOrDefault();

            if(loginIdClaim == null) { return BadRequest(); }

            var loginId = int.Parse(loginIdClaim.Value);

            var existingUser = await _userRepository.GetUserByLoginId(loginId);
            if(existingUser != null) { return BadRequest(); }

            user.loginId = loginId;
            await _userRepository.AddUser(user);

            return Ok(new { id = user.id });
        }
    }
}
