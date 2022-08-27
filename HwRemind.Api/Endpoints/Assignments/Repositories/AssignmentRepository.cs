using HwRemind.Contexts;
using HwRemind.Api.Endpoints.Assignments.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using HwRemind.Api.Gloabl_Services.Models;

namespace HwRemind.Api.Endpoints.Assignments.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {

        private readonly DbSet<Assignment> _assignments;
        private readonly DBContext _dbContext;

        public AssignmentRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
            _assignments = dbContext.Assignments;
        }

        public async Task AddAssignment(Assignment assignment)
        {
            await _assignments.AddAsync(assignment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteAssignment(int id, int userId)
        {
            var assignment = await _assignments.Where(a => a.userId == userId).FirstOrDefaultAsync();
            if(assignment == null) { return false; }

            _assignments.Remove(assignment);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Assignment>> GetByUserId(int userId, PageFilter filter)
        {
            filter.TotalRecords = await _assignments.Where(a => a.userId == userId).CountAsync();

           return await 
                _assignments
                .Where(a => a.userId == userId)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .OrderBy(a => a.name)
                .ToListAsync();
        }

        public async Task<Assignment> GetById(int id)
        {
            return await _assignments.FindAsync(id);
        }

        public async Task<bool> UpdateAssignment(int id, Assignment assignment)
        {
            var existingAssignment = await _assignments.FindAsync(id);
            if(existingAssignment == null) { return false; }

            existingAssignment.description = assignment.description;
            existingAssignment.name = assignment.name;
            existingAssignment.dueAt = assignment.dueAt;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAssignment(int id, JsonPatchDocument<Assignment> assignment)
        {
            var existingAssignment = await _assignments.FindAsync(id);
            if (existingAssignment == null) { return false; }

            assignment.ApplyTo(existingAssignment);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
