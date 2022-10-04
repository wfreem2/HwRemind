using HwRemind.Api.Endpoints.Users.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HwRemind.Api.Endpoints.Users.Repositories
{
    public interface IUserRepository
    {
        public Task<User> GetUserById(int id);
        public Task<User> GetUserByLoginId(int id);

        public Task<User> GetUserByLoginEmail(string email);
        public Task AddUser(User user);
        public Task<bool> DeleteUser(int id);
        public Task<bool> UpdateUser(int id, User user);
        public Task<bool> UpdateUser(int id, JsonPatchDocument<User> user);
    }
}
