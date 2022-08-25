using HwRemind.Api.Endpoints.Assignments.Models;
using HwRemind.Api.Endpoints.Assignments.Repositories;
using HwRemind.Api.Endpoints.Users.Repositories;
using HwRemind.Api.Gloabl_Services;
using HwRemind.Api.Gloabl_Services.Models;
using HwRemind.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HwRemind.Api.Endpoints.Assignments
{
    [Authorize(Policy = "LoginRequired")]
    [ApiController]
    [Route("api/assignments")]

    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ILogger<AssignmentsController> _logger;
        private readonly IUserRepository _userRepository;

        public AssignmentsController(IAssignmentRepository assignmentRepo, ILogger<AssignmentsController> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _assignmentRepo = assignmentRepo;
            _userRepository = userRepository;
        }

        [HttpGet, Route("me")]
        public async Task<IActionResult> GetAssignments([FromQuery] PageFilter filter, 
            [FromServices] IUriService uriService)
        {
            var loginId = GetLoginIdFromClaim();
           /* var user = await _userRepository.GetUserByLoginId(loginId);

            if (user == null) { return BadRequest(); }*/
            //if(loginId != user.loginId) { return Unauthorized(); }

            var assignments = await _assignmentRepo.GetAllByLoginId(loginId, filter);

            var route = Request.Path.Value;
            var pagedResult = new 
                PagedResult<Assignment>(
                pagedData: assignments, 
                filter: filter, 
                totalRecords: filter.TotalRecords, 
                uriService: uriService, 
                route: route);

            return Ok(pagedResult);
        }

        [HttpPost, Route("add")]

        public async Task<IActionResult> AddAssignment([FromBody] Assignment assignment)
        {
            _logger.LogInformation($"Adding assignment: {assignment.name}");

            await _assignmentRepo.AddAssignment(assignment);

            return NoContent();
        }

        private int GetLoginIdFromClaim()
        {
            var claims = HttpContext.GetClaims();
            var loginId = claims.Where(c => c.Type.Equals("id")).FirstOrDefault();

            return int.Parse(loginId.Value);
        }

    }
}
