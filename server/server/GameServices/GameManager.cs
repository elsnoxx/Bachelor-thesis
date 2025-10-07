using server.Models;

namespace server.GameServices
{
    public class GameManager
    {
        // Připojení hráčů: ConnectionId -> Player
        private readonly Dictionary<string, Player> _connectedPlayers = new();

        // Hra -> room -> hráči
        private readonly Dictionary<string, HashSet<string>> _gameRooms = new();

        // Hra -> Service
        private readonly Dictionary<string, IGameService> _gameServices = new();

        public GameManager()
        {
            // Inicializace herních služeb
            _gameServices["EnergyBattle"] = new LudoGameServices();
            // Můžeš přidat další hry zde
        }
    }
}
