using System.ComponentModel.DataAnnotations;

namespace HwRemind.Endpoints.Authentication.Models
{
    public class RefreshToken
    {
        [Key]
        public string token { get; set; }
        public int loginId { get; set; }
        public DateTime expiration { get; set; }
    }
}
