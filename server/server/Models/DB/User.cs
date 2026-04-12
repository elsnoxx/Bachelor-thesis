using System.ComponentModel.DataAnnotations;

namespace server.Models.DB
{
    /// <summary>
    /// Core user entity containing identity, authentication state, and role management.
    /// Serves as the central point for all player-generated data.
    /// </summary>
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = null!;

        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = "User";
        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public bool IsEmailConfirmed { get; set; } = false;
        public Guid EmailConfirmationToken { get; set; } = Guid.NewGuid();

        // One-to-Many relationships
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<GameRoom> CreatedRooms { get; set; } = new List<GameRoom>();
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
        public virtual ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();
        public virtual ICollection<BioFeedback> BioFeedbacks { get; set; } = new List<BioFeedback>();
    }
}