using System.ComponentModel.DataAnnotations;

namespace HwRemind.Endpoints.Authentication.Models
{
    public class RevokeRequest
    {
        [Required]
        public string token { get; set; }
    }
}
