using server.Models.Games.BalloonGame;
using System.Collections.Concurrent;

namespace server.Models.Games
{
    public class BalloonControlGame
    {
        public string RoomId { get; set; } = string.Empty;
        public ConcurrentDictionary<string, BalloonPlayer> Players { get; set; } = new();
        public bool IsFinished { get; set; }
        public string? WinnerEmail { get; set; }
        public DateTime? StartTime { get; set; }
        public double FinishLineDistance { get; set; } = 1000; // Cílová vzdálenost
        public int MaxPlayers { get; set; } = 2;
        public string? EndReason { get; set; }

    }
}
