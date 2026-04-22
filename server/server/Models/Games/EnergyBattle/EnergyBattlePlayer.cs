namespace server.Models.Games.EnergyBattle
{
    public class EnergyBattlePlayer
    {
        public string Email { get; set; } = string.Empty;
        public double Energy { get; set; } = 0;
        public double Health { get; set; } = 100;
        public double Baseline { get; set; } = 0;
        public double LastValue { get; set; } = 0;
        public bool IsCalibrated { get; set; } = false;
        public List<double> CalibrationData { get; set; } = new();
        public bool IsReadyToFire => Energy >= 100;
    }
}
