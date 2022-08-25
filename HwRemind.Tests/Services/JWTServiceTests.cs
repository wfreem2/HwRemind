using HwRemind.Configs;
using HwRemind.Endpoints.Authentication.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace HwRemind.Tests.Services
{
    [TestFixture]
    public class JWTServiceTests : TestBase
    {
        private static JWTTestData testData = GetTestData();
        private JWTConfig jwtConfig;
        private JWTService jwtService;

        [SetUp]
        public void Setup()
        {
            jwtConfig = GetJWTConfig();

            jwtService = new JWTService(Options.Create(jwtConfig));
        }

        [Test]
        public async Task AccessToken_Should_Have_Correct_Claims()
        {
            var token = await jwtService.GenerateAccessToken(1);
            var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.NotNull(parsedToken);
            Assert.NotNull(parsedToken.Issuer, "iss claim was not present");
            Assert.NotNull(parsedToken.ValidTo, "expiration was not present");
            Assert.NotNull(parsedToken.IssuedAt, "iat claim was not present");
            Assert.NotNull(parsedToken.Audiences, "aud claim was not present");
            Assert.NotNull(parsedToken.SignatureAlgorithm, "alg claim was not present");
        }

        [Test]
        public async Task AccessToken_Should_Be_Valid_and_Expired()
        {
            var isExpired = await jwtService.IsTokenExpired(testData.expired);

            Assert.IsTrue(isExpired, "Token should be expired");
        }

        [Test]
        public async Task AccessToken_Should_Be_Valid_and_Not_Expired()
        {
            var token = await jwtService.GenerateAccessToken(1);
            var isExpired = await jwtService.IsTokenExpired(token);

            Assert.IsFalse(isExpired, "Token should not be expired");

        }

        [Test]
        public async Task Should_Return_True_With_Expired_Token()
        {
            var expiredToken = testData.expired;

            var isExpired = await jwtService.IsTokenExpired(expiredToken);

            Assert.IsTrue(isExpired, "Token was not expired");
        }

        [Test]
        public async Task RefreshToken_Should_Be_Generated()
        {
            var token = await jwtService.GenerateRefreshToken(2);

            Assert.IsNotNull(token, "Refresh token was null");
            Assert.IsTrue(token.loginId == 2, "Login id is not correct");
            Assert.IsNotNull(token.token, "No token was provided");
            Assert.IsInstanceOf<string>(token.token, "Refresh token should be a string");

        }

        private static IEnumerable<string> InvalidIssAndAudTokens
        {
            get
            {
                return new List<string>
                {
                    testData.invalid_iss,
                    testData.invalid_aud
                };
            }
        }
    }


}