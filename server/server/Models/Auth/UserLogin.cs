using System.ComponentModel.DataAnnotations;

namespace server.Models.Auth
{
    public class UserLogin
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
