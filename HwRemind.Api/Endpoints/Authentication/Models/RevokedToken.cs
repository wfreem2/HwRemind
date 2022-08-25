using System.ComponentModel.DataAnnotations;

namespace HwRemind.Endpoints.Authentication.Models
{
    public class RevokedToken
    {
        [Key]
        public string token { get; set; }
        public DateTime expiration { get; set; }
    }
}
