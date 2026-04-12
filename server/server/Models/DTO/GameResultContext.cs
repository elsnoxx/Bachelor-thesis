namespace server.Models.DTO
{
    /// <summary>
    /// Contextual data used for finalizing a game and persisting results.
    /// Identifies players and their positions/roles in the completed session.
    /// </summary>
    public class GameResultContext
    {
        public string RoomId { get; set; }
        public string LeftPlayerId { get; set; }
        public string RightPlayerId { get; set; }
        public string GameType { get; set; }

        public GameResultContext(string roomId, string leftPlayerId, string rightPlayerId, string gameType)
        {
            RoomId = roomId;
            LeftPlayerId = leftPlayerId;
            RightPlayerId = rightPlayerId;
            GameType = gameType;
        }
    }
}