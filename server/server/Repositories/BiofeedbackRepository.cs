using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
    public class BiofeedbackRepository : IBiofeedbackRepository
    {
        private readonly AppDbContext _context;

        public BiofeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BioFeedback>> GetStatistic(Guid userId)
        {
            return await _context.BioFeedbacks
                .Include(b => b.User)
                .Where(b => b.User.Id == userId)
                .OrderByDescending(b => b.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<BioFeedback>> GetBySessionAsync(Guid userId, Guid sessionId)
        {
            return await _context.BioFeedbacks
                .Where(b => b.UserId == userId && b.GameRoomId == sessionId)
                .ToListAsync(); // Tady se filtrace provede už v SQL databázi
        }
    }
}
