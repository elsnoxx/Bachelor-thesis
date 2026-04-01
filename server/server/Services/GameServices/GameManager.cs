using Serilog;
using server.Services.GameServices;
using server.Models;

namespace server.Services.GameServices
{
    public class GameManager
    {
        private readonly Dictionary<string, Player> _connectedPlayers = new();
        private readonly Dictionary<string, HashSet<string>> _gameRooms = new();
        private readonly Dictionary<string, IGameService> _gameServices = new();
        private readonly Dictionary<string, string> _roomToGameType = new();

        private readonly LudoGameServices _ludoService;
        private readonly EnergyBattleGameServices _energyService;

        public GameManager(BallanceGameService ballanceService, EnergyBattleGameServices energyService, LudoGameServices ludoService)
        {
            _gameServices["ballance"] = ballanceService;
            _gameServices["energybattle"] = energyService;
            _gameServices["ludo"] = ludoService;

            _ludoService = ludoService;
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

        // Přidána kontrola na null, aby zmizely warningy
        public object? LudoRollDice(string roomId, string? playerId)
        {
            if (string.IsNullOrEmpty(playerId)) return null;
            return _ludoService.RollDice(roomId, playerId);
        }

        public object? LudoMovePiece(string roomId, string? playerId, string pieceId)
        {
            if (string.IsNullOrEmpty(playerId)) return null;
            return _ludoService.MovePiece(roomId, playerId, pieceId);
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