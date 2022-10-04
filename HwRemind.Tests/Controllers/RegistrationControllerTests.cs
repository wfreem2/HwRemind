using Microsoft.Extensions.Logging;
using Moq;
using HwRemind.Endpoints.Registration;

namespace HwRemind.Tests.Controllers
{
    public class RegistrationControllerTests : TestBase
    {
        private Mock<ILogger<RegistrationController>> mockLogger;


        [SetUp]
        public void Setup()
        {
            setup();
            mockLogger = GetMockLogger<RegistrationController>();
        }

      
        private RegistrationController CreateRegistrationController()
        {
            return new RegistrationController(mockLogger.Object, mockAuthRepo.Object, mockPswdService.Object, mockJWTService.Object);
        }
    }
}
