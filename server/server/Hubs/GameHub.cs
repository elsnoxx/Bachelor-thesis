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
    /// <summary>
    /// SignalR Hub responsible for real-time multiplayer synchronization.
    /// Handles player connections, sensor data distribution (GSR), and game state updates.
    /// </summary>
    [Authorize] // Requires a valid JWT token for connection
    public class GameHub : Hub
    {
        private readonly GameManager _gameManager;
        private readonly IStatisticService _statisticService;
        private readonly IUserRepository _userRepository;
        private readonly IStatisticService _statisticServices;

        public GameHub(GameManager gameManager, IStatisticService statistic, IUserRepository userRepository, IStatisticService   statisticServices)
        {
            _gameManager = gameManager;
            _statisticService = statistic;
            _userRepository = userRepository;
            _statisticServices = statisticServices;
        }

        /// <summary>
        /// Connects a client to a specific game room group.
        /// </summary>
        /// <param name="roomId">The unique identifier of the game room.</param>
        public async Task JoinRoom(string roomId)
        {
            // Adding connection to a SignalR group for targeted broadcasting
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            Log.Information($"Client {Context.ConnectionId} joined room: {roomId}");

            // Notify other players in the room
            await Clients.Group(roomId).SendAsync("PlayerJoined", Context.ConnectionId);
        }

        /// <summary>
        /// Cleanup logic when a player disconnects (e.g., stops the game or closes the tab).
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Log.Information($"❌ Client disconnected: {Context.ConnectionId}");
            // Clean up player state in the game manager to prevent "ghost" players
            _gameManager.RemovePlayer(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Receives physiological data (EDA/GSR) from the client and updates the game state.
        /// This method is the core loop for the biofeedback interaction.
        /// </summary>
        /// <param name="roomId">Target room ID.</param>
        /// <param name="gameType">Type of the game (e.g., 'balance', 'energybattle').</param>
        /// <param name="value">The processed EDA value from the sensor.</param>
        public async Task SendGameData(string roomId, string gameType, double value)
        {
            // Extracting user identity from the JWT claims
            var userEmail = Context.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail) || !Guid.TryParse(roomId, out Guid roomGuid))
                return;

            // Process the movement/input in the GameManager (In-memory state management)
            var gameState = _gameManager.HandleMove(gameType, roomId, userEmail, value);

            if (gameState != null)
            {
                // Broadcast the updated state to all players in the room
                await Clients.Group(roomId).SendAsync("ReceiveGameState", gameState);
            }
        }

        /// <summary>
        /// Handles specific game actions like firing a cannon in Energy Battle mode.
        /// </summary>
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
