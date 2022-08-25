using HwRemind.Endpoints.Authentication.Models;

namespace HwRemind.Endpoints.Authentication.Repositories
{
    public interface IAuthRepository
    {
        public Task<Login> AddLogin(Login login);
        public Task<Login> GetLogin(string email);
        public Task<Login> GetLogin(int id);
        public Task<RefreshToken> GetRefreshToken(string token);
        public Task<RefreshToken> GetRefreshToken(int loginId);
        public Task AddRefreshToken(RefreshToken newToken);
    }
}
