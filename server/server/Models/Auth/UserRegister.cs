using System.ComponentModel.DataAnnotations;

namespace server.Models.Auth
{
    public class UserRegister
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Heslo musí mít alespoň 8 znaků.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$",
            ErrorMessage = "Heslo musí obsahovat alespoň jedno velké písmeno, číslici a speciální znak.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hesla se neshodují.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
