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

        public class BalloonPlayer
        {
            public string Email { get; set; } = string.Empty;
            public double Altitude { get; set; } = 0; // Výška (ovládaná GSR)
            public double DistanceTraveled { get; set; } = 0; // Progres v mapě
            public double LastValue { get; set; } // Pro ukládání biofeedbacku
        }
    }
}
