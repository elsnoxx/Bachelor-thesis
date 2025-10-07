using Serilog;
using server.Services.GameServices;
using server.Models;

namespace server.Services.GameServices
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


        // Přidání hráče do hry
        public void AddPlayerToGame(string gameType, string connectionId, string playerName)
        {
            var player = new Player { ConnectionId = connectionId, PlayerName = playerName, GameType = gameType };
            _connectedPlayers[connectionId] = player;

            if (!_gameRooms.ContainsKey(gameType))
                _gameRooms[gameType] = new HashSet<string>();

            _gameRooms[gameType].Add(connectionId);
        }

        // Odebrání hráče při odpojení
        public void RemovePlayer(string connectionId)
        {
            if (!_connectedPlayers.ContainsKey(connectionId)) return;
            var gameType = _connectedPlayers[connectionId].GameType;
            _gameRooms[gameType]?.Remove(connectionId);
            _connectedPlayers.Remove(connectionId);
        }
        public void GetPlayersInGame(string gameType)
        {
            foreach (var player in _connectedPlayers.Values)
            {
                Log.Information($"{player.PlayerName}");
            }
        }

    }
}
