using System.ComponentModel.DataAnnotations;

namespace server.Models.DB
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = null!;
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "User";
        public string? AvatarUrl { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [DataType(DataType.DateTime)]
        public DateTime? LastLogin { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<GameRoom> CreatedRooms { get; set; } = new List<GameRoom>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();
    }
}
