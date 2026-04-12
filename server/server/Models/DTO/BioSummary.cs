namespace server.Models.DTO
{
    /// <summary>
    /// Minimal summary of a biofeedback session for quick dashboard views.
    /// </summary>
    public class BioSummary
    {
        public float Avg { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
    }
}
