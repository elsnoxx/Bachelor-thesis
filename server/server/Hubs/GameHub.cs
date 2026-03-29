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

        /// <summary>
        /// Jednotná metoda pro zpracování herních dat (GSR senzor, pohyby, atd.)
        /// </summary>
        public async Task SendGameData(string roomId, string gameType, double value)
        {
            // Získáme ID (buď z JWT tokenu/Emailu, nebo connectionId jako fallback)
            var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;

            // 1. Necháme GameManager a příslušnou Service přepočítat stav (včetně těch 30 hodnot)
            var gameState = _gameManager.HandleMove(gameType, roomId, userId, value);

            if (gameState != null)
            {
                Log.Information($"Game state updated for room {roomId} by player {userId}: {gameState}");
                // 2. Rozešleme aktualizovaný, vypočítaný stav celé místnosti
                // Klient dostane: ballPosition, leftValue (průměr), rightValue (průměr), atd.
                await Clients.Group(roomId).SendAsync("ReceiveGameState", gameState);
            }
        }

        //public async Task LudoRollDice(string roomId)
        //{
        //    var game = _gameManager.GetLudoGame(roomId);
        //    var value = game.RollDice(); // Logika v C#

        //    // Pošleme aktualizovaný stav všem v místnosti
        //    await Clients.Group(roomId).SendAsync("UpdateLudoState", game.GetState());
        //}

        //public async Task LudoMovePiece(string roomId, string pieceId)
        //{
        //    var game = _gameManager.GetLudoGame(roomId);
        //    bool success = game.TryMove(pieceId); // Ověření a posun v C#

        //    if (success)
        //    {
        //        await Clients.Group(roomId).SendAsync("UpdateLudoState", game.GetState());
        //    }
        //}
    }
}
