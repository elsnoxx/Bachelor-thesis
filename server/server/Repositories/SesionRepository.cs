using Serilog;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
    public class SesionRepository : ISesionRepository
    {
        private readonly AppDbContext _context;

        public SesionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddUserToSesion(Session session)
        {
            try
            {
                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding user to session: {ex}");
                return false;
            }
        }

        public async Task<Session?> GetSesionByIdAsync(Guid sessionId)
        {
            return await _context.Sessions.FindAsync(sessionId);
        }

        public async Task DeleteSesionAsync(Session session)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SesionExistsAsync(Guid sessionId)
        {
            return await _context.Sessions.FindAsync(sessionId) != null;
        }

        public async Task<IEnumerable<Guid>> GetUsersInGameRoomAsync(Guid gameRoomId)
        {
            return await Task.FromResult(_context.Sessions
                .Where(s => s.GameRoomId == gameRoomId)
                .Select(s => s.UserId)
                .ToList());
        }

        public async Task<bool> RemoveUserFromSesion(Guid userId, Guid gameRoomId)
        {
            try
            {
                var session = _context.Sessions.FirstOrDefault(s => s.UserId == userId && s.GameRoomId == gameRoomId);
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
                Log.Error($"Error removing user from session: {ex}");
                return false;
            }
        }
    }
}
