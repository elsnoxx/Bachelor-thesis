namespace server.Models.DTO
{
    /// <summary>
    /// Comprehensive analysis of a user's physiological response during a session.
    /// Provides metrics for stress level assessment and chart visualization.
    /// </summary>
    public class DetailBioFeedbackData
    {
        public double AverageGsr { get; set; }
        public double MaxGsr { get; set; }
        public double MinGsr { get; set; }

        /// <summary>
        /// Total number of detected stress peaks (SCR - Skin Conductance Responses).
        /// </summary>
        public int PeakCount { get; set; }

        /// <summary>
        /// Duration in seconds during which the user's GSR was significantly above their baseline.
        /// </summary>
        public double TimeAboveThreshold { get; set; }

        /// <summary>
        /// Processed and smoothed data points ready for frontend chart rendering.
        /// </summary>
        public List<BioPoint> ChartData { get; set; } = new();

        /// <summary>
        /// Calculated resting skin conductance level used as a reference point for this session.
        /// </summary>
        public double Baseline { get; set; }
    }
}