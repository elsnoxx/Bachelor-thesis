using server.Models.DB;

namespace server.Services.Utils.Interfaces
{
    /// <summary>
    /// Defines a contract for security utilities related to JSON Web Tokens (JWT) 
    /// and session management.
    /// </summary>
    public interface IJwtUtils
    {
        /// <summary>
        /// Generates an authentication token for the specified user.
        /// Typically used for short-term authorization (access token).
        /// </summary>
        /// <param name="user">The user entity whose information will be embedded in the token.</param>
        /// <returns>A string representation of the generated JWT.</returns>
        string GenerateJwtToken(User user);

        /// <summary>
        /// Creates a high-entropy random token used to obtain a new access token 
        /// without requiring user credentials.
        /// </summary>
        /// <returns>A cryptographically secure random string.</returns>
        string GenerateRefreshToken();
    }
}