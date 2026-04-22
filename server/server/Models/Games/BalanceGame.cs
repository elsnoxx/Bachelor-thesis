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

        // --- Kalibrace ---
        private const int CalibrationDurationSeconds = 10;
        private bool IsCalibrating => StartTime.HasValue && (DateTime.UtcNow - StartTime.Value).TotalSeconds < CalibrationDurationSeconds;
        private double _leftBaseline = 0;
        private double _rightBaseline = 0;

        public bool WasSaved { get; set; } = false;
        public double TargetMax { get; set; }
        // Startovní šířka zóny (50 % je fér pro začátek)
        public double TargetWidth { get; set; } = 50.0;

        public DateTime? StartTime { get; set; }
        public int DurationSeconds { get; set; } = 120;

        private double _scrPenalty = 0;
        private double _lastValueLeft = 0;
        private double _lastValueRight = 0;

        // Zvětšená historie pro lepší filtraci šumu (podle tvé DP o filtraci signálu)
        private readonly Queue<double> _leftHistory = new();
        private readonly Queue<double> _rightHistory = new();
        private const int MaxHistory = 50;

        // Zprůměrované hodnoty (SCL)
        public double LeftValue => _leftHistory.Any() ? _leftHistory.Average() : 0;
        public double RightValue => _rightHistory.Any() ? _rightHistory.Average() : 0;

        public int GetCalibrationRemaining()
        {
            if (!StartTime.HasValue) return CalibrationDurationSeconds;
            var elapsed = (DateTime.UtcNow - StartTime.Value).TotalSeconds;
            return Math.Max(0, CalibrationDurationSeconds - (int)elapsed);
        }

        public bool IsGameOver
        {
            get
            {
                if (!StartTime.HasValue) return false;
                if (IsCalibrating) return false;

                // Přidáváme časovou pojistku (20s), aby hra neskončila hned v první sekundě po kalibraci
                var secondsSinceStart = (DateTime.UtcNow - StartTime.Value).TotalSeconds;
                if (secondsSinceStart > (CalibrationDurationSeconds + 20))
                {
                    if (GetBallPosition() >= TargetMax) return true;
                }

                // Vítězství časem
                if (GetRemainingTime() <= 0) return true;

                return false;
            }
        }

        public void AddValue(string playerId, double value)
        {
            if (playerId == LeftPlayerId)
            {
                // Detekce SCR (stresového píku)
                if (!IsCalibrating && _lastValueLeft > 0 && (value - _lastValueLeft) > 100)
                {
                    _scrPenalty += 15;
                }
                _lastValueLeft = value;
                UpdateQueue(_leftHistory, value);
            }
            else if (playerId == RightPlayerId)
            {
                if (!IsCalibrating && _lastValueRight > 0 && (value - _lastValueRight) > 100)
                {
                    _scrPenalty += 15;
                }
                _lastValueRight = value;
                UpdateQueue(_rightHistory, value);
            }

            // Fixace baseline ihned po kalibraci
            if (StartTime.HasValue && !IsCalibrating && _leftBaseline == 0)
            {
                if (_leftHistory.Count > 10 && _rightHistory.Count > 10)
                {
                    _leftBaseline = _leftHistory.Min();
                    _rightBaseline = _rightHistory.Min();
                    GenerateLimits();
                }
            }

            if (!IsCalibrating)
            {
                UpdateTargetZone();
                // Pomalejší vyprchání stresu
                _scrPenalty *= 0.97;
            }
        }

        private void UpdateQueue(Queue<double> queue, double value)
        {
            queue.Enqueue(value);
            if (queue.Count > MaxHistory) queue.Dequeue();
        }

        public double GetBallPosition()
        {
            if (IsCalibrating || _leftBaseline == 0) return 0;

            // SCL: Odchylka od klidového stavu
            double leftDiff = Math.Max(0, LeftValue - _leftBaseline);
            double rightDiff = Math.Max(0, RightValue - _rightBaseline);

            double sensitivity = 40.0;
            double sclBase = Math.Max(leftDiff, rightDiff) / sensitivity;

            return Math.Clamp(sclBase + _scrPenalty, 0, 100);
        }

        public int GetRemainingTime()
        {
            if (!StartTime.HasValue) return DurationSeconds;
            var elapsed = (DateTime.UtcNow - StartTime.Value).TotalSeconds;
            if (elapsed < CalibrationDurationSeconds) return DurationSeconds;
            return Math.Max(0, DurationSeconds - (int)(elapsed - CalibrationDurationSeconds));
        }

        public void GenerateLimits()
        {
            TargetMax = TargetWidth;
        }

        public void UpdateTargetZone()
        {
            // Pokud má být zóna fixní dle textu:
            // TargetMax = 50.0; // Nebo jiná konstanta, kterou zvolíte jako obtížnost

            if (LeftValue < _leftBaseline + 150 && RightValue < _rightBaseline + 150)
            {
                if (TargetWidth < 80) TargetWidth += 0.05;
            }
            else
            {
                if (TargetWidth > 40) TargetWidth -= 0.1;
            }
            TargetMax = TargetWidth;
        }

        public object GetState()
        {
            double ballPos = GetBallPosition();
            int remaining = GetRemainingTime();
            bool isWin = IsGameOver && remaining <= 0 && ballPos < TargetMax;

            string status = IsCalibrating ? "Kalibrace..." : "Hra běží";
            if (IsGameOver)
            {
                status = ballPos >= TargetMax ? "Příliš vysoký stres!" : "Vítězství!";
            }

            return new
            {
                roomId = RoomId,
                ballPosition = ballPos,
                isCalibrating = IsCalibrating,
                calibrationTimeLeft = GetCalibrationRemaining(),
                leftValue = LeftValue,
                rightValue = RightValue,
                targetMax = TargetMax,
                isGameOver = IsGameOver,
                remainingTime = remaining,
                isWin = isWin,
                endReason = status,
                leftScl = Math.Max(0, LeftValue - _leftBaseline),
                rightScl = Math.Max(0, RightValue - _rightBaseline)
            };
        }
    }
}