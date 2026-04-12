namespace server.Models.DB
{
    /// <summary>
    /// Records the duration and activity of a user within a specific game room.
    /// Acts as a link for aggregating biofeedback data for a single session.
    /// </summary>
    public class Session
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public Guid GameRoomId { get; set; }
        public virtual GameRoom GameRoom { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Indicates if the player is currently connected and active in the room.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public virtual ICollection<BioFeedback> BioFeedbacks { get; set; } = new List<BioFeedback>();
    }
}