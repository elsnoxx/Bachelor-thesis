using server.Models.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server.Repositories.Interfaces
{
    /// <summary>
    /// Interface for managing game room persistence.
    /// Handles CRUD operations and complex queries for the lobby and matchmaking.
    /// </summary>
    public interface IGameRoomRepository
    {
        /// <summary>
        /// Retrieves all available game rooms of a specific type that are currently in the 'Waiting' status.
        /// </summary>
        Task<IEnumerable<GameRoom>> GetAvailableRoomsAsync(string gameType);

        /// <summary>
        /// Set game room status to 'InProgress' when the game starts, and update it to 'Finished' when the game ends.
        /// </summary>
        Task UpdateRoomStatusAsync(Guid roomId, string newStatus);

        /// <summary>
        /// Persists a new game room instance.
        /// </summary>
        Task CreateAsync(GameRoom gameRoom);

        /// <summary>
        /// Finds a specific game room by its unique identifier.
        /// </summary>
        Task<GameRoom?> GetByIdAsync(Guid roomId);

        /// <summary>
        /// Checks if a game room exists in the database.
        /// </summary>
        Task<bool> ExistsAsync(Guid roomId);

        /// <summary>
        /// Removes a game room record.
        /// </summary>
        Task DeleteAsync(GameRoom gameRoom);

        /// <summary>
        /// Updates an existing game room record (e.g., status changes or player joining).
        /// </summary>
        Task UpdateAsync(GameRoom gameRoom);
    }
}