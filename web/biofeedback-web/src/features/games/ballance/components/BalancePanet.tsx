import React from "react";

type Props = {
  width?: number;
  height?: number;
  onFall?: (side: "left" | "right") => void;
  leftValue?: number;
  rightValue?: number;
  min?: number;
  max?: number;
  targetMin?: number;
  targetMax?: number;
};

export default function BalanceArena({
  width = 520,
  height = 260,
  onFall,
  leftValue = 0,
  rightValue = 0,
  min = 0,
  max = 1000,
  targetMin = 500,
  targetMax = 800,
}: Props) {
  // průměr a normalizace do [0..1]
  const combined = (leftValue + rightValue) / 2;
  const range = Math.max(1, max - min);
  const norm = Math.min(1, Math.max(0, (combined - min) / range));

  // % pozice a cílová oblast v rámci SVG viewBox 0..100
  const cxPct = norm * 100;
  const tStartPct = Math.min(100, Math.max(0, ((targetMin - min) / range) * 100));
  const tEndPct = Math.min(100, Math.max(0, ((targetMax - min) / range) * 100));
  const tWidthPct = Math.max(0, tEndPct - tStartPct);

  const arenaH = height;

  return (
    <div className="w-full h-full flex flex-col items-center justify-center">
      <div className="w-full max-w-3xl">
        <div className="bg-white rounded-lg shadow p-4 overflow-hidden">
          <div className="relative" style={{ height: arenaH }}>
            <div className="absolute inset-0 bg-gradient-to-b from-gray-50 to-gray-100 rounded" />

            {/* responzivní SVG: viewBox 0..100, width 100% */}
            <svg width="100%" height={arenaH} viewBox="0 0 100 100" className="block">
              <rect x="0" y="0" width="100" height="100" fill="transparent" />

              {/* target band (v % z šířky) */}
              <rect
                x={tStartPct}
                y={12}
                width={tWidthPct}
                height={76}
                fill="rgba(16,185,129,0.06)"
                rx={2}
              />

              {/* centerline (vizuální) */}
              <line x1={50} x2={50} y1={8} y2={92} stroke="#e5e7eb" strokeWidth={0.6} />

              {/* kulička: cx v procentech, cy uprostřed (50), r v jednotkách viewBox */}
              <circle cx={cxPct} cy={50} r={4.5} fill="#3b82f6" stroke="#111827" strokeOpacity={0.08} />
            </svg>
          </div>
        </div>
      </div>
    </div>
  );
}