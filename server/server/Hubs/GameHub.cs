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

        // Přidej si pomocnou vlastnost do GameHub.cs
        private async Task<Guid?> GetUserGuidFromEmail()
        {
            // 1. Vytáhneme email z claimů (podle toho, co vidíš v logu)
            var email = Context.User?.Identity?.Name
                     ?? Context.User?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email)) return null;

            // 2. Najdeme uživatele v DB přes tvou existující UserRepository
            var user = await _userRepository.GetByEmailAsync(email);
            return user?.Id;
        }
    }
}
