using System.Collections.Concurrent;

namespace server.Models.Games
{
    public class EnergyBattleGame
    {
        public string RoomId { get; set; } = string.Empty;
        public ConcurrentDictionary<string, EnergyBattlePlayer> Players { get; set; } = new();
        public bool IsFinished { get; set; }
        public string? WinnerEmail { get; set; }
        public DateTime? StartTime { get; set; }

        public string? LeftPlayerId => Players.Keys.FirstOrDefault();
        public string? RightPlayerId => Players.Keys.Skip(1).FirstOrDefault();


        public class EnergyBattlePlayer
        {
            public string Email { get; set; } = string.Empty;
            public double Energy { get; set; } = 0;
            public double Health { get; set; } = 100;
            public double TargetBioValue { get; set; }
            public bool IsReadyToFire => Energy >= 100;
            public DateTime LastTargetChange { get; set; } = DateTime.UtcNow;
        }
    }
}
