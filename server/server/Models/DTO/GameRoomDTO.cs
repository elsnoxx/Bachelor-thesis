using System.ComponentModel.DataAnnotations;

namespace server.Models.DTO
{
    public class GameRoomDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string GameType { get; set; }
        public string password { get; set; }
        public int MaxPlayers { get; set; }
    }
}
