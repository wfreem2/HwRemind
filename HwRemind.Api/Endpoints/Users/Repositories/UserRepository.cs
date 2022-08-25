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
            var user = await _users.FindAsync(id);
            if(user == null) { return false; }

            _users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<User> GetUserById(int id)
        {
            return await _users.FindAsync(id);
        }

        public async Task<User> GetUserByLoginId(int id)
        {
            return await _users.Where(u => u.loginId == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateUser(User user)
        {
            var existingUser = await _users.FindAsync(user.id);
            if(existingUser == null) { return false; }

            user.id = existingUser.id;
            existingUser = user;

            _dbContext.Entry(existingUser).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateUser(int id, JsonPatchDocument<User> user)
        {
            var existingUser = await _users.FindAsync(id);
            if (existingUser == null) { return false; }

            user.ApplyTo(existingUser);

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
