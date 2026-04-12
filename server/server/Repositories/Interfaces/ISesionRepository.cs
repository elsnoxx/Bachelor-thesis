using server.Models.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server.Repositories.Interfaces
{
    /// <summary>
    /// Interface for managing active player sessions within game rooms.
    /// Handles player entry, exit, and session tracking.
    /// </summary>
    public interface ISessionRepository
    {
        /// <summary>
        /// Registers a player's entry into a game room by creating a session record.
        /// </summary>
        Task<bool> AddUserToSessionAsync(Session session);

        /// <summary>
        /// Retrieves a session by its unique identifier.
        /// </summary>
        Task<Session?> GetByIdAsync(Guid sessionId);

        /// <summary>
        /// Permanently removes a session record.
        /// </summary>
        Task DeleteAsync(Session session);

        /// <summary>
        /// Checks if a specific session exists in the database.
        /// </summary>
        Task<bool> ExistsAsync(Guid sessionId);

        /// <summary>
        /// Returns a list of all User IDs currently participating in a specific game room.
        /// </summary>
        Task<IEnumerable<Guid>> GetUsersInGameRoomAsync(Guid gameRoomId);

        /// <summary>
        /// Removes a player from a game room by locating and deleting their active session.
        /// </summary>
        Task<bool> RemoveUserFromSessionAsync(Guid userId, Guid gameRoomId);

        /// <summary>
        /// Finds the active Session ID for a specific user within a specific room.
        /// </summary>
        Task<Guid> GetSessionIdByUserAndRoomAsync(Guid userId, Guid roomId);
    }
}