using server.Models.Games;
using System.Collections.Concurrent;

namespace server.Services.GameServices
{
    // Příklad pro Ballance (podobně uděláš EnergyBattle)
    public class BallanceGameService : IGameService
    {
        // Mapa: RoomId -> Instance hry
        private readonly ConcurrentDictionary<string, BalanceGame> _activeGames = new();

        // Tady je ta metoda, na kterou ses ptal
        private BalanceGame GetOrCreateGame(string roomId)
        {
            return _activeGames.GetOrAdd(roomId, id => new BalanceGame { RoomId = id });
        }

        public object ProcessInput(string roomId, string playerId, double value)
        {
            var game = GetOrCreateGame(roomId);

            // Dynamické přiřazení stran (kdo dřív přijde, ten je vlevo)
            if (string.IsNullOrEmpty(game.LeftPlayerId))
                game.LeftPlayerId = playerId;
            else if (string.IsNullOrEmpty(game.RightPlayerId) && game.LeftPlayerId != playerId)
                game.RightPlayerId = playerId;

            // Přidáme hodnotu do historie (toho tvých 30 vzorků)
            game.AddValue(playerId, value);

            // Vrátíme anonymní objekt, který SignalR pošle na frontend
            return new
            {
                roomId = game.RoomId,
                ballPosition = game.GetBallPosition(),
                leftValue = game.LeftValue,   // Toto je už ten průměr z 30 hodnot
                rightValue = game.RightValue, // Toto také
                leftPlayerId = game.LeftPlayerId,
                rightPlayerId = game.RightPlayerId,
                isGameOver = game.IsGameOver
            };
        }

        public void RemoveRoom(string roomId)
        {
            _activeGames.TryRemove(roomId, out _);
        }

        // Pokud tvůj interface vyžaduje GetScores, implementuj ho takto:
        public Dictionary<string, double> GetScores()
        {
            // Pro ballance můžeme vrátit např. aktuální průměry všech her
            return new Dictionary<string, double>();
        }
    }
}
