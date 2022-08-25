using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HwRemind.Endpoints.Authentication.Models
{
    public class Login : BaseLogin
    {
        public int id { get; set; }

        [NotMapped]
        public string salt { get; set; }

        [NotMapped]
        public string hashedPassword { get; set; }
    }

    public class BaseLogin 
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }

}
