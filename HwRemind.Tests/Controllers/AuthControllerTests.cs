using HwRemind.Endpoints.Authentication;
using HwRemind.Endpoints.Authentication.Repositories;
using HwRemind.Endpoints.Authentication.Services;
using Microsoft.Extensions.Logging;
using Moq;
using HwRemind.Endpoints.Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using HwRemind.Extensions;
using Microsoft.AspNetCore.Http;
using HwRemind.API.Endpoints.Authentication.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace HwRemind.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests : TestBase
    {
        private Mock<IAuthRepository> mockRepo;
        private Mock<IJWTService> mockJWTService;
        private Mock<ILogger<AuthController>> mockLogger;
        private Mock<IPasswordService> mockPswdService;
        private Mock<IDistributedCache> mockCache;

        [SetUp]
        public void Setup()
        {
            mockCache = new Mock<IDistributedCache>();
            mockRepo = GetMockRepository();
            mockJWTService = GetMockJWTService();
            mockLogger = CreateMockLogger<AuthController>();
            mockPswdService = new Mock<IPasswordService>();
        }

        [Test]
        public async Task Existing_Refresh_Token_Should_Be_Updated()
        {
            int loginId = 2;

            var login = new BaseLogin
            {
                email = "someemail",
                password = "somepassword"
            };

            var existingRefreshToken = new RefreshToken()
            {
                token = "sometoken",
                loginId = loginId
            };

            var token = "arandotoken";

            var context = new DefaultHttpContext();
                context.Request.Headers["Authorization"] = "Bearer " + token;

            mockJWTService.Setup(m => m.GenerateAccessToken())
            .ReturnsAsync("anewtoken");
            
            mockJWTService.Setup(m => m.GenerateRefreshToken(loginId))
            .ReturnsAsync(new RefreshToken()
            {
                token = "newtoken"
            });

            mockRepo.Setup(m => m.GetLogin(login.email))
            .ReturnsAsync(new Login
            {
                id = loginId,
                email = login.email,
                password = login.password
            });

            mockRepo.Setup(m => m.GetRefreshToken(loginId))
            .ReturnsAsync(existingRefreshToken);

            mockPswdService.Setup(m => m.IsMatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

            var authController = CreateAuthController(context);

            var result = await authController.Authenticate(login, mockPswdService.Object);

            var ok = result as OkObjectResult;
            Assert.NotNull(ok, "Ok result should not be null");

            var authReq = ok.Value as AuthenticationRequest;

            Assert.NotNull(authReq, "Authentication request was null");
            Assert.NotNull(authReq.accesstoken, "Access token should be provided");
            Assert.NotNull(authReq.refreshToken, "Refresh token should be provided");

            mockRepo.Verify(
                m => m.AddRefreshToken(It.Is<RefreshToken>(r => r.token.Equals("newtoken"))), Times.Once
            );
        }

        [Test]
        public async Task Authentication_Should_Return_Tokens()
        {
            int loginId = 2;

            var login = new BaseLogin
            {
                email = "someemail",
                password = "somepassword"
            };

            var token = "arandotoken";

            var context = new DefaultHttpContext();
                context.Request.Headers["Authorization"] = "Bearer " + token;

            mockJWTService.Setup(m => m.GenerateAccessToken())
            .ReturnsAsync("anewtoken");
            
            mockJWTService.Setup(m => m.GenerateRefreshToken(loginId))
            .ReturnsAsync(new RefreshToken() { token = "sometoken"});

            mockRepo.Setup(m => m.GetLogin(login.email))
            .ReturnsAsync(new Login
            {
                id = 2,
                email = login.email,
                password = "somehash",
                salt = "somesalt"
            });

            mockRepo.Setup(m => m.GetRefreshToken(It.IsAny<int>()));
            mockRepo.Setup(m => m.AddRefreshToken(It.IsAny<RefreshToken>()));

            mockPswdService.Setup(m => m.IsMatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);


            var authController = CreateAuthController(context);

            var result = await authController.Authenticate(login, mockPswdService.Object);

            var ok = result as OkObjectResult;
            Assert.NotNull(ok, "Ok result should not be null");

            var authReq = ok.Value as AuthenticationRequest;

            Assert.NotNull(authReq, "Authentication request was null");
            Assert.NotNull(authReq.accesstoken, "Access token should be provided");
            Assert.NotNull(authReq.refreshToken, "Refresh token should be provided");

            mockRepo.Verify(m => m.AddRefreshToken(It.IsAny<RefreshToken>()), Times.Once);
        }
        
        [Test]
        public async Task NonExistent_Login_Should_Return_400()
        {
            
            var login = new BaseLogin
            {
                email = "someemail",
                password = "somepassword"
            };

            var token = "arandotoken";

            var context = new DefaultHttpContext();
                context.Request.Headers["Authorization"] = "Bearer " + token;

            mockRepo.Setup(m => m.GetLogin(login.email))
            .ReturnsAsync(default(Login));

            var authController = CreateAuthController(context);

            var result = await authController.Authenticate(login, mockPswdService.Object);
            var http = result as StatusCodeResult;

            Assert.NotNull(http, "Result should return a Statuscode");
            Assert.True(http.StatusCode == 400, "Status code should be 400");
        }

        [Test]
        public async Task RefreshRequest_Should_Return_400_With_No_JWT_Provided()
        {
            var refreshRequest = new RefreshRequest
            {
                refreshToken = "refreshotken"
            };

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = string.Empty;


            var authController = CreateAuthController();
            authController.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var result = await authController.Refresh(refreshRequest);
            var http = result as StatusCodeResult;

            Assert.NotNull(http, "Result should return a Statuscode");
            Assert.True(http.StatusCode == 400, "Status code should be 400");
        }

        [Test]
        public async Task RefreshRequest_Should_Return_400_With_Invalid_JWT()
        {
            var refreshRequest = new RefreshRequest
            {
                refreshToken = "refreshotken"
            };

            var token = "newtoken";

            var context = new DefaultHttpContext();
                context.Request.Headers["Authorization"] = "Bearer " + token;

            mockJWTService.Setup(m => m.IsExpiredAccessTokenValid(It.IsAny<string>()))
            .ReturnsAsync(false);


            var authController = CreateAuthController(context);

            var result = await authController.Refresh(refreshRequest);
            var http = result as StatusCodeResult;

            Assert.NotNull(http, "Result should return a Statuscode");
            Assert.True(http.StatusCode == 400, "Status code should be 400");

            mockJWTService.Verify(m => m.IsExpiredAccessTokenValid(token), Times.Once);
        }

        [Test]
        public async Task RefreshRequest_Should_Return_400_With_Expired_Existing_Refresh_Token()
        {
            var refreshRequest = new RefreshRequest
            {
                refreshToken = "expiredtoken"
            };

            var existingExpiredToken = new RefreshToken
            {
                token = "expiredtoken",
                expiration = DateTime.Now.AddDays(-1)
            };

            var token = "arandotoken";
            
            var context = new DefaultHttpContext();
                context.Request.Headers["Authorization"] = "Bearer " + token;

            mockJWTService.Setup(m => m.IsExpiredAccessTokenValid(It.IsAny<string>()))
           .ReturnsAsync(true);

            mockRepo.Setup(m => m.GetRefreshToken(refreshRequest.refreshToken))
            .ReturnsAsync(existingExpiredToken);

            var authController = CreateAuthController();
            authController.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var result = await authController.Refresh(refreshRequest);
            var http = result as StatusCodeResult;

            Assert.NotNull(http, "Result should return a Statuscode");
            Assert.True(http.StatusCode == 400, "Status code should be 400");

            mockJWTService.Verify(m => m.IsExpiredAccessTokenValid(token), Times.Once);
            mockRepo.Verify(m => m.GetRefreshToken(refreshRequest.refreshToken), Times.Once);   
        }

        [Test]
        public async Task RefreshRequest_Should_Return_Tokens_With_Valid_Request()
        {
            var refreshRequest = new RefreshRequest
            {
                refreshToken = "expiredtoken"
            };

            var existingToken = new RefreshToken
            {
                token = "expiredtoken",
                expiration = DateTime.Now.AddDays(1)
            };

            var newToken = new RefreshToken
            {
                token = "newtoken"
            };

            var token = "arandotoken";

            var context = new DefaultHttpContext();
                context.Request.Headers["Authorization"] = "Bearer " + token;

            mockJWTService.Setup(m => m.IsExpiredAccessTokenValid(It.IsAny<string>()))
           .ReturnsAsync(true);

            mockJWTService.Setup(m => m.GenerateRefreshToken(existingToken.loginId))
            .ReturnsAsync(new RefreshToken() { token = "notempty"});

            mockJWTService.Setup(m => m.GenerateAccessToken())
            .ReturnsAsync("somenewtoken");

            mockRepo.Setup(m => m.GetRefreshToken(refreshRequest.refreshToken))
            .ReturnsAsync(existingToken);

            mockRepo.Setup(m => m.AddRefreshToken(It.IsAny<RefreshToken>()));

            var authController = CreateAuthController(context);

            var result = await authController.Refresh(refreshRequest);

            var ok = result as OkObjectResult;
            Assert.NotNull(ok, "Ok result should not be null");

            var authReq = ok.Value as AuthenticationRequest;

            Assert.NotNull(authReq, "Authentication request was null");
            Assert.NotNull(authReq.accesstoken, "Access token should be provided");
            Assert.NotNull(authReq.refreshToken, "Refresh token should be provided");


            mockJWTService.Verify(m => m.IsExpiredAccessTokenValid(token), Times.Once);
            mockRepo.Verify(m => m.GetRefreshToken(refreshRequest.refreshToken), Times.Once);
            mockRepo.Verify(m => m.AddRefreshToken(It.IsAny<RefreshToken>()), Times.Once);

        }

        [Test]
        public async Task Authentication_Should_Return_401_Invalid_Password()
        {
            int loginId = 2;

            var login = new BaseLogin
            {
                email = "someemail",
                password = "somepassword"
            };

            var token = "arandotoken";

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer " + token;

            mockJWTService.Setup(m => m.GenerateAccessToken())
            .ReturnsAsync("anewtoken");

            mockJWTService.Setup(m => m.GenerateRefreshToken(loginId))
            .ReturnsAsync(new RefreshToken() { token = "sometoken" });

            mockRepo.Setup(m => m.GetLogin(login.email))
            .ReturnsAsync(new Login
            {
                id = 2,
                email = login.email,
                password = "somehash",
                salt = "somesalt"
            });

            mockRepo.Setup(m => m.GetRefreshToken(It.IsAny<int>()));
            mockRepo.Setup(m => m.AddRefreshToken(It.IsAny<RefreshToken>()));

            mockPswdService.Setup(m => m.IsMatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);


            var authController = CreateAuthController(context);

            var result = await authController.Authenticate(login, mockPswdService.Object);
            var http = result as StatusCodeResult;

            Assert.NotNull(http, "Result should return a Statuscode");
            Assert.True(http.StatusCode == 401, "Status code should be 400");
        }

        private AuthController CreateAuthController(DefaultHttpContext context = null)
        {
            if (context != null)
            {
                return new AuthController(mockLogger.Object, mockJWTService.Object, mockRepo.Object, mockCache.Object)
                {
                    ControllerContext = new ControllerContext()
                    {
                        HttpContext = context
                    }
                };


            }
            return new AuthController(mockLogger.Object, mockJWTService.Object, mockRepo.Object, mockCache.Object);
        }
        private Mock<IAuthRepository> GetMockRepository() => new Mock<IAuthRepository>();
        private Mock<IJWTService> GetMockJWTService() => new Mock<IJWTService>();

     }
}
