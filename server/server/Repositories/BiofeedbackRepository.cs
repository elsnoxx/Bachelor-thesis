using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.Sig;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
    /// <summary>
    /// Implementation of the repository pattern for BioFeedback using Entity Framework Core.
    /// Handles optimized querying of large physiological datasets.
    /// </summary>
    public class BiofeedbackRepository : IBiofeedbackRepository
    {
        private readonly AppDbContext _context;

        public BiofeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetches user biofeedback history. 
        /// Uses Include for eager loading of user details if necessary for statistics.
        /// </summary>
        public async Task<IEnumerable<BioFeedback>> GetUserStatisticsAsync(Guid userId)
        {
            return await _context.BioFeedbacks
                .Include(b => b.User)
                .Where(b => b.User.Id == userId)
                .OrderByDescending(b => b.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Filters biofeedback data at the database level using session and user IDs.
        /// This ensures only relevant data points are transferred to memory.
        /// </summary>
        public async Task<IEnumerable<BioFeedback>> GetBySessionAsync(Guid userId, Guid sessionId)
        {
            return await _context.BioFeedbacks
                .Where(b => b.UserId == userId && b.GameRoomId == sessionId)
                .ToListAsync();
        }

        /// <summary>
        /// Asynchronously adds a data point. SaveChangesAsync ensures high-frequency 
        /// writes are handled according to the configured database pool.
        /// </summary>
        public async Task AddAsync(BioFeedback bioFeedback)
        {
            _context.BioFeedbacks.Add(bioFeedback);
            await _context.SaveChangesAsync();
        }
    }
}
