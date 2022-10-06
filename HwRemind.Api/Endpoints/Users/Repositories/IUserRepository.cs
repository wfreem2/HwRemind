using HwRemind.Api.Endpoints.Users.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HwRemind.Api.Endpoints.Users.Repositories
{
    public interface IUserRepository
    {
        public Task<User> GetUserById(int id);
        public Task<User> GetUserByLoginId(int loginId);

        public Task<User> GetUserByLoginEmail(string email);
        public Task AddUser(User user);
        public Task<bool> DeleteUser(int loginId);
        public Task<bool> UpdateUser(int loginId, User user);
        public Task<bool> UpdateUser(int loginId, JsonPatchDocument<User> user);
    }
}
