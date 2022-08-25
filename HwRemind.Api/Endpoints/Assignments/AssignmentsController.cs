using HwRemind.Api.Endpoints.Assignments.Models;
using HwRemind.Api.Endpoints.Assignments.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HwRemind.Api.Endpoints.Assignments
{
    [Authorize]
    [ApiController]
    [Route("api/assignments")]

    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ILogger<AssignmentsController> _logger;

        public AssignmentsController(IAssignmentRepository assignmentRepo, ILogger<AssignmentsController> logger)
        {
            _assignmentRepo = assignmentRepo;
            _logger = logger;
        }

        [HttpPost, Route("add")]

        public async Task<IActionResult> AddAssignment([FromBody] Assignment assignment)
        {
            _logger.LogInformation($"Adding assignment: {assignment.name}");

            await _assignmentRepo.AddAssignment(assignment);

            return NoContent();
        }
    }
}
