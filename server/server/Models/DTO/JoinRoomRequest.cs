using System.ComponentModel.DataAnnotations;

namespace server.Models.DTO
{
    public class JoinRoomRequest
    {
        [Required]
        public string UserEmail { get; set; }
        public string? Password { get; set; }
    }
}
