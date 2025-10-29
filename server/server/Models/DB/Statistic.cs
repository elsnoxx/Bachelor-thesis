namespace server.Models.DB
{
    public class Statistic
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string? GameType { get; set; }
        public float? AverageGsr { get; set; }
        public float? BestScore { get; set; }
        public int TotalSessions { get; set; } = 0;
        public DateTime? LastPlayed { get; set; }
    }
}
