using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace HwRemind.API.Endpoints.Authentication.Services
{
    public class PasswordService : IPasswordService
    {
        private const int iterations = 100000;
        private const int bytes = 256 / 8;
        private readonly HashAlgorithmName alg = HashAlgorithmName.SHA384;

        public string[] HashPassword(string password)
        {
            //Divide by 8 to convert to bytes
            byte[] salt = RandomNumberGenerator.GetBytes(bytes);

            byte[] pswdBytes = Encoding.UTF8.GetBytes(password);

            using var rfc = new Rfc2898DeriveBytes(pswdBytes, salt, iterations, alg);
            var hash = Convert.ToBase64String(rfc.GetBytes(bytes));

            return new string[] { hash, Convert.ToBase64String(salt) };
        }

        public bool IsMatch(string password, string hash, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] pswdBytes = Encoding.UTF8.GetBytes(password);

            using var rfc = new Rfc2898DeriveBytes(pswdBytes, saltBytes, iterations, alg);
            var computedHash = Convert.ToBase64String(rfc.GetBytes(bytes));

            return computedHash.Equals(hash);
        }
    }
}
