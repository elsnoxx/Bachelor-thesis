using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;

namespace server.Repositories
{
    public class GameRoomRepository : IGameRoomRepository
    {
        private readonly AppDbContext _context;

        public GameRoomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GameRoom>> AllGameRoomsAsync(string gameType)
        {
            return await _context.GameRooms
                .Where(gr => gr.GameType == gameType)
                .Include(gr => gr.Sessions)
                .ThenInclude(s => s.User)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task CreateGameRoomAsync(GameRoom gameRoom)
        {
            _context.GameRooms.Add(gameRoom);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GameRoomExistsAsync(Guid roomId)
        {
            return await _context.GameRooms.FindAsync(roomId) != null;
        }

        public async Task<GameRoom> GameRoomById(Guid roomId)
        {
            return await _context.GameRooms.FindAsync(roomId);
        }

        public async Task DeleteGameRoomAsync(GameRoom gameRoom)
        {
            _context.GameRooms.Remove(gameRoom);
            await _context.SaveChangesAsync();
        }
    }
}
