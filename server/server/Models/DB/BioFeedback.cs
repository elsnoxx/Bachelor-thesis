namespace server.Models.DB
{
    public class BioFeedback
    {
        public int Id { get; set; }

        public Guid SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public float GsrValue { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
