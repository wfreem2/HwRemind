using HwRemind.Api.Endpoints.Users.Models;
using HwRemind.Contexts;
using HwRemind.Endpoints.Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace HwRemind.Endpoints.Authentication.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DbSet<Login> _logins; 
        private readonly DbSet<User> _users; 
        private readonly DbSet<RefreshToken> _refreshTokens; 
        private readonly DbSet<RevokedToken> _revokedTokens;
        private readonly DBContext _dbContext;
        private const char delimeter = ' ';
        public AuthRepository(DBContext dbContext)
        {
            _dbContext = dbContext;

            _users = dbContext.Users;
            _logins = dbContext.Logins;
            _refreshTokens = dbContext.RefreshTokens;
            _revokedTokens = dbContext.RevokedTokens;
        }

        public async Task<Login> AddLogin(Login login)
        {
            login.password = login.password + delimeter + login.salt;

            await _logins.AddAsync(login);
            await _dbContext.SaveChangesAsync();
            return login;
        }

        public async Task<Login> GetLoginByEmail(string email)
        {
            var login = await _logins.Where(l => l.email.Equals(email)).FirstOrDefaultAsync();
            if(login == null) { return login; }

            var saltPswd = login.password.Split(delimeter);

            login.hashedPassword = saltPswd[0];
            login.salt = saltPswd[1];

            return login;
        }
        
        public async Task<Login> GetLoginById(int loginId)
        {
            var login = await _logins.Where(l => l.id == loginId).FirstOrDefaultAsync();
            if(login == null) { return login; }

            var saltPswd = login.password.Split(delimeter);

            login.hashedPassword = saltPswd[0];
            login.salt = saltPswd[1];

            return login;
        }

        public async Task<RefreshToken> GetRefreshToken(string token)
        {

            var refreshToken = await
                (
                from r in _refreshTokens
                where r.token.Equals(token)
                select new RefreshToken
                    {
                        loginId = r.loginId,
                        token = r.token,
                        expiration = r.expiration
                    }
                )
                .FirstOrDefaultAsync();

            var user = await _users.Where(u => u.loginId == refreshToken.loginId).FirstOrDefaultAsync();

            if(user != null) { refreshToken.userId = user.id; }

            return refreshToken;
        } 
        
        public async Task<RefreshToken> GetRefreshToken(int loginId)
        {
            var refreshToken = await
                (
                from r in _refreshTokens
                where r.loginId == loginId
                select new RefreshToken
                {
                    loginId = r.loginId,
                    token = r.token,
                    expiration = r.expiration
                }
                )
                .FirstOrDefaultAsync();

            var user = await _users.Where(u => u.loginId == refreshToken.loginId).FirstOrDefaultAsync();

            if (user != null) { refreshToken.userId = user.id; }

            return refreshToken;
        }

        public async Task AddOrUpdateRefreshToken(RefreshToken newToken)
        {
            var existingToken = await _refreshTokens.Where(t => t.loginId == newToken.loginId).FirstOrDefaultAsync();

            if (existingToken == null) { 
                await _refreshTokens.AddAsync(newToken);
                await _dbContext.SaveChangesAsync();

                return;
            }

            _refreshTokens.Remove(existingToken);
            await _refreshTokens.AddAsync(newToken);

            await _dbContext.SaveChangesAsync();
        }
    }
}
