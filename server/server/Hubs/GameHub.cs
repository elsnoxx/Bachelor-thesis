using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using server.Repositories;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;
using server.Services.GameServices;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Threading.Tasks;

namespace server.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly GameManager _gameManager;
        private readonly IStatisticServices _statisticService;
        private readonly IUserRepository _userRepository;
        private readonly IStatisticServices _statisticServices;

        public GameHub(GameManager gameManager, IStatisticServices statistic, IUserRepository userRepository, IStatisticServices statisticServices)
        {
            _gameManager = gameManager;
            _statisticService = statistic;
            _userRepository = userRepository;
            _statisticServices = statisticServices;
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

        public async Task SendEnergyData(string roomId, double value)
        {
            var userEmail = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return;

            // Získáme surový stav hry (seznam hráčů)
            var result = _gameManager.HandleMove("energybattle", roomId, userEmail, value);

            if (result != null)
            {
                await Clients.Group(roomId).SendAsync("ReceiveGameState", result);
            }
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
            // 1. Získáme email z tokenu (to, co máš v logu r@r.cz)
            var userEmail = Context.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail) || !Guid.TryParse(roomId, out Guid roomGuid))
                return;

            

            // 3. Zpracujeme pohyb v paměti (GameManager pracuje se stringem/emailem jako ID)
            var gameState = _gameManager.HandleMove(gameType, roomId, userEmail, value);

            if (gameState != null)
            {
                await Clients.Group(roomId).SendAsync("ReceiveGameState", gameState);
            }
        }

        public async Task FireCannon(string roomId)
        {
            var userEmail = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return;

            var energyService = _gameManager.GetEnergyService();
            var gameState = energyService.Fire(roomId, userEmail);

            if (gameState != null)
            {
                await Clients.Group(roomId).SendAsync("ReceiveGameState", gameState);

            }
        }

    }
}
