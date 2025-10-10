namespace server.Models.DB
{
    public class Session
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid GameRoomId { get; set; }
        public GameRoom GameRoom { get; set; } = null!;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<BioFeedback> BioFeedbacks { get; set; } = new List<BioFeedback>();
    }
}
