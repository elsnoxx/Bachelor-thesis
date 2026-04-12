using System.Text.Json.Serialization;

namespace server.Models.DB
{
    /// <summary>
    /// Aggregated summary of player performance and physiological response.
    /// Calculated after a session ends to provide persistent progress tracking.
    /// </summary>
    public class Statistic
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        public string? GameType { get; set; }

        /// <summary>
        /// Calculated average of EDA/GSR values during the session.
        /// </summary>
        public float? AverageGsr { get; set; }

        /// <summary>
        /// The highest score achieved (e.g., longest duration in balance mode).
        /// </summary>
        public float? BestScore { get; set; }

        public int TotalSessions { get; set; } = 0;
        public DateTime? LastPlayed { get; set; }
    }
}