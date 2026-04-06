using System.ComponentModel.DataAnnotations;

namespace server.Models.Auth
{
    public class UserResetPassword
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Heslo musí mít alespoň 8 znaků.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$",
            ErrorMessage = "Heslo musí obsahovat alespoň jedno velké písmeno, číslici a speciální znak.")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Hesla se neshodují.")]
        public string ConfirmNewPassword { get; set; }
    }
}
