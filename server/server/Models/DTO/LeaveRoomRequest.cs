using System.ComponentModel.DataAnnotations;

namespace server.Models.DTO
{
    /// <summary>
    /// Request object used when a player intentionally leaves a room.
    /// </summary>
    public class LeaveRoomRequest
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }
    }
}
