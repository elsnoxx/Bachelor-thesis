using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
    /// <summary>
    /// Implementation of the repository pattern for player statistics using Entity Framework Core.
    /// </summary>
    public class StatisticRepository : IStatisticRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatisticRepository> _logger;

        public StatisticRepository(AppDbContext context, ILogger<StatisticRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all statistics for a user. 
        /// In production, this could be extended with .OrderByDescending(s => s.LastPlayed).
        /// </summary>
        public async Task<IEnumerable<Statistic>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Statistics
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.LastPlayed)
                .ToListAsync();
        }

        public async Task AddAsync(Statistic statistic)
        {
            _context.Statistics.Add(statistic);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Fetches the GameRoom identifier linked to a specific session.
        /// Demonstrates cross-entity lookup within the repository layer.
        /// </summary>
        public async Task<Guid> GetGameRoomIdBySessionIdAsync(Guid sessionId)
        {
            var gameRoomId = await _context.Sessions
                .Where(s => s.Id == sessionId)
                .Select(s => s.GameRoomId)
                .FirstOrDefaultAsync();

            if (gameRoomId == Guid.Empty)
            {
                _logger.LogWarning("Session with ID {SessionId} was not found when resolving GameRoomId.", sessionId);
            }

            return gameRoomId;
        }

        public async Task<Statistic?> GetByIdAsync(Guid id)
        {
            return await _context.Statistics
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}