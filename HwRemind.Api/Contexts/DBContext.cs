using HwRemind.Api.Endpoints.Assignments.Models;
using HwRemind.Api.Endpoints.Users.Models;
using HwRemind.Endpoints.Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace HwRemind.Contexts
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        { }

        public DbSet<Login> Logins { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
