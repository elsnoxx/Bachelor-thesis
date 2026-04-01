namespace server.Models.Games
{
    public class EnergyBattleUIUpdate
    {
        public string PlayerEmail { get; set; } = string.Empty;
        public double MyEnergy { get; set; }
        public double MyHealth { get; set; }
        public double MyTargetZone { get; set; }
        public double OpponentHealth { get; set; }
        public bool CanFire { get; set; }
    }
}
