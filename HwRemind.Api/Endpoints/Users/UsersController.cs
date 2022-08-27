using HwRemind.Api.Configs;
using HwRemind.Api.Endpoints.Users.Models;
using HwRemind.Api.Endpoints.Users.Repositories;
using HwRemind.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HwRemind.Api.Endpoints.Users
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Policy = PolicyNames.LoginRequired)]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [Authorize(Policy = PolicyNames.UserAndLoginRequired)]
        [HttpGet, Route("me")]
        public async Task<IActionResult> GetUser()
        {
            var loginId = HttpContext.GetLoginIdFromClaim();

            var user = await _userRepository.GetUserByLoginId(loginId);

            return user != null ? Ok(new BaseUser(user)) : NotFound();
        }

        [HttpPost, Route("add")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            _logger.LogInformation($"Adding user {user.firstName}");

            var loginId = HttpContext.GetLoginIdFromClaim();

            var existingUser = await _userRepository.GetUserByLoginId(loginId);
            if (existingUser != null) { return BadRequest(); }

            user.loginId = loginId;

            await _userRepository.AddUser(user);

            return Ok(new { id = user.id });
        }


        [Authorize(Policy = PolicyNames.UserAndLoginRequired)]
        [HttpDelete, Route("")]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = HttpContext.GetUserIdFromClaim();

            _logger.LogInformation($"Deleting user: {userId}");

            var isDeleted = await _userRepository.DeleteUser(userId);

            _logger.LogInformation($"User deletion successful: {isDeleted}");

            return NoContent();
        }

        [Authorize(Policy = PolicyNames.UserAndLoginRequired)]
        [HttpPut, Route("")]
        public async Task<IActionResult> UpdateUser([FromBody] User updatedUser)
        {
            var userId = HttpContext.GetUserIdFromClaim();

            _logger.LogInformation($"Updating user: {userId}");

            var isUpdated = await _userRepository.UpdateUser(userId, updatedUser);

            _logger.LogInformation($"User update successful: {isUpdated}");

            return NoContent();
        }

        [Authorize(Policy = PolicyNames.UserAndLoginRequired)]
        [HttpPatch, Route("")]
        public async Task<IActionResult> UpdateUser([FromBody] JsonPatchDocument<User> updatedUser)
        {
            var userId = HttpContext.GetUserIdFromClaim();

            _logger.LogInformation($"Patching user: {userId}");

            var isUpdated = await _userRepository.UpdateUser(userId, updatedUser);

            _logger.LogInformation($"User patch successful: {isUpdated}");

            return NoContent();
        }
    }
}
