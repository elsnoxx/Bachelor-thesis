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
                .Include(b => b.Session)
                .Where(b => b.Session.UserId == userId)
                .OrderByDescending(b => b.Timestamp)
                .ToListAsync();
        }
    }
}
