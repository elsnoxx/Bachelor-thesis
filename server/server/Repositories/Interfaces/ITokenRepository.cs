using server.Models.DB;
using System;
using System.Threading.Tasks;

namespace server.Repositories.Interfaces
{
    /// <summary>
    /// Interface for managing refresh tokens used in the authentication process.
    /// Supports token persistence, retrieval, and secure revocation.
    /// </summary>
    public interface ITokenRepository
    {
        /// <summary>
        /// Persists a new refresh token to the database.
        /// </summary>
        Task AddAsync(RefreshToken refreshToken);

        /// <summary>
        /// Retrieves a refresh token and its associated user details.
        /// </summary>
        /// <param name="refreshToken">The raw token string.</param>
        Task<RefreshToken?> GetByTokenAsync(string refreshToken);

        /// <summary>
        /// Invalidates a token by marking it as revoked.
        /// Part of the "Refresh Token Rotation" security strategy.
        /// </summary>
        /// <param name="id">Database ID of the token.</param>
        /// <param name="ipAddress">IP address of the client requesting revocation.</param>
        /// <param name="replacedByToken">Optional ID of the new token replacing this one.</param>
        Task RevokeTokenAsync(Guid id, string ipAddress, string? replacedByToken = null);
    }
}