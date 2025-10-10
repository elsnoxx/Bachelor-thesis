using System.ComponentModel.DataAnnotations;

namespace server.Models.DTO
{
    public class JoinRoomRequest
    {
        [Required]
        public Guid UserId { get; set; }
        public string Password { get; set; }
    }
}
