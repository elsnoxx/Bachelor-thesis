namespace server.Models.Games
{
    public class LudoGameInstance
    {
        public string RoomId { get; set; }
        public List<LudoPiece> Pieces { get; set; } = new();
        public string CurrentPlayerId { get; set; } // "r", "b", "g", "y"
        public int? LastDiceValue { get; set; }
        public bool WaitingForMove { get; set; }
        public List<string> PlayerOrder { get; set; } = new() { "r", "b", "g", "y" };

        public LudoGameInstance(string roomId)
        {
            RoomId = roomId;
            // Inicializace figurek na startovní pozice
            InitializePieces();
            CurrentPlayerId = "r";
        }

        private void InitializePieces() { /* Naplnění seznamu Pieces */ }

        // Logika hodu kostkou
        public int RollDice()
        {
            LastDiceValue = new Random().Next(1, 7);
            WaitingForMove = true;
            // Pokud hráč nemůže pohnout žádnou figurkou, měl by přepnout tah (to je ta logika serveru!)
            return LastDiceValue.Value;
        }

        // Logika pohybu
        public bool TryMove(string pieceId)
        {
            if (!WaitingForMove) return false;

            var piece = Pieces.FirstOrDefault(p => p.Id == pieceId);
            if (piece == null || piece.PlayerId != CurrentPlayerId) return false;

            // Tady zavoláš svou engine logiku (applyMove), kterou přepíšeš do C#
            // ... logika posunu ...

            WaitingForMove = false;
            SwitchTurn();
            return true;
        }

        private void SwitchTurn()
        {
            int currentIndex = PlayerOrder.IndexOf(CurrentPlayerId);
            CurrentPlayerId = PlayerOrder[(currentIndex + 1) % PlayerOrder.Count];
            LastDiceValue = null;
        }
    }

    public class LudoPiece
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public int PosIndex { get; set; } // Index na desce
    }
}
