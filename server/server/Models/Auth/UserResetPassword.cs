using System.ComponentModel.DataAnnotations;

namespace server.Models.Auth
{
    /// <summary>
    /// Data transfer object for password reset operations.
    /// Ensures secure password updates by requiring confirmation.
    /// </summary>
    public class UserResetPassword
    {
        /// <summary>
        /// Email of the user requesting a password reset.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        /// <summary>
        /// The new password following system complexity standards.
        /// </summary>
        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirmation of the new password.
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}