using Microsoft.AspNetCore.Mvc.Formatters;

namespace server.Models.DTO
{
    public class GameResultContext
    {
        public string roomId { get; set; }
        public string LeftPlayerId { get; set; }
        public string RightPlayerId { get; set; }
        public string GameType { get; set; }

        public GameResultContext(string roomId, string leftPlayerId, string rightPlayerId, string gameType)
        {
            this.roomId = roomId;
            LeftPlayerId = leftPlayerId;
            RightPlayerId = rightPlayerId;
            GameType = gameType;
        }
    }
}
