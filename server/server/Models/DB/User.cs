namespace server.Models.DB
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Username { get; set; } = null!;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public ICollection<GameRoom> CreatedRooms { get; set; } = new List<GameRoom>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();
    }
}
