using server.Models.Games.LudoModels;

namespace server.Models.Games
{
    public class LudoGame
    {
        public string RoomId { get; set; }
        public List<LudoPlayer> Players { get; set; } = new();
        public int CurrentPlayerIndex { get; set; } = 0;
        public int LastDiceRoll { get; set; } = 0;
        public bool WaitingForMove { get; set; } = false; // Čeká se, až hráč vybere figurku
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        // Metoda pro inicializaci hry pro 2-4 hráče
        public void SetupGame(List<(string id, string name)> playerInfos)
        {
            var colors = Enum.GetValues<LudoColor>().ToList();
            for (int i = 0; i < playerInfos.Count; i++)
            {
                var p = new LudoPlayer
                {
                    PlayerId = playerInfos[i].id,
                    Name = playerInfos[i].name,
                    Color = colors[i],
                    Pieces = Enumerable.Range(1, 4).Select(num => new LudoPiece
                    {
                        Id = $"{colors[i]}_{num}",
                        Position = 0
                    }).ToList()
                };
                Players.Add(p);
            }
        }

        public LudoPlayer GetCurrentPlayer() => Players[CurrentPlayerIndex];

        // Logika hodu kostkou
        public int RollDice(string playerId)
        {
            if (GetCurrentPlayer().PlayerId != playerId || WaitingForMove) return 0;

            Random rnd = new Random();
            LastDiceRoll = rnd.Next(1, 7);

            // Pokud hráč nemá žádný možný tah, přepneme rovnou na dalšího (zjednodušeno)
            // Jinak čekáme na MovePiece
            WaitingForMove = true;
            return LastDiceRoll;
        }

        // Logika pohybu
        public bool MovePiece(string playerId, string pieceId)
        {
            var player = GetCurrentPlayer();
            if (player.PlayerId != playerId || !WaitingForMove) return false;

            var piece = player.Pieces.FirstOrDefault(p => p.Id == pieceId);
            if (piece == null) return false;

            // ZDE: Implementace pravidel (nasazení 6kou, vyhazování, posun do cíle)
            // piece.Position += LastDiceRoll;

            CheckCollisions(piece);

            WaitingForMove = false;
            NextTurn();
            return true;
        }

        private void NextTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            LastDiceRoll = 0;
        }

        private void CheckCollisions(LudoPiece movedPiece)
        {
            // Logika pro vyhození figurky jiného hráče zpět na 0
        }

        // Snapshot pro frontend
        public object GetState()
        {
            return new
            {
                roomId = RoomId,
                currentPlayerId = GetCurrentPlayer().PlayerId,
                lastRoll = LastDiceRoll,
                waitingForMove = WaitingForMove,
                players = Players.Select(p => new {
                    id = p.PlayerId,
                    name = p.Name,
                    color = p.Color.ToString(),
                    pieces = p.Pieces
                })
            };
        }
    }
}
