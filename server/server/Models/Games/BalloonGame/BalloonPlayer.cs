namespace server.Models.Games.BalloonGame
{
    public class BalloonPlayer
    {
        public string Email { get; set; } = string.Empty;
        public double Altitude { get; set; } = 0;
        public double DistanceTraveled { get; set; } = 0;
        public double LastValue { get; set; }
        public double Baseline { get; set; } = 0;
        public bool IsCalibrated { get; set; } = false;
        public List<double> CalibrationData { get; set; } = new();
    }
}
