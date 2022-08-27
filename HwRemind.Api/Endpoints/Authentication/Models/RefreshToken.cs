using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HwRemind.Endpoints.Authentication.Models
{
    public class RefreshToken
    {
        [Key]
        public string token { get; set; }
        [NotMapped]
        public int? userId { get; set; }
        public int loginId { get; set; }
        public DateTime expiration { get; set; }

        public override string ToString()
        {
            return token;
        }
    }
}
