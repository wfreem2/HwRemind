using HwRemind.Api.Endpoints.Assignments.Models;
using HwRemind.Api.Gloabl_Services.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HwRemind.Api.Endpoints.Assignments.Repositories
{
    public interface IAssignmentRepository
    {
        public Task<Assignment> GetById(int id);
        public Task<IEnumerable<Assignment>> GetByUserId(int id, PageFilter filter);

        public Task AddAssignment(Assignment assignment);
        public Task<bool> DeleteAssignment(int id, int loginId);
        public Task<bool> UpdateAssignment(int id, Assignment assignment);
        public Task<bool> UpdateAssignment(int id, JsonPatchDocument<Assignment> assignment);
    }
}
