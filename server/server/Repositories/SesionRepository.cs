using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
    /// <summary>
    /// Implementation of session management logic using Entity Framework Core.
    /// </summary>
    public class SessionRepository : ISessionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SessionRepository> _logger;

        public SessionRepository(AppDbContext context, ILogger<SessionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Optimized query to retrieve only the Guid of an active session.
        /// </summary>
        public async Task<Guid> GetSessionIdByUserAndRoomAsync(Guid userId, Guid roomId)
        {
            // Use FirstOrDefaultAsync for non-blocking database access
            var sessionId = await _context.Sessions
                .Where(s => s.UserId == userId && s.GameRoomId == roomId && s.IsActive)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (sessionId == Guid.Empty)
            {
                _logger.LogWarning("Active session for user {UserId} in room {RoomId} was not found.", userId, roomId);
            }

            return sessionId;
        }

        public async Task<bool> AddUserToSessionAsync(Session session)
        {
            try
            {
                await _context.Sessions.AddAsync(session);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user {UserId} to session in room {RoomId}.", session.UserId, session.GameRoomId);
                return false;
            }
        }

        public async Task<Session?> GetByIdAsync(Guid sessionId)
        {
            return await _context.Sessions.FindAsync(sessionId);
        }

        public async Task DeleteAsync(Session session)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid sessionId)
        {
            // AnyAsync is faster than FindAsync for simple existence checks
            return await _context.Sessions.AnyAsync(s => s.Id == sessionId);
        }

        public async Task<IEnumerable<Guid>> GetUsersInGameRoomAsync(Guid gameRoomId)
        {
            // Removed Task.FromResult - ToListAsync() is the correct way to handle this
            return await _context.Sessions
                .Where(s => s.GameRoomId == gameRoomId)
                .Select(s => s.UserId)
                .ToListAsync();
        }

        public async Task<bool> RemoveUserFromSessionAsync(Guid userId, Guid gameRoomId)
        {
            try
            {
                // Correctly awaited async lookup
                var session = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.GameRoomId == gameRoomId);

                if (session != null)
                {
                    _context.Sessions.Remove(session);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while removing user {UserId} from room {RoomId}.", userId, gameRoomId);
                return false;
            }
        }
    }
}