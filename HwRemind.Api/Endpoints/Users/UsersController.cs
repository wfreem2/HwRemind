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


        [HttpPost, Route("add")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            _logger.LogInformation($"Adding user {user.firstName}");

            var loginId = HttpContext.GetLoginId();

            var existingUser = await _userRepository.GetUserByLoginId(loginId);
            if (existingUser != null) { return BadRequest(); }

            user.loginId = loginId;

            await _userRepository.AddUser(user);

            return Ok(new { id = user.id });
        }


        [HttpGet, Route("me")]
        public async Task<IActionResult> GetUser()
        {
            var loginId = HttpContext.GetLoginId();

            var user = await _userRepository.GetUserByLoginId(loginId);

            return user != null ? Ok(new BaseUser(user)) : NotFound();
        }


        [HttpDelete, Route("")]
        public async Task<IActionResult> DeleteUser()
        {
            var loginId = HttpContext.GetLoginId();

            _logger.LogInformation($"Deleting user wit loginId: {loginId}");

            var isDeleted = await _userRepository.DeleteUser(loginId);

            _logger.LogInformation($"User deletion successful: {isDeleted}");

            return NoContent();
        }

        [HttpPut, Route("")]
        public async Task<IActionResult> UpdateUser([FromBody] User updatedUser)
        {
            var loginId = HttpContext.GetLoginId();

            _logger.LogInformation($"Updating user with loginId: {loginId}");

            var isUpdated = await _userRepository.UpdateUser(loginId, updatedUser);

            _logger.LogInformation($"User update successful: {isUpdated}");

            return NoContent();
        }

        [HttpPatch, Route("")]
        public async Task<IActionResult> UpdateUser([FromBody] JsonPatchDocument<User> updatedUser)
        {
            var loginId = HttpContext.GetLoginId();

            _logger.LogInformation($"Patching user with loginId: {loginId}");

            var isUpdated = await _userRepository.UpdateUser(loginId, updatedUser);

            _logger.LogInformation($"User patch successful: {isUpdated}");

            return NoContent();
        }
    }
}
