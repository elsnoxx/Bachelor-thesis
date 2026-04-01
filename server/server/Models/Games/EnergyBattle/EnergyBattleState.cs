namespace server.Models.Games.EnergyBattle
{
    public class EnergyBattleState
    {
        public string RoomId { get; set; }
        public Dictionary<string, EnergyBattlePlayer> Players { get; set; } = new();
        public bool IsFinished { get; set; } = false;
        public string WinnerEmail { get; set; }
    }
}
