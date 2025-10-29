using System.ComponentModel.DataAnnotations;

namespace server.Models.DB
{
    public class GameRoom
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public string GameType { get; set; } = null!;
        public int MaxPlayers { get; set; } = 2;
        public string Status { get; set; } = "waiting";
        [MaxLength(255)]
        public string? PasswordHash { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid CreatedBy { get; set; }
        public User Creator { get; set; } = null!;

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
