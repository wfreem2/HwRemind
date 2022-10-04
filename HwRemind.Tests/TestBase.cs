using HwRemind.Api.Endpoints.Users.Repositories;
using HwRemind.API.Endpoints.Authentication.Services;
using HwRemind.Configs;
using HwRemind.Endpoints.Authentication;
using HwRemind.Endpoints.Authentication.Repositories;
using HwRemind.Endpoints.Authentication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;

namespace HwRemind.Tests
{
    public class TestBase
    {
        private const string testDataPath = "./TestData/tokens.json";

        protected Mock<IAuthRepository> mockAuthRepo;
        protected Mock<IUserRepository> mockUserRepo;

        protected Mock<IJWTService> mockJWTService;
        protected Mock<IPasswordService> mockPswdService;

        protected Mock<IDistributedCache> mockCache;


        protected virtual void setup()
        {
            mockAuthRepo = (Mock<IAuthRepository>)GetMock<IAuthRepository>();
            mockUserRepo = (Mock<IUserRepository>)GetMock<IUserRepository>();


            mockCache = (Mock<IDistributedCache>)GetMock<IDistributedCache>();

            mockJWTService = (Mock<IJWTService>)GetMock<IJWTService>();
            mockPswdService = (Mock<IPasswordService>)GetMock<IPasswordService>();
        }

        protected class JWTTestData
        {
            public string expired { get; set; }
            public string invalid_iss { get; set; }
            public string invalid_aud { get; set; }
        }

        protected static JWTTestData GetTestData()
        {
            using var reader = new StreamReader(testDataPath);
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<JWTTestData>(json);
        }

        protected static JWTConfig GetJWTConfig()
        {
            var config = new
            ConfigurationBuilder()
            .AddUserSecrets<JWTConfig>()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

            var jwt = config.GetSection("JWT").Get<JWTConfig>();

            IOptions<JWTConfig> options = Options.Create(jwt);

            return options.Value;
        }
        
        protected void VerifyBadRequest(IActionResult result)
        {
            var http = result as StatusCodeResult;

            Assert.NotNull(http, "Result should return a Statuscode");
            Assert.True(http.StatusCode == 400, "Status code should be 400");
        }


        protected static Mock<ILogger<T>> GetMockLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }

        protected static Mock GetMock<T>() where T : class
        {
            return new Mock<T>();
        }
    }


}
