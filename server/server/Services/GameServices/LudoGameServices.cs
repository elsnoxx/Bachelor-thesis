using System;
using System.Collections.Generic;

namespace server.Services.GameServices
{
    public class LudoGameServices : IGameService
    {
        private readonly Dictionary<string, double> _scores = new();

        public LudoGameServices() { }

        public void ProcessInput(string playerId, double value)
        {
            _scores[playerId] = value;
        }

        public Dictionary<string, double> GetScores()
        {
            return new Dictionary<string, double>(_scores);
        }
    }
}