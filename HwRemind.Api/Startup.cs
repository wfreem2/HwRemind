using HwRemind.Api.Configs;
using HwRemind.Api.Endpoints.Assignments.Repositories;
using HwRemind.Api.Endpoints.Authentication.Filters;
using HwRemind.Api.Endpoints.Users.Repositories;
using HwRemind.Api.Gloabl_Services;
using HwRemind.API.Endpoints.Authentication.Services;
using HwRemind.Configs;
using HwRemind.Contexts;
using HwRemind.Endpoints.Authentication.Repositories;
using HwRemind.Endpoints.Authentication.Services;
using HwRemind.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HwRemind
{
    public class Startup
    {
        public Startup(IConfiguration config) => Configuration = config;

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseMiddleware<JWTRevokedMiddleware>();
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(ep => ep.MapControllers() );
        }


        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.Configure<JWTConfig>(Configuration.GetSection("JWT"));
            
            services.AddDbContext<DBContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("HwReminder"));
            });

            services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = Configuration.GetConnectionString("Redis");
            });

            //For paginated results
            services.AddHttpContextAccessor();
            services.AddSingleton<IUriService>(o =>
            {
                var accessor = o.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var uri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriService(uri);
            });

            services.AddSingleton<IJWTService, JWTService>();
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<ExpiredTokenWithLoginFilter>();

            services.AddTransient<IAuthRepository, AuthRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IAssignmentRepository, AssignmentRepository>();

            services
           .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               var jwtConfig = Configuration.GetSection("JWT").Get<JWTConfig>();
               var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.secret));

               options.SaveToken = true;
               options.TokenValidationParameters = new()
               {
                   ValidateIssuer = true,
                   ValidateLifetime = true,
                   ValidateAudience = true,

                   ValidIssuer = jwtConfig.iss,
                   ValidAudience = jwtConfig.aud,
                   ValidAlgorithms = new[] { jwtConfig.alg },
                   IssuerSigningKey = signingKey,

                   ClockSkew = TimeSpan.Zero
               };

               options.Validate();
           });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.LoginRequired, policy => policy.RequireClaim("id"));
                options.AddPolicy(PolicyNames.UserAndLoginRequired, 
                    policy => {
                        policy.RequireClaim("userId");
                        policy.RequireClaim("id");
                    }
                );
            });
        }


        public IConfiguration Configuration { get; }
    }
}