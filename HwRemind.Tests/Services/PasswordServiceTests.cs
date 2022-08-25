using HwRemind.API.Endpoints.Authentication.Services;

namespace HwRemind.Tests.Services
{
    [TestFixture]
    public class PasswordServiceTests
    {
        private IPasswordService pswdService;

        [OneTimeSetUp]
        public void Setup()
        {
            pswdService = new PasswordService();
        }

        [Test]
        public void Hash_Should_Return_HashSalt_Array()
        {
            var hashSalt = pswdService.HashPassword("somepswd");

            Assert.IsTrue(hashSalt.Length == 2, "Function should return array of length 2");
            Assert.IsInstanceOf<string[]>(hashSalt, "Function should be string array");
            Assert.NotNull(hashSalt[0], "Array should contain hash");
            Assert.NotNull(hashSalt[1], "Array should contain salt");
        }


        [Test]
        public void IsMatch_Should_Return_True()
        {
            var pswd = "somepswd";
            var hashSalt = pswdService.HashPassword(pswd);

            var isMatch = pswdService.IsMatch(pswd, hashSalt[0], hashSalt[1]);

            Assert.IsTrue(isMatch, "Should be true for same password");
        }
        
        [Test]
        public void IsMatch_Should_Return_False()
        {
            var pswd = "somepswd";
            var pswd2 = "notsomepswd";

            var hashSalt = pswdService.HashPassword(pswd);

            var isMatch = pswdService.IsMatch(pswd2, hashSalt[0], hashSalt[1]);

            Assert.IsFalse(isMatch, "Should be false for different password");
        }
    }
}
