using System;
using System.Collections.Generic;
using System.Linq;

namespace server.Models.Games
{
    public class BalanceGame
    {
        public string RoomId { get; set; }
        public string LeftPlayerId { get; set; }
        public string RightPlayerId { get; set; }

        // Fronty pro ukládání historie hodnot
        private readonly Queue<double> _leftHistory = new();
        private readonly Queue<double> _rightHistory = new();
        private const int MaxHistory = 30;

        // Veřejné vlastnosti vrací průměr z historie (nebo 500, pokud historie zeje prázdnotou)
        public double LeftValue => _leftHistory.Any() ? _leftHistory.Average() : 500;
        public double RightValue => _rightHistory.Any() ? _rightHistory.Average() : 500;

        /// <summary>
        /// Přidá novou hodnotu pro konkrétního hráče a udrží historii na 30 prvcích.
        /// </summary>
        public void AddValue(string playerId, double value)
        {
            if (playerId == LeftPlayerId)
            {
                UpdateQueue(_leftHistory, value);
            }
            else if (playerId == RightPlayerId)
            {
                UpdateQueue(_rightHistory, value);
            }
        }

        private void UpdateQueue(Queue<double> queue, double value)
        {
            queue.Enqueue(value);
            if (queue.Count > MaxHistory)
            {
                queue.Dequeue();
            }
        }

        // Výpočet pozice kuličky (0-100) z průměrovaných hodnot
        public double GetBallPosition()
        {
            double combined = (LeftValue + RightValue) / 2;
            return Math.Clamp(combined / 10.0, 0, 100);
        }

        public bool IsGameOver => GetBallPosition() <= 0 || GetBallPosition() >= 100;
    }
}