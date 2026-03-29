namespace server.Models.DTO
{
    public class BioPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public bool IsPeak { get; set; }
    }
}
