using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BioFeedbackGenerator
{
    public class ClientManager
    {
        private readonly List<PlayerSimulator> _players = new();
        private readonly string _hubUrl;

        public ClientManager(string hubUrl)
        {
            _hubUrl = hubUrl;
        }

        public void AddPlayer(string playerId, string playerName, string gameType)
        {
            var player = new Player { Id = playerId, Name = playerName, GameType = gameType };
            var simulator = new PlayerSimulator(player, _hubUrl);
            _players.Add(simulator);
        }

        public async Task StartAllAsync()
        {
            var tasks = _players.Select(p => p.StartAsync());
            await Task.WhenAll(tasks);
        }
    }
}
