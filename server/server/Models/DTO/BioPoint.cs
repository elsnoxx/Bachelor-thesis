namespace server.Models.DTO
{
    /// <summary>
    /// Represents a single point in a time-series chart of biofeedback data.
    /// Includes metadata for identifying stress peaks.
    /// </summary>
    public class BioPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }

        /// <summary>
        /// Indicates if this point represents a Skin Conductance Response (SCR) peak.
        /// </summary>
        public bool IsPeak { get; set; }
    }
}
