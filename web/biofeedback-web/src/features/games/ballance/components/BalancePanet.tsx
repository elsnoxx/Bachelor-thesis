import React from "react";

type Props = {
  ballPos?: number | null;
  targetMin?: number;
  targetMax?: number;
  isCalibrating?: boolean;
  height?: number;
};

export default function BalanceArena({
  ballPos = 0,
  targetMax = 30,
  isCalibrating = false,
  height = 200,
}: Props) {
  const clamp = (v: number, a = 0, b = 100) => Math.min(b, Math.max(a, v));
  
  // Vizuální pozice: Chceme, aby 0 na serveru byla vlevo na liště a 100 vpravo.
  const cxPct = clamp(typeof ballPos === "number" ? ballPos : 0, 0, 100);
  
  // Hranice v UI (zelená zóna) začíná na 0 a končí na targetMax
  const tStart = 0;
  const tEnd = clamp(targetMax, 0, 100);
  const tWidth = tEnd - tStart;
  
  const inTarget = cxPct <= tEnd;

  return (
    <div className="p-4 bg-white rounded shadow-sm border h-100 d-flex flex-column align-items-center">
      <h6 className="mb-3">Společná úroveň stresu</h6>
      <svg viewBox="0 0 100 100" className="w-100" style={{ maxHeight: `${height}px` }}>
        {/* Pozadí pruhu (Šedá linka jako dráha) */}
        <rect x="0" y="48" width="100" height="4" fill="#f3f4f6" rx="2" />

        {/* Zelená zóna (Bezpečná oblast od 0 do TargetMax) */}
        <rect
          x={tStart}
          y="42"
          width={tWidth}
          height="16"
          fill="#10b981"
          fillOpacity={inTarget ? 0.2 : 0.1}
          style={{ transition: "width 300ms ease" }}
        />

        {/* Cílová čára (Propast) */}
        <line x1={tEnd} y1="35" x2={tEnd} y2="65" stroke="#ef4444" strokeWidth="1" strokeDasharray="2 2" />

        {/* Kulička (Pohybuje se zleva doprava) */}
        <circle
          cx={cxPct}
          cy="50"
          r={isCalibrating ? 0 : 5}
          fill={inTarget ? "#10b981" : "#ef4444"}
          stroke="#fff"
          strokeWidth={1}
          style={{ transition: "all 100ms linear" }}
        />
      </svg>

      <div className="w-100 mt-2 d-flex justify-content-between">
        <small className="text-muted">Klid (0)</small>
        <small className="text-danger">Limit ({Math.round(targetMax)})</small>
      </div>
    </div>
  );
}