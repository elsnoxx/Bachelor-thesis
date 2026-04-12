using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace server.Repositories
{
    /// <summary>
    /// Repository implementation for refresh token management using Entity Framework Core.
    /// </summary>
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _context;

        public TokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Fetches the token with Eager Loading of the User entity.
        /// This is necessary to verify user status during the refresh process.
        /// </summary>
        public async Task<RefreshToken?> GetByTokenAsync(string refreshToken)
        {
            return await _context.RefreshTokens
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Token == refreshToken);
        }

        /// <summary>
        /// Revokes a refresh token. 
        /// Crucial for preventing replay attacks and ensuring secure logout.
        /// </summary>
        public async Task RevokeTokenAsync(Guid id, string ipAddress, string? replacedByToken = null)
        {
            var tokenRecord = await _context.RefreshTokens.FindAsync(id);

            if (tokenRecord != null)
            {
                tokenRecord.Revoked = DateTime.UtcNow;
                tokenRecord.RevokedByIp = ipAddress;
                tokenRecord.ReplacedByToken = replacedByToken;

                _context.RefreshTokens.Update(tokenRecord);
                // FIXED: Added missing SaveChangesAsync to persist the revocation
                await _context.SaveChangesAsync();
            }
        }
    }
}