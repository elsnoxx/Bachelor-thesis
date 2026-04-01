namespace server.Models.Games.EnergyBattle
{
    public class EnergyBattlePlayer
    {
        public string Email { get; set; }
        public double Health { get; set; } = 100;
        public double Energy { get; set; } = 0;
        public double TargetBioValue { get; set; }
        public bool IsReadyToFire => Energy >= 100;
    }
}
