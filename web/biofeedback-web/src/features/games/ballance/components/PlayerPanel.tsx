import React from "react";

type Props = {
    value: number;
    min?: number;
    max?: number;
    targetMin?: number;
    targetMax?: number;
    label?: string;
    recentMin?: number | null;
    recentMax?: number | null;
    edgePct?: number;
};

export default function PlayerPanel({
    value,
    min = 0,
    max = 1000,
    targetMin = 500,
    targetMax = 800,
    label = "Hráč",
    recentMin = null,
    recentMax = null,
    edgePct = 0.1,
}: Props) {
    const clamp = (v: number, a: number, b: number) => Math.max(a, Math.min(b, v));
    const range = max - min || 1;

    const pct = ((clamp(value, min, max) - min) / range) * 100;

    const targetStartPct = ((clamp(targetMin, min, max) - min) / range) * 100;
    const targetEndPct = ((clamp(targetMax, min, max) - min) / range) * 100;
    const targetWidthPct = Math.max(0, targetEndPct - targetStartPct);

    const rMin =
        recentMin != null && recentMax != null ? Math.min(recentMin, recentMax) : null;
    const rMax =
        recentMin != null && recentMax != null ? Math.max(recentMin, recentMax) : null;

    const inTarget = value >= targetMin && value <= targetMax;
    const edgeAbs = (targetMax - targetMin) * edgePct;
    const nearEdge =
        inTarget && (value - targetMin < edgeAbs || targetMax - value < edgeAbs);

    const statusColor = !inTarget ? "text-red-600" : nearEdge ? "text-yellow-600" : "text-green-600";
    const valueColor = !inTarget ? "#ef4444" : nearEdge ? "#f59e0b" : "#10b981";

    // větší graf - parametry
    const chartW = 160;
    const chartH = 120;
    const padX = 16;
    const colW = 36;

    const valueH = ((clamp(value, min, max) - min) / range) * chartH;

    const safeHeight = (h: number) => Math.max(4, Math.round(h));

    return (
        <div className="bg-white rounded-lg shadow p-4 h-100 d-flex flex-column">
            <div className="flex items-center justify-between mb-3">
                <div>
                    <h3 className="font-semibold">{label}</h3>
                    <p className="text-sm text-gray-500">Statistiky / ovládání</p>
                    <div className={`${statusColor} font-semibold text-lg`}>Nyní hodnota: {value}</div>
                </div>
                
            </div>
            <div className="flex items-center justify-center flex-grow">

                {/* velký vertikální sloupec (větší vizuální ukazatel) */}
                <div className="flex items-center gap-4">
                    <svg
                        width="160"
                        height="160"
                        viewBox={`0 0 ${chartW} ${chartH + 40}`}
                        aria-hidden
                        className="block"
                    >
                        {/* pozadí - oblast targetu vyznačená horizontálně, pro kontext */}
                        <rect x="0" y="0" width={chartW} height={chartH} fill="transparent" />

                        {/* current value bar (vertikální, větší) */}
                        <rect
                            x={(chartW - colW) / 2}
                            y={chartH - safeHeight(valueH)}
                            width={colW}
                            height={safeHeight(valueH)}
                            fill={valueColor}
                            rx={6}
                        />

                        {/* marker target jako tenká linka u odpovídajících hodnot (volitelné) */}
                        {/* targetMin a targetMax pozice pro vizuální kontext */}
                        <line
                            x1={(chartW - colW) / 2 - 12}
                            x2={(chartW + colW) / 2 + 12}
                            y1={chartH - ((targetMin - min) / range) * chartH}
                            y2={chartH - ((targetMin - min) / range) * chartH}
                            stroke="#16a34a"
                            strokeWidth={1}
                            opacity={0.35}
                        />
                        <line
                            x1={(chartW - colW) / 2 - 12}
                            x2={(chartW + colW) / 2 + 12}
                            y1={chartH - ((targetMax - min) / range) * chartH}
                            y2={chartH - ((targetMax - min) / range) * chartH}
                            stroke="#16a34a"
                            strokeWidth={1}
                            opacity={0.35}
                        />

                        {/* popisky pod sloupcem */}
                        <text
                            x={chartW / 2}
                            y={chartH + 20}
                            fontSize={12}
                            fill="#374151"
                            textAnchor="middle"
                        >
                            {Math.round(value)}
                        </text>
                    </svg>
                </div>
            </div>
        </div>
    );
}