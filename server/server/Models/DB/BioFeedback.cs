using System.Text.Json.Serialization;

namespace server.Models.DB
{
    /// <summary>
    /// Represents a high-frequency telemetry data point of Electrodermal Activity (EDA).
    /// Stores the skin conductance value linked to a specific user and game session.
    /// </summary>
    public class BioFeedback
    {
        public int Id { get; set; }

        /// <summary>
        /// Reference to the GameRoom where the data was recorded.
        /// </summary>
        public Guid GameRoomId { get; set; }

        /// <summary>
        /// Reference to the User who produced the physiological signal.
        /// </summary>
        public Guid UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [JsonIgnore]
        public virtual GameRoom GameRoom { get; set; } = null!;

        /// <summary>
        /// Measured Skin Conductance (GSR) value, typically in microsiemens.
        /// </summary>
        public float GsrValue { get; set; }

        /// <summary>
        /// UTC timestamp of the measurement for precise time-series analysis.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}