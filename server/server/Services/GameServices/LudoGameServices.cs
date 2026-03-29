using System;
using System.Collections.Generic;

namespace server.Services.GameServices
{
    public class LudoGameServices : IGameService
    {
        private readonly Dictionary<string, double> _scores = new();


        public object ProcessInput(string roomId, string playerId, double value)
        {
            _scores[playerId] = value;
            return new {value};
        }

        public Dictionary<string, double> GetScores()
        {
            return new Dictionary<string, double>(_scores);
        }
    }
}