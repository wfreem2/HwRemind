using HwRemind.Configs;
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

        protected static Mock<ILogger<T>> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }

    }


}
