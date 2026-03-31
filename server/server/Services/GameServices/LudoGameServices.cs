using server.Models.Games;
using System;
using System.Collections.Generic;

namespace server.Services.GameServices
{
    public class LudoGameServices : IGameService
    {
        private readonly Dictionary<string, LudoGame> _activeGames = new();

        public object ProcessInput(string roomId, string playerId, double value)
        {
            // V Ludo 'value' může být např. akce: 1 = Hod kostkou, 2 = Pohyb
            // Ale pro čistší kód doporučuji přidat do IGameService metody specifické pro Ludo
            return null;
        }

        // Doporučené metody pro GameHub:
        public object RollDice(string roomId, string playerId)
        {
            if (!_activeGames.TryGetValue(roomId, out var game)) return null;
            game.RollDice(playerId);
            return game.GetState();
        }

        public object MovePiece(string roomId, string playerId, string pieceId)
        {
            if (!_activeGames.TryGetValue(roomId, out var game)) return null;
            game.MovePiece(playerId, pieceId);
            return game.GetState();
        }

        public Dictionary<string, double> GetScores()
        {
            return new Dictionary<string, double>();
        }
    }
}