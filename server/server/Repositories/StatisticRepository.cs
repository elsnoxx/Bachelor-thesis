using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatisticRepository> _logger;

        public StatisticRepository(AppDbContext context, ILogger<StatisticRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<IEnumerable<Statistic>> GetStatistic(Guid UserId)
        {
            return await _context.Statistics
                .Where(s => s.UserId == UserId)
                .ToListAsync();
        }

        public async Task AddAsync(Statistic statistic)
        {
            _context.Statistics.Add(statistic);
            await _context.SaveChangesAsync();

        }

        public async Task<Guid> GetGameRoomBySessionId(Guid sessionId)
        {
            var gameRoomId = await _context.Sessions
                .Where(s => s.Id == sessionId)
                .Select(s => s.GameRoomId)
                .FirstOrDefaultAsync();

            if (gameRoomId == Guid.Empty)
            {
                _logger.LogWarning("Session with ID {SessionId} was not found in database.", sessionId);
            }

            return gameRoomId;
        }

        public async Task<Statistic> GetStatisticById(Guid id)
        {
            return await _context.Statistics
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
