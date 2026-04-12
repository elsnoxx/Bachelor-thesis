namespace server.Models.DTO
{
    /// <summary>
    /// Data Transfer Object representing a single physiological measurement point.
    /// Used for capturing and transmitting EDA (GSR) values in real-time.
    /// </summary>
    public class BioData
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique identifier of the player who generated the data.
        /// </summary>
        public Guid PlayerId { get; set; }

        /// <summary>
        /// The skin conductance value recorded at a specific moment.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Precise timestamp of the recording (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}