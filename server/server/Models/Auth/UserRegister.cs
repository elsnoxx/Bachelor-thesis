using System.ComponentModel.DataAnnotations;

namespace server.Models.Auth
{
    /// <summary>
    /// Data transfer object for new user registration.
    /// Includes validation rules for password complexity and data integrity.
    /// </summary>
    public class UserRegister
    {
        /// <summary>
        /// Unique display name for the user.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; }

        /// <summary>
        /// User's email address used for account verification and login.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        /// <summary>
        /// New password meeting complexity requirements: 
        /// At least 8 characters, one uppercase letter, one digit, and one special character.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one digit, and one special character.")]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation field to ensure the password was entered correctly.
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;
    }
}