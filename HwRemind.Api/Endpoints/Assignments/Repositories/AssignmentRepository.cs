using HwRemind.Contexts;
using HwRemind.Api.Endpoints.Assignments.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using HwRemind.Api.Gloabl_Services.Models;
using HwRemind.Endpoints.Authentication.Models;
using HwRemind.Api.Endpoints.Users.Models;

namespace HwRemind.Api.Endpoints.Assignments.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {

        private readonly DbSet<Assignment> _assignments;
        private readonly DbSet<User> _users;
        private readonly DBContext _dbContext;

        public AssignmentRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
            _users = dbContext.Users;
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

        public async Task<IEnumerable<Assignment>> GetAllByLoginId(int id, PageFilter filter)
        {
            filter.TotalRecords = await (
                             from a in _assignments
                             join u in _users on id equals u.loginId
                             select a
                           )
                           .CountAsync();

            return await (
                        from a in _assignments
                        join u in _users on id equals u.loginId
                        select a
                    )
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
