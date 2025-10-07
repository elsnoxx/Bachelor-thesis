using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Serilog;
using System.Security.AccessControl;
using server.Services.GameServices;

namespace server.Hubs
{
    public class GameHub : Hub
    {
        public readonly GameManager _gameManager;

        public GameHub(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        // Když se připojí klient
        public override async Task OnConnectedAsync()
        {
            Log.Information($"Client connected: {Context.ConnectionId}");
            
            await base.OnConnectedAsync();
        }

        public async Task JoinGame(string playerId, string gameType)
        {
            Log.Information($"[{playerId}] joined the game.");
            _gameManager.AddPlayerToGame(gameType, Context.ConnectionId, playerId);
            await Clients.Others.SendAsync("PlayerJoined", playerId, gameType);
        }

        // Když klient pošle biofeedback data
        public async Task SendBioData(string playerId, string gameType, double value)
        {
            Log.Information($"[{playerId}] → {value}");

            // Pošli všem ostatním
            _gameManager.GetPlayersInGame(gameType);
            await Clients.Others.SendAsync("ReceiveBioData", playerId, value);
        }

        // Když se klient odpojí
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Log.Information($"❌ Client disconnected: {Context.ConnectionId}");
            _gameManager.RemovePlayer(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

    }
}
