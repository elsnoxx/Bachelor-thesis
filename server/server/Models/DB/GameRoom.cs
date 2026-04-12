using System.ComponentModel.DataAnnotations;

namespace server.Models.DB
{
    /// <summary>
    /// Represents a multiplayer game instance. Manages the room's lifecycle,
    /// player capacity, and access control (password).
    /// </summary>
    public class GameRoom
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Specific game mode (e.g., 'Balance', 'EnergyBattle').
        /// </summary>
        public string GameType { get; set; } = null!;

        public int MaxPlayers { get; set; } = 2;

        /// <summary>
        /// Current state of the room: 'Waiting', 'InGame', 'Finished'.
        /// </summary>
        public string Status { get; set; } = "Waiting";

        /// <summary>
        /// Optional hashed password for private game rooms.
        /// </summary>
        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The user who created the room and has administrative privileges.
        /// </summary>
        public Guid CreatedBy { get; set; }
        public virtual User Creator { get; set; } = null!;

        // Navigation properties for related entities
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
        public virtual ICollection<BioFeedback> BioFeedbacks { get; set; } = new List<BioFeedback>();
    }
}