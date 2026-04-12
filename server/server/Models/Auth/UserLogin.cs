using System.ComponentModel.DataAnnotations;

namespace server.Models.Auth
{
    /// <summary>
    /// Data transfer object for user authentication requests.
    /// Holds credentials required to issue a JWT token.
    /// </summary>
    public class UserLogin
    {
        /// <summary>
        /// User's registered email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        /// <summary>
        /// User's plain-text password to be verified against the stored hash.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}