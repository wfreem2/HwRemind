using HwRemind.Endpoints.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.Extensions.Logging;
using HwRemind.Endpoints.Authentication.Models;
using HwRemind.Endpoints.Authentication.Services;

namespace HwRemind.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests : TestBase
    {
        private Mock<ILogger<AuthController>> mockLogger;
        private AuthController authController;

        private BaseLogin login;
        private Login existingLogin;
        private RefreshRequest refreshRequest;

        [OneTimeSetUp]
        public void Setup()
        {
            setup();

            mockLogger = GetMockLogger<AuthController>();
        }

        [SetUp]
        public void SetupFields()
        {
            authController = CreateAuthController();

            login = new BaseLogin
            {
                email = "Email",
                password = "pswd"
            };

            existingLogin = new Login
            {
                id = 2,
                email = login.email,
                password = login.password,
                hashedPassword = "hashed",
                salt = "salted"
            };

            refreshRequest = new RefreshRequest()
            {
                refreshToken = "token"
            };
        }

        [TearDown]
        public void TearDown()
        {
            //Loggers should be logging information at least once
            mockLogger.Verify(m =>
            m.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ),
            Times.AtLeastOnce);
        }

        /* ############################## Login ############################## */
        [Test]
        public async Task Should_Return_200_With_Existing_Login()
        {
            mockAuthRepo
            .Setup(m => m.GetLoginByEmail(login.email))
            .ReturnsAsync(existingLogin);

            mockPswdService.Setup(m => 
                m.IsMatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
            )
            .Returns(true);

            mockJWTService.Setup(m => m.GenerateAccessToken(existingLogin.id))
            .ReturnsAsync(""); 

            mockJWTService.Setup(m => m.GenerateRefreshToken(existingLogin.id))
            .ReturnsAsync(new RefreshToken
            {
                userId = 3,
                expiration = DateTime.Now,
                loginId = existingLogin.id,
                token = ""
            });


            var result = await authController.Login(login, mockPswdService.Object);
           VerifyAuthRequest(result);

            mockAuthRepo.Verify(
                m => m.AddOrUpdateRefreshToken(It.Is<RefreshToken>(r => r.token.Equals(""))),
                Times.Once
            );
        }

        [Test]
        public async Task Should_Return_400_With_Non_Existent_Login()
        {
            mockAuthRepo
            .Setup(m => m.GetLoginByEmail(login.email))
            .ReturnsAsync(default(Login));

            var result = await authController.Login(login, mockPswdService.Object);
            VerifyBadRequest(result);
        }

        [Test]
        public async Task Should_Return_401_With_Incorrect_Password()
        {
            mockAuthRepo
            .Setup(m => m.GetLoginByEmail(login.email))
            .ReturnsAsync(existingLogin);

            mockPswdService.Setup(m =>
                m.IsMatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
            )
            .Returns(false);


            var result = await authController.Login(login, mockPswdService.Object);
            var http = result as StatusCodeResult;

            Assert.NotNull(http, "Result should return a Statuscode");
            Assert.True(http.StatusCode == 401, "Status code should be 401 (Unauthorized)");
        }


        /* ############################## Refresh ############################## */
       /* [Test]
        public async Task Should_Return_400_With_Invalid_Token()
        {
            mockJWTService.Setup(m => m.IsExpiredAccessTokenValid(It.IsAny<string>()))
            .ReturnsAsync(false);
            
            authController = CreateAuthController(GetContext());

            var result = await authController.Refresh(refreshRequest);
            VerifyBadRequest(result);
        }

        [Test]
        public async Task Should_Return_400_With_Non_Existent_Refresh_Token()
        {
            mockJWTService.Setup(m => m.IsExpiredAccessTokenValid(It.IsAny<string>()))
            .ReturnsAsync(true);

            mockAuthRepo.Setup(m => m.GetRefreshToken(refreshRequest.refreshToken))
            .ReturnsAsync(default(RefreshToken));

            authController = CreateAuthController(GetContext());

            var result = await authController.Refresh(refreshRequest);
            VerifyBadRequest(result);
        }

        [Test]
        public async Task Should_Return_400_With_Expired_Existing_Refresh_Token()
        {
            var expiredToken = new RefreshToken
            {
                expiration = DateTime.Now.AddDays(-1),
                loginId = 2,
                token = "",
                userId = 3
            };

            mockJWTService.Setup(m => m.IsExpiredAccessTokenValid(It.IsAny<string>()))
            .ReturnsAsync(true);

            mockAuthRepo.Setup(m => m.GetRefreshToken(refreshRequest.refreshToken))
            .ReturnsAsync(expiredToken);

            authController = CreateAuthController(GetContext());

            var result = await authController.Refresh(refreshRequest);
            VerifyBadRequest(result);
        }*/


        /* ############################## Revoke ############################## */

        [Test]
        public async Task Should_Return_400_With_Null_Token()
        {
            var revoke = new RevokeRequest
            {
                token = string.Empty
            };

            var result = await authController.Revoke(revoke);
            VerifyBadRequest(result);
        }

        [Test]
        [TestCase("")]
        [TestCase("somejunk")]
        [TestCase("90213091039012930")]
        public async Task Should_Return_400_With_Invalid_Revoke_Token(string token)
        {
            var revoke = new RevokeRequest
            {
                token = token
            };
            var jwt = new JWTService(GetJWTConfigOptions()).TokenValidationParams;

            mockJWTService.SetupGet(m => m.TokenValidationParams)
            .Returns(jwt);

            var result = await authController.Revoke(revoke);
            VerifyBadRequest(result);
        }


        private AuthController CreateAuthController(DefaultHttpContext context = null)
        {
            if (context != null)
            {
                return new AuthController(mockLogger.Object, mockJWTService.Object, mockAuthRepo.Object, mockCache.Object)
                {
                    ControllerContext = new ControllerContext()
                    {
                        HttpContext = context
                    }
                };


            }
            return new AuthController(mockLogger.Object, mockJWTService.Object, mockAuthRepo.Object, mockCache.Object);
        }


        private DefaultHttpContext GetContext(string token = "")
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer " + token;

            return context;
        }
     }
}
