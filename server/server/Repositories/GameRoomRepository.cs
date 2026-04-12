using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models.DB;
using server.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Repositories
{
    /// <summary>
    /// Implementation of game room data access using Entity Framework Core.
    /// Includes optimizations for loading related player session data.
    /// </summary>
    public class GameRoomRepository : IGameRoomRepository
    {
        private readonly AppDbContext _context;

        public GameRoomRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Updates the game room state. 
        /// Uses asynchronous SaveChanges to ensure non-blocking I/O operations.
        /// </summary>
        public async Task UpdateAsync(GameRoom gameRoom)
        {
            _context.GameRooms.Update(gameRoom);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Fetches rooms with 'Waiting' status. 
        /// Uses .AsSplitQuery() to optimize performance when including multiple collections.
        /// </summary>
        public async Task<IEnumerable<GameRoom>> GetAvailableRoomsAsync(string gameType)
        {
            return await _context.GameRooms
                .Where(gr => gr.GameType == gameType && gr.Status == "Waiting")
                .Include(gr => gr.Sessions)
                    .ThenInclude(s => s.User)
                .AsSplitQuery() // Prevents "Cartesian explosion" when loading large sets of related data
                .ToListAsync();
        }

        public async Task CreateAsync(GameRoom gameRoom)
        {
            _context.GameRooms.Add(gameRoom);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Efficiently checks for existence using AnyAsync instead of loading the whole entity.
        /// </summary>
        public async Task<bool> ExistsAsync(Guid roomId)
        {
            return await _context.GameRooms.AnyAsync(gr => gr.Id == roomId);
        }

        public async Task<GameRoom?> GetByIdAsync(Guid roomId)
        {
            return await _context.GameRooms.FindAsync(roomId);
        }

        public async Task DeleteAsync(GameRoom gameRoom)
        {
            _context.GameRooms.Remove(gameRoom);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoomStatusAsync(Guid roomId, string newStatus)
        {
            var gameRoom = await _context.GameRooms.FindAsync(roomId);
            if (gameRoom != null)
            {
                gameRoom.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }
    }
}