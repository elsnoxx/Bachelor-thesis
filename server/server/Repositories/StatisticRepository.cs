using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly AppDbContext _context;

        public StatisticRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Statistic>> GetStatistic(Guid UserId)
        {
            return await _context.Statistics
                .Where(s => s.UserId == UserId)
                .ToListAsync();
        }

    }
}
