using System.ComponentModel.DataAnnotations;

namespace HwRemind.Endpoints.Authentication.Models
{
    public class AuthenticationRequest
    {
        public string accesstoken { get; set; }
        [Required]
        public string refreshToken { get; set; }    
    }
}
