using Serilog;
using server.Models;
using server.Models.DTO;
using server.Services.GameServices;

namespace server.Services.GameServices
{
    public class GameManager
    {
        private readonly Dictionary<string, Player> _connectedPlayers = new();
        private readonly Dictionary<string, HashSet<string>> _gameRooms = new();
        private readonly Dictionary<string, IGameService> _gameServices = new();

        private readonly EnergyBattleGameServices _energyService;

        public GameManager(BallanceGameService ballanceService, EnergyBattleGameServices energyService, BalloonGameService balloonService)
        {
            _gameServices["ballance"] = ballanceService;
            _gameServices["energybattle"] = energyService;
            _gameServices["balloon"] = balloonService;

            _energyService = energyService;
        }

        // TATO METODA TI CHYBĚLA - Hub ji potřebuje pro přístup k Fire()
        public EnergyBattleGameServices GetEnergyService() => _energyService;

        public object? HandleMove(string gameType, string roomId, string playerId, double value)
        {
            if (_gameServices.TryGetValue(gameType.ToLower(), out var service))
            {
                return service.ProcessInput(roomId, playerId, value);
            }
            return null;
        }


        public void AddPlayerToGame(string gameType, string connectionId, string playerName)
        {
            var player = new Player { ConnectionId = connectionId, PlayerName = playerName, GameType = gameType };
            _connectedPlayers[connectionId] = player;

            if (!_gameRooms.ContainsKey(gameType))
                _gameRooms[gameType] = new HashSet<string>();

            _gameRooms[gameType].Add(connectionId);
        }

        public void RemovePlayer(string connectionId)
        {
            if (!_connectedPlayers.ContainsKey(connectionId)) return;
            var gameType = _connectedPlayers[connectionId].GameType;
            _gameRooms[gameType]?.Remove(connectionId);
            _connectedPlayers.Remove(connectionId);
        }
    }
}