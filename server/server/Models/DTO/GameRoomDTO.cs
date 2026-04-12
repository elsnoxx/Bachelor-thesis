namespace server.Models.DTO
{
    /// <summary>
    /// Represents a simplified view of a game room for display in the lobby or list.
    /// Does not expose sensitive creator details.
    /// </summary>
    public class GameRoomDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string GameType { get; set; }

        /// <summary>
        /// Indicates if the room requires a password to join.
        /// In a real-world scenario, we would only send a boolean 'IsPasswordProtected'.
        /// </summary>
        public string Password { get; set; }

        public int MaxPlayers { get; set; }

        /// <summary>
        /// Number of users currently connected to this room.
        /// </summary>
        public int CurrentPlayers { get; set; }
    }
}