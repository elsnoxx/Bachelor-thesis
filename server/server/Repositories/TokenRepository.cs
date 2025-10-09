using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
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

        public async Task<RefreshToken?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.RefreshTokens
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Token == refreshToken);
        }

        public async Task TokenRevocation(Guid id, string ip, string token)
        {
            var tokenRev = await _context.RefreshTokens.FindAsync(id);
            tokenRev.Revoked = DateTime.UtcNow;
            tokenRev.RevokedByIp = ip;
            tokenRev.ReplacedByToken = token;
            _context.RefreshTokens.Update(tokenRev);
        }
    }
}