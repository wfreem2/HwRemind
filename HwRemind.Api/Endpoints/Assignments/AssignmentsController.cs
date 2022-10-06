using HwRemind.Api.Configs;
using HwRemind.Api.Endpoints.Assignments.Models;
using HwRemind.Api.Endpoints.Assignments.Repositories;
using HwRemind.Api.Gloabl_Services;
using HwRemind.Api.Gloabl_Services.Models;
using HwRemind.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HwRemind.Api.Endpoints.Assignments
{
    [ApiController]
    [Route("api/assignments")]
    [Authorize(Policy = PolicyNames.UserAndLoginRequired)]

    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ILogger<AssignmentsController> _logger;

        public AssignmentsController(IAssignmentRepository assignmentRepo, ILogger<AssignmentsController> logger)
        {
            _logger = logger;
            _assignmentRepo = assignmentRepo;
        }

        [HttpGet, Route("me")]
        public async Task<IActionResult> GetAssignments([FromQuery] PageFilter filter, 
            [FromServices] IUriService uriService)
        {
            var userId = HttpContext.GetUserId();

            _logger.LogInformation($"Getting assignments from user: {userId}");

            var assignments = await _assignmentRepo.GetByUserId(userId, filter);

            if (!assignments.Any()) { return NoContent(); }

            var pagedResult = new 
                PagedResult<BaseAssignment>(
                pagedData: assignments, 
                filter: filter, 
                totalRecords: filter.TotalRecords, 
                uriService: uriService, 
                route: Request.Path.Value);

            return Ok(pagedResult);
        }

        [HttpPost, Route("add")]

        public async Task<IActionResult> AddAssignment([FromBody] Assignment assignment)
        {
            _logger.LogInformation($"Adding assignment: {assignment.name}");

            await _assignmentRepo.AddAssignment(assignment);

            return Ok(new { id = assignment.id });
        }


        [HttpDelete, Route("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            _logger.LogInformation($"Deleting assignment: {id}");

            var userId = HttpContext.GetUserId();
            var isDeleted = await _assignmentRepo.DeleteAssignment(id, userId);

            _logger.LogInformation($"Assignment deletion successful: {isDeleted}");

            return isDeleted ? NoContent() : BadRequest();
        }
      
    }
}
