using server.Models.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server.Repositories.Interfaces
{
    /// <summary>
    /// Interface for accessing and managing BioFeedback data in the persistent storage.
    /// Provides abstraction over the database context for physiological telemetry.
    /// </summary>
    public interface IBiofeedbackRepository
    {
        /// <summary>
        /// Retrieves all biofeedback records for a specific user, ordered by most recent.
        /// </summary>
        Task<IEnumerable<BioFeedback>> GetUserStatisticsAsync(Guid userId);

        /// <summary>
        /// Retrieves telemetry data associated with a specific user and game session.
        /// </summary>
        /// <param name="userId">Unique ID of the player.</param>
        /// <param name="sessionId">ID of the game room or session.</param>
        Task<IEnumerable<BioFeedback>> GetBySessionAsync(Guid userId, Guid sessionId);

        /// <summary>
        /// Persists a new biofeedback data point to the database.
        /// </summary>
        Task AddAsync(BioFeedback bioFeedback);
    }
}