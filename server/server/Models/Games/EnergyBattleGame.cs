using server.Models.Games.EnergyBattle;
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

    }
}
