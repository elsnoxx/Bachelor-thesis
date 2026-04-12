using Microsoft.AspNetCore.Http;
using server.Models.Auth;
using server.Models.DTO;

namespace server.Services.DbServices.Interfaces
{
    /// <summary>
    /// Service for handling authentication and authorization logic.
    /// Orchestrates user identity, token issuance, and account verification.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Validates user credentials and issues a set of security tokens.
        /// </summary>
        Task<Result<Tokens>> LoginAsync(UserLogin userLogin, HttpContext httpContext);

        /// <summary>
        /// Handles new user registration, password hashing, and confirmation triggers.
        /// </summary>
        Task<Result<bool>> RegisterAsync(UserRegister userRegister);

        /// <summary>
        /// Renews an expired Access Token using a valid Refresh Token.
        /// </summary>
        Task<Result<Tokens>> RefreshTokenAsync(string refreshToken, HttpContext httpContext);

        /// <summary>
        /// Performs a secure password update for an existing user account.
        /// </summary>
        Task<bool> ResetPasswordAsync(UserResetPassword model);

        /// <summary>
        /// Validates a unique token and activates the user's account.
        /// </summary>
        Task<Result<bool>> ConfirmEmailAsync(string token);
    }
}