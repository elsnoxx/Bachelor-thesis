namespace server.Models.DTO
{
    public class DetailBioFeedbackData
    {
        public double AverageGsr { get; set; }
        public double MaxGsr { get; set; }
        public double MinGsr { get; set; }
        public int PeakCount { get; set; } // Počet stresových špiček
        public double TimeAboveThreshold { get; set; } // Sekundy v "stresu"
        public List<BioPoint> ChartData { get; set; } // Vyhlazená data pro graf
        public double Baseline { get; set; } // Klidová hodnota
    }
}
