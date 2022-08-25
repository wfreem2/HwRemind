using HwRemind.Contexts;
using HwRemind.Api.Endpoints.Assignments.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

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

        public async Task<bool> DeleteAssignment(int id)
        {
            var assignment = await _assignments.FindAsync(id);

            if(assignment == null) { return false; }

            _assignments.Remove(assignment);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Assignment>> GetAllByUserId(int id)
        {
            return await _assignments.Where(a => a.id == id).ToListAsync();
        }

        public async Task<Assignment> GetById(int id)
        {
            return await _assignments.FindAsync(id);
        }

        public async Task<bool> UpdateAssignment(int id, Assignment assignment)
        {
            var existingAssignment = await _assignments.FindAsync(id);
            if(existingAssignment == null) { return false; }

            assignment.id = existingAssignment.id;
            existingAssignment = assignment;

            _dbContext.Entry(existingAssignment).State = EntityState.Modified;
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
