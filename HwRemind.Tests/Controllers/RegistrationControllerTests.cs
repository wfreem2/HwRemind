using Microsoft.Extensions.Logging;
using Moq;
using HwRemind.Endpoints.Registration;
using HwRemind.Endpoints.Authentication.Models;
using Microsoft.AspNetCore.Mvc;

namespace HwRemind.Tests.Controllers
{
    public class RegistrationControllerTests : TestBase
    {
        private Mock<ILogger<RegistrationController>> mockLogger;
        private RegistrationController regController;
        private string[] hashWithSalt;
        private BaseLogin login;
        private Login newLogin;

        [SetUp]
        public void Setup()
        {
            setup();
            mockLogger = GetMockLogger<RegistrationController>();
            regController = CreateRegistrationController();
        }

        [SetUp]
        public void SetupFields()
        {
            login = new BaseLogin
            {
                email = "Email",
                password = "pswd"
            };

            newLogin = new Login
            {
                id = 2,
                email = login.email,
                password = login.password,
                hashedPassword = "hashed",
                salt = "salted"
            };

            hashWithSalt = new string[] { "hashedpassword", "salt" };
        }

        [Test]
        public async Task Should_Return_Ok_With_Tokens()
        {
            mockAuthRepo.Setup(m => m.GetLoginByEmail(login.email))
            .ReturnsAsync(default(Login));

            mockAuthRepo.Setup(
                m => m.AddLogin(It.Is<Login>(l => l.email.Equals(newLogin.email))))
            .ReturnsAsync(newLogin);

            mockPswdService.Setup(m => m.HashPassword(login.password))
            .Returns(hashWithSalt);

            mockJWTService.Setup(m => m.GenerateAccessToken(newLogin.id))
            .ReturnsAsync("");

            mockJWTService.Setup(m => m.GenerateRefreshToken(newLogin.id))
            .ReturnsAsync(new RefreshToken
            {
                userId = 3,
                expiration = DateTime.Now,
                loginId = newLogin.id,
                token = ""
            });

            var result = await regController.Register(login);
            VerifyAuthRequest(result);


            mockAuthRepo.Verify(
               m => m.AddOrUpdateRefreshToken(It.Is<RefreshToken>(r => r.token.Equals(""))),
               Times.Once
           );
        }


        [Test]
        public async Task Should_Return_400_With_Existing_Login()
        {
            mockAuthRepo.Setup(m => m.GetLoginByEmail(login.email))
           .ReturnsAsync(newLogin);

            var result = await regController.Register(login);
            VerifyBadRequest(result);
        }

        private RegistrationController CreateRegistrationController()
        {
            return new RegistrationController(mockLogger.Object, mockAuthRepo.Object, mockPswdService.Object, mockJWTService.Object);
        }
    }
}
