using HwRemind.Api.Endpoints.Users.Models;
using HwRemind.Api.Endpoints.Users.Repositories;
using HwRemind.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HwRemind.Api.Endpoints.Users
{
    [Authorize(Policy = "LoginRequired")]
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

        [HttpGet, Route("me")]
        public async Task<IActionResult> GetUser()
        {
            var loginId = GetLoginIdFromClaim();

            var user = await _userRepository.GetUserByLoginId(loginId);

            return user != null ? 
            Ok(new BaseUser
                {
                   id = user.id,
                   firstName = user.firstName,
                   lastName = user.lastName,
                   schoolName = user.schoolName
                }
            ) 
            : BadRequest();
        }

        [HttpPost, Route("add")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            _logger.LogInformation($"Adding user {user.firstName}");

            var loginId = GetLoginIdFromClaim();

            var existingUser = await _userRepository.GetUserByLoginId(loginId);
            if(existingUser != null) { return BadRequest(); }

            user.loginId = loginId;

            await _userRepository.AddUser(user);

            return Ok(new { id = user.id });
        }

        [HttpDelete, Route("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation($"Deleting user: {id}");

            var isAuthorized = await IsUserAuthorized(id);
            if(isAuthorized != null) { return isAuthorized; }

            await _userRepository.DeleteUser(id);

            _logger.LogInformation($"User deletion successful");

            return NoContent();
        }

        [HttpPut, Route("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            _logger.LogInformation($"Updating user: {id}");

            var isAuthorized = await IsUserAuthorized(id);
            if(isAuthorized != null) { return isAuthorized; }

            await _userRepository.UpdateUser(id, updatedUser);

            _logger.LogInformation($"User update successful");

            return NoContent();
        }
        
        [HttpPatch, Route("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] JsonPatchDocument<User> updatedUser)
        {
            _logger.LogInformation($"Patching user: {id}");

            var isAuthorized = await IsUserAuthorized(id);
            if(isAuthorized != null) { return isAuthorized; }

            await _userRepository.UpdateUser(id, updatedUser);

            _logger.LogInformation($"User patch successful");

            return NoContent();
        }

        private int GetLoginIdFromClaim()
        {
            var claims = HttpContext.GetClaims();
            var loginId = claims.Where(c => c.Type.Equals("id")).FirstOrDefault();

            return int.Parse(loginId.Value);
        }

        private async Task<IActionResult> IsUserAuthorized(int id)
        {
            var loginId = GetLoginIdFromClaim();
            var user = await _userRepository.GetUserById(id);

            if (user == null) { return BadRequest(); }
            if (user.loginId != loginId) { return Unauthorized(); }

            return null;
        }
    }
}
