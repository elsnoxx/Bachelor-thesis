using server.Models.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server.Repositories.Interfaces
{
    /// <summary>
    /// Interface for managing persistent player statistics and historical game data.
    /// Provides methods for retrieving aggregated performance metrics.
    /// </summary>
    public interface IStatisticRepository
    {
        /// <summary>
        /// Retrieves all statistical records associated with a specific user.
        /// </summary>
        Task<IEnumerable<Statistic>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Finds a specific statistic entry by its unique identifier.
        /// </summary>
        Task<Statistic?> GetByIdAsync(Guid id);

        /// <summary>
        /// Persists a new statistic record (usually called after a game session finishes).
        /// </summary>
        Task AddAsync(Statistic statistic);

        /// <summary>
        /// Helper method to resolve a GameRoom ID from a Session ID.
        /// Useful for linking session results back to the room context.
        /// </summary>
        Task<Guid> GetGameRoomIdBySessionIdAsync(Guid sessionId);
    }
}