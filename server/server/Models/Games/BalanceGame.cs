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
        public bool WasSaved { get; set; } = false;
        public double TargetMinPlayer { get; set; }
        public double TargetMaxPlayer { get; set; }
        public bool LimitsGenerated { get; set; } = false;
        public double TargetMin { get; set; }
        public double TargetMax { get; set; }
        public double TargetWidth { get; set; } = 20.0;

        public DateTime? StartTime { get; set; }
        public int DurationSeconds { get; set; } = 120;

        // Fronty pro ukládání historie hodnot
        private readonly Queue<double> _leftHistory = new();
        private readonly Queue<double> _rightHistory = new();
        private const int MaxHistory = 30;

        // Veřejné vlastnosti vrací průměr z historie (nebo 500, pokud historie zeje prázdnotou)
        public double LeftValue => _leftHistory.Any() ? _leftHistory.Average() : 500;
        public double RightValue => _rightHistory.Any() ? _rightHistory.Average() : 500;

        public bool IsGameOver => GetBallPosition() <= 0 || GetBallPosition() >= 100 || GetRemainingTime() <= 0;

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
            // 1. Získá průměry posledních 30 hodnot obou hráčů (GSR senzory)
            double combined = (LeftValue + RightValue) / 2;

            // 2. Transformuje hodnotu na stupnici 0-100 (vydělením 10.0)
            // 3. Math.Clamp zajistí, že hodnota nikdy "nevyletí" mimo rozsah 0 a 100
            return Math.Clamp(combined / 10.0, 0, 100);
        }

        public int GetRemainingTime()
        {
            if (!StartTime.HasValue) return DurationSeconds;
            var elapsed = (DateTime.UtcNow - StartTime.Value).TotalSeconds;
            var remaining = DurationSeconds - (int)elapsed;
            return Math.Max(0, remaining);
        }

        public bool IsInBalance => GetBallPosition() >= TargetMin && GetBallPosition() <= TargetMax;

        public void GenerateLimits()
        {
            Random rnd = new Random();

            // 1. Definujeme společný cíl pro kuličku (v procentech 0-100)
            double halfWidth = TargetWidth / 2;
            double centerPos = rnd.Next(25, 75); // Střed arény
            TargetMin = centerPos - halfWidth;
            TargetMax = centerPos + halfWidth;

            // 2. Definujeme cíl pro individuální panely (GSR hodnoty)
            // Aby to logicky sedělo, pokud budou oba hráči na svém středu, 
            // kulička bude na svém středu.
            double centerGSR = centerPos * 10; // Přepočet z 0-100 na 0-1000
            TargetMinPlayer = centerGSR - 100; // Tolerance +- 100 jednotek GSR
            TargetMaxPlayer = centerGSR + 100;

            LimitsGenerated = true;
        }
    }
}