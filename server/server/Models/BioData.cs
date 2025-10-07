namespace server.Models
{
    public class BioData
    {
        public int Id { get; set; }
        public Guid PlayerId { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
