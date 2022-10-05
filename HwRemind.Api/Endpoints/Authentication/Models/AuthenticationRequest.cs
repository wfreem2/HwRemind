using System.ComponentModel.DataAnnotations;

namespace HwRemind.Endpoints.Authentication.Models
{
    public class AuthenticationRequest
    {
        public string accessToken { get; set; }
        [Required]
        public string refreshToken { get; set; }    
    }
}
