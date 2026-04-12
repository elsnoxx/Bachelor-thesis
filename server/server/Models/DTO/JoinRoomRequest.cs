using System.ComponentModel.DataAnnotations;

namespace server.Models.DTO
{
    /// <summary>
    /// Request object used when a player attempts to enter a specific room.
    /// </summary>
    public class JoinRoomRequest
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        /// <summary>
        /// Password provided by the user for private rooms.
        /// </summary>
        public string? Password { get; set; }
    }
}