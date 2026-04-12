using System.ComponentModel.DataAnnotations;

namespace server.Models.DTO
{
    /// <summary>
    /// Data required to initialize and create a new game room.
    /// Includes configuration for game mode and access security.
    /// </summary>
    public class GameRoomCreationDTO
    {
        /// <summary>
        /// The unique ID of the user who is creating the room (Host).
        /// </summary>
        [Required(ErrorMessage = "Creator User ID is required.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Room name is required.")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Defines the game logic to be loaded (e.g., 'Balance', 'EnergyBattle').
        /// </summary>
        [Required(ErrorMessage = "Game type must be specified.")]
        public string GameType { get; set; }

        /// <summary>
        /// Optional password for private game sessions.
        /// </summary>
        public string? Password { get; set; }

        [Range(2, 10, ErrorMessage = "Max players must be between 2 and 10.")]
        public int MaxPlayers { get; set; }
    }
}