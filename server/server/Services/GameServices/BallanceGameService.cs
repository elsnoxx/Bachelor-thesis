using Microsoft.Extensions.DependencyInjection;
using Serilog;
using server.Models.DTO;
using server.Models.Games;
using server.Services.Utils;

namespace server.Services.GameServices
{
    public class BallanceGameService : BaseGameService<BalanceGame>
    {
        public BallanceGameService(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue) : base(scopeFactory, dbQueue) { }

        protected override BalanceGame CreateGame(string roomId) => new() { RoomId = roomId };

        public override object? ProcessInput(string roomId, string playerId, double value)
        {
            var game = GetOrCreateGame(roomId);

            // 1. Přiřazení hráčů
            if (string.IsNullOrEmpty(game.LeftPlayerId))
                game.LeftPlayerId = playerId;
            else if (string.IsNullOrEmpty(game.RightPlayerId) && game.LeftPlayerId != playerId)
                game.RightPlayerId = playerId;

            // 2. Start hry (včetně kalibrace)
            if (game.StartTime == null && !string.IsNullOrEmpty(game.LeftPlayerId) && !string.IsNullOrEmpty(game.RightPlayerId))
            {
                game.StartTime = DateTime.UtcNow;
                game.GenerateLimits();
                Log.Information("[GAME START] Room: {RoomId}", roomId);
                NotifyRoomStatus(roomId, RoomStatus.Start);
            }

            // 3. Zpracování dat, pokud hra běží a neskončila
            if (game.StartTime != null && !game.IsGameOver)
            {
                game.AddValue(playerId, value);
                SaveBioFeedback(playerId, roomId, value);
            }

            // 4. LOGIKA PRO ULOŽENÍ VÝSLEDKU (PŘIDÁNO SEM)
            if (game.IsGameOver && !game.WasSaved)
            {
                game.WasSaved = true; // Označíme, že ukládáme, aby se to neopakovalo

                // Určíme vítěze (v Balance hře vyhrávají buď oba, nebo nikdo)
                // Pokud isWin = true, posíláme e-mail jednoho z nich (nebo null pro oba)
                string? winnerEmail = game.GetState() is var s && (bool)((dynamic)s).isWin
                    ? game.LeftPlayerId
                    : null;

                // Zavoláme metodu z BaseGameService
                SaveGameResult(
                    roomId,
                    game.LeftPlayerId,
                    game.RightPlayerId,
                    "ballance",
                    winnerEmail
                );

                // Notifikujeme DB, že místnost skončila
                NotifyRoomStatus(roomId, RoomStatus.Finish);

                Log.Information("[GAME ENDED & SAVED] Room: {RoomId}, Result: {Result}", roomId, winnerEmail != null ? "Victory" : "Loss");
            }

            return game.GetState();
        }

        public void RemoveRoom(string roomId) => RemoveGame(roomId);  // <-- base method

    }
}