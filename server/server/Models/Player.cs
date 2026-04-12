namespace server.Models.DTO
{
    /// <summary>
    /// Represents an active player session within the real-time communication layer.
    /// Maps a SignalR connection to a specific user and their current game mode.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Unique ID assigned by the SignalR hub to this specific connection.
        /// </summary>
        public string ConnectionId { get; set; }

        public string PlayerName { get; set; }

        /// <summary>
        /// The identifier of the game mode the player is currently participating in.
        /// </summary>
        public string GameType { get; set; }
    }
}