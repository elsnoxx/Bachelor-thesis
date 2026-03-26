using System.ComponentModel.DataAnnotations;

namespace server.Models.DTO
{
    public class GameRoomCreationDTO
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string GameType { get; set; }
        public string password { get; set; }
        public int MaxPlayers { get; set; }
    }
}
