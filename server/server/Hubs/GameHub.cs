using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Serilog;

namespace server.Hubs
{
    public class GameHub : Hub
    {
        // Když se připojí klient
        public override async Task OnConnectedAsync()
        {
            Log.Information($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public async Task JoinGame(string playerId)
        {
            Log.Information($"[{playerId}] joined the game.");
            await Clients.Others.SendAsync("PlayerJoined", playerId);
        }

        // Když klient pošle biofeedback data
        public async Task SendBioData(string playerId, string gameType ,double value)
        {
            Log.Information($"[{playerId}] → {value}");

            // Pošli všem ostatním
            await Clients.Others.SendAsync("ReceiveBioData", playerId, value);
        }

        // Když se klient odpojí
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Log.Information($"❌ Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
