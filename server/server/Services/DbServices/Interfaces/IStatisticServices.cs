using server.Models.DB;
using server.Models.DTO;

namespace server.Services.DbServices.Interfaces
{
    /// <summary>
    /// Service for processing physiological telemetry and player performance metrics.
    /// Handles data aggregation, peak detection, and statistical summaries.
    /// </summary>
    public interface IStatisticService
    {
        /// <summary>
        /// Links and stores game performance statistics for a specific user.
        /// </summary>
        Task AddStatisticByEmailAsync(string email, Statistic statistic);

        /// <summary>
        /// Captures a single biofeedback data point (e.g., GSR value) during gameplay.
        /// </summary>
        Task AddBioFeedbackByEmailAsync(string email, Guid roomGuid, float value);

        /// <summary>
        /// Retrieves all historical game statistics for a specific user.
        /// </summary>
        Task<IEnumerable<Statistic>> GetUserStatsAsync(string userEmail);

        /// <summary>
        /// Retrieves all raw biofeedback records associated with a user.
        /// </summary>
        Task<IEnumerable<BioFeedback>> GetUserBiofeedbackAsync(string userEmail);

        /// <summary>
        /// Generates a detailed analytics report for a specific statistic record, including peak detection and moving averages.
        /// </summary>
        Task<DetailBioFeedbackData> GetBioSummaryAsync(string userEmail, string statisticId);

        /// <summary>
        /// Calculates basic summary (Min, Max, Avg) for a specific statistic record and user.
        /// </summary>
        Task<BioSummary?> GetSessionSummaryAsync(string email, Guid roomId);
    }
}