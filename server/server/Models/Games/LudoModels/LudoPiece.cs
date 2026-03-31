namespace server.Models.Games.LudoModels
{
    public class LudoPiece
    {
        public string Id { get; set; } // Např. "Red_1"
        // 0 = Startovací domeček, 1-40 = Herní pole, 101-104 = Cílový domeček
        public int Position { get; set; }
        public bool IsInHome { get; set; } = true;
        public bool IsFinished { get; set; } = false;
    }
}
