namespace server.Models.Games.LudoModels
{
    public class LudoPlayer
    {
        public string PlayerId { get; set; } // Email nebo Guid
        public string Name { get; set; }
        public LudoColor Color { get; set; }
        public List<LudoPiece> Pieces { get; set; } = new();
        public bool IsReady { get; set; } = false;
    }
}
