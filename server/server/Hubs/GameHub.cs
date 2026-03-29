using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Serilog;
using System.Security.AccessControl;
using server.Services.GameServices;

namespace server.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameManager _gameManager;

        public GameHub(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        // Metoda, kterou volá tvůj frontend přes .invoke("JoinRoom", roomId)
        public async Task JoinRoom(string roomId)
        {
            // Přidá spojení do logické skupiny SignalR
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            Log.Information($"Client {Context.ConnectionId} joined room: {roomId}");

            // Volitelně informuj ostatní v místnosti
            await Clients.Group(roomId).SendAsync("PlayerJoined", Context.ConnectionId);
        }

        // Metoda pro posílání dat specifických pro EnergyBattle
        public async Task SendEnergyData(string roomId, double value)
        {
            // Získáme ID uživatele z claimů (pokud jsi přihlášen)
            var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;

            var packet = new
            {
                playerId = userId,
                value = value,
                timestamp = DateTime.UtcNow
            };

            // Pošle data VŠEM v dané místnosti (včetně odesílatele)
            await Clients.Group(roomId).SendAsync("ReceiveEnergyData", packet);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Log.Information($"❌ Client disconnected: {Context.ConnectionId}");
            _gameManager.RemovePlayer(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
