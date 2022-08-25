using System.ComponentModel.DataAnnotations;

namespace HwRemind.Endpoints.Authentication.Models
{
    public class RefreshRequest
    {
        [Required]
        public string refreshToken { get; set; }
    }
}
