import React from "react";

type Props = {
  leftValue?: number;
  rightValue?: number;
  ballPos?: number;        // NOVÉ: pozice z websocketu (0-100)
  min?: number;
  max?: number;
  targetMin?: number;
  targetMax?: number;
  height?: number;
};

export default function BalanceArena({
  leftValue = 0,
  rightValue = 0,
  ballPos,
  min = 0,
  max = 1000,
  targetMin = 400,
  targetMax = 600,
  height = 200,
}: Props) {
  const range = Math.max(1, max - min);

  // 1. Pozice kuličky: Priorita má ballPos ze serveru (0-100)
  const cxPct = (typeof ballPos === "number" && !Number.isNaN(ballPos)) 
                ? Math.min(100, Math.max(0, ballPos)) 
                : ((leftValue + rightValue) / 2 / range) * 100;

  // 2. Výpočet zelené plochy: Převod GSR limitů na procenta šířky
  const tStartPct = ((targetMin - min) / range) * 100;
  const tEndPct = ((targetMax - min) / range) * 100;
  const tWidthPct = Math.max(0, tEndPct - tStartPct);

  // 3. Stav "V balancu": ballPos je 0-100, takže ho musíme porovnat s procenty limitů
  const ballPosNorm = cxPct / 100;
  const targetMinNorm = (targetMin - min) / range;
  const targetMaxNorm = (targetMax - min) / range;
  const inTarget = ballPosNorm >= targetMinNorm && ballPosNorm <= targetMaxNorm;

  // 4. Tilt (náklon): Vizuální efekt podle rozdílu hodnot
  const tilt = ((rightValue - leftValue) / range) * 15; // Zvýšeno na 15 pro lepší feedback

  return (
    <div className="w-full flex flex-col items-center gap-4">
      <div className="w-full max-w-3xl flex justify-between px-4 font-medium text-gray-400 text-sm">
        <span className={((leftValue + rightValue) / 2 / range) < 0.3 ? "text-red-500" : ""}>L</span>
        <span className={inTarget ? "text-emerald-500" : ""}>CENTER</span>
        <span className={((leftValue + rightValue) / 2 / range) > 0.7 ? "text-red-500" : ""}>P</span>
      </div>

      <div className="w-full max-w-3xl perspective-1000">
        <div
          className="relative bg-white rounded-2xl shadow-xl overflow-hidden border-4 border-gray-100 transition-transform duration-200 ease-out"
          style={{
            height: height,
            transform: `rotateZ(${tilt}deg)`,
          }}
        >
          <div className="absolute inset-0 flex">
            <div className="w-1/4 h-full bg-gradient-to-r from-red-50 to-transparent opacity-50" />
            <div className="w-2/4 h-full" />
            <div className="w-1/4 h-full bg-gradient-to-l from-red-50 to-transparent opacity-50" />
          </div>

          <svg width="100%" height="100%" viewBox="0 0 100 100" preserveAspectRatio="none">
            <rect
              x={tStartPct}
              y={0}
              width={tWidthPct}
              height={100}
              fill={inTarget ? "#10b981" : "#e5e7eb"}
              fillOpacity={inTarget ? "0.15" : "0.1"}
              className="transition-colors duration-300"
            />

            <line x1="50" y1="20" x2="50" y2="80" stroke="#d1d5db" strokeWidth="0.5" strokeDasharray="2 2" />
            <line x1="10" y1="50" x2="90" y2="50" stroke="#f3f4f6" strokeWidth="2" strokeLinecap="round" />

            <g style={{ transition: 'all 0.1s ease-out' }}>
              <circle cx={cxPct} cy={55} r={4} fill="black" fillOpacity="0.05" />
              <circle
                cx={cxPct}
                cy={50}
                r={5}
                fill={inTarget ? "#10b981" : "#3b82f6"}
                stroke="white"
                strokeWidth="1.5"
                className="shadow-lg"
              />
              <circle cx={cxPct - 1.5} cy={48.5} r={1} fill="white" fillOpacity="0.4" />
            </g>
          </svg>

          {cxPct < 10 && (
            <div className="absolute left-4 top-1/2 -translate-y-1/2 animate-pulse text-red-500 font-bold">
              ◀ FALLING
            </div>
          )}
          {cxPct > 90 && (
            <div className="absolute right-4 top-1/2 -translate-y-1/2 animate-pulse text-red-500 font-bold">
              FALLING ▶
            </div>
          )}
        </div>
      </div>
    </div>
  );
}