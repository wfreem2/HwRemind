namespace HwRemind.API.Endpoints.Authentication.Services
{
    public interface IPasswordService
    {
        public string[] HashPassword(string password);

        public bool IsMatch(string password1, string hash, string salt);
    }
}
