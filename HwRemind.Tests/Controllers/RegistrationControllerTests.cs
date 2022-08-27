using HwRemind.Endpoints.Authentication.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using HwRemind.Endpoints.Registration;
using HwRemind.Endpoints.Authentication.Models;
using HwRemind.API.Endpoints.Authentication.Services;

namespace HwRemind.Tests.Controllers
{
    public class RegistrationControllerTests : TestBase
    {
        private Mock<IAuthRepository> mockRepo;
        private Mock<ILogger<RegistrationController>> mockLogger;
        private Mock<IPasswordService> mockPswdService;

        [SetUp]
        public void Setup()
        {
            mockRepo = GetMockRepository();
            mockPswdService = new Mock<IPasswordService>();
            mockLogger = CreateMockLogger<RegistrationController>();
        }

        [Test]
        public async Task Should_Return_400_With_Existing_Login() 
        {
            var existingLogin = new Login
            {
                email = "iexist",
                password = "ialsoexist"
            };

            mockRepo.Setup(m => m.GetLoginByEmail(It.IsAny<string>()))
            .ReturnsAsync(existingLogin);

            var controller = CreateRegistrationController();


            var result = await controller.Register(existingLogin, mockPswdService.Object);

            var http = result as StatusCodeResult;

            Assert.NotNull(http, "Result should return a Statuscode");
            Assert.True(http.StatusCode == 400, "Status code should be 400");
        }

        [Test]
        public async Task Should_Return_200_With_Valid_Login()
        {
            var validLogin = new Login
            {
                email = "valid",
                password = "validpswd"
            };

            mockRepo.Setup(m => m.GetLoginByEmail(It.IsAny<string>()))
           .ReturnsAsync(default(Login));

            mockPswdService.Setup(m => m.HashPassword(It.IsAny<string>()))
            .Returns(new string[] { "somepswd", "somesalt" });

            var controller = CreateRegistrationController();

            var result = await controller.Register(validLogin, mockPswdService.Object);

            var http = result as OkResult;

            mockRepo.Verify(m => m.AddLogin(It.Is<Login>(l => l.email.Equals(validLogin.email))), Times.Once);

            Assert.NotNull(http, "Result should return a OkResult");
            Assert.True(http.StatusCode == 200, "Status code should be 200");
        }

        private RegistrationController CreateRegistrationController()
        {
            return new RegistrationController(mockLogger.Object, mockRepo.Object);
        }
        private Mock<IAuthRepository> GetMockRepository() => new Mock<IAuthRepository>();
    }
}
