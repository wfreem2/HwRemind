using HwRemind.Api.Endpoints.Users.Models;
using HwRemind.Contexts;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace HwRemind.Api.Endpoints.Users.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DbSet<User> _users;
        private readonly DBContext _dbContext;

        public UserRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
            _users = dbContext.Users;
        }

        public async Task AddUser(User user)
        {
            await _users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteUser(int id)
        {
            var user = await GetUserByLoginId(id);
            if(user == null) { return false; }

            _users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<User> GetUserById(int id)
        {
            return await _users.FindAsync(id);
        }

        public async Task<User> GetUserByLoginEmail(string email)
        {
            return await 
                (
                 from u in _dbContext.Users
                 join l in _dbContext.Logins on u.loginId equals l.id
                 where l.email.Equals(email)
                 select u
                )
                .FirstOrDefaultAsync();

        }

        public async Task<User> GetUserByLoginId(int id)
        {
            return await _users.Where(u => u.loginId == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateUser(int id, User user)
        {
            var existingUser = await GetUserByLoginId(id);
            if(existingUser == null) { return false; }

            existingUser.firstName = user.firstName;
            existingUser.lastName = user.lastName;
            existingUser.schoolName = user.schoolName;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateUser(int id, JsonPatchDocument<User> user)
        {
            var existingUser = await GetUserByLoginId(id);
            if (existingUser == null) { return false; }

            user.ApplyTo(existingUser);

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
