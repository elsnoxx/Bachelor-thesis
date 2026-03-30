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
    edgePct = 0.1,
}: Props) {
    const clamp = (v: number, a: number, b: number) => Math.max(a, Math.min(b, v));
    const range = max - min || 1;

    // Zaokrouhlená hodnota pro zobrazení
    const roundedValue = Math.round(value);

    // Výpočty pro SVG
    const chartW = 100; // Šířka plátna
    const chartH = 200; // Výška plátna
    const colW = 40;    // Šířka sloupce

    const valueH = ((clamp(value, min, max) - min) / range) * chartH;
    const targetStartY = chartH - ((targetMax - min) / range) * chartH;
    const targetEndY = chartH - ((targetMin - min) / range) * chartH;
    const targetHeight = targetEndY - targetStartY;

    const inTarget = value >= targetMin && value <= targetMax;
    const edgeAbs = (targetMax - targetMin) * edgePct;
    const nearEdge = inTarget && (value - targetMin < edgeAbs || targetMax - value < edgeAbs);

    const statusColorClass = !inTarget ? "text-red-500" : nearEdge ? "text-amber-500" : "text-emerald-500";
    const barColor = !inTarget ? "#ef4444" : nearEdge ? "#f59e0b" : "#10b981";

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 h-full d-flex flex-column transition-all hover:shadow-md">
            {/* Header */}
            <div className="mb-6 text-center"> {/* Přidáno text-center pro vycentrování textu */}
                <h3 className="text-xl font-bold text-gray-800 m-0">{label}</h3>
                <p className="text-xs font-medium uppercase tracking-wider text-gray-400 mb-2">
                    Live Biofeedback
                </p>

                {/* Změněno na justify-center, aby byla hodnota s jednotkou uprostřed */}
                <div className="flex items-baseline justify-center gap-2">
                    <span className={`text-4xl font-black ${statusColorClass}`}>
                        {roundedValue}
                    </span>
                    <span className="text-gray-400 text-sm font-semibold uppercase tracking-widest">
                        GSR
                    </span>
                </div>
            </div>

            {/* Vizualizace */}
            <div className="flex-grow flex items-center justify-center bg-gray-50 rounded-xl p-4">
                <svg
                    width="100%"
                    height="240"
                    viewBox={`0 0 ${chartW} ${chartH}`}
                    className="overflow-visible"
                >
                    {/* Background track */}
                    <rect
                        x={(chartW - colW) / 2}
                        y="0"
                        width={colW}
                        height={chartH}
                        fill="#e5e7eb"
                        rx={colW / 2}
                    />

                    {/* Target Range Highlight */}
                    <rect
                        x={(chartW - colW) / 2}
                        y={targetStartY}
                        width={colW}
                        height={targetHeight}
                        fill="#10b981"
                        fillOpacity="0.15"
                    />

                    {/* Target Range Borders */}
                    <line
                        x1={(chartW - colW) / 2 - 10} x2={(chartW + colW) / 2 + 10}
                        y1={targetStartY} y2={targetStartY}
                        stroke="#10b981" strokeWidth="1" strokeDasharray="2 2"
                    />
                    <line
                        x1={(chartW - colW) / 2 - 10} x2={(chartW + colW) / 2 + 10}
                        y1={targetEndY} y2={targetEndY}
                        stroke="#10b981" strokeWidth="1" strokeDasharray="2 2"
                    />

                    {/* Actual Value Bar */}
                    <rect
                        x={(chartW - colW) / 2}
                        y={chartH - valueH}
                        width={colW}
                        height={valueH}
                        fill={barColor}
                        rx={colW / 2}
                        className="transition-all duration-300 ease-out"
                    />
                </svg>
            </div>

        </div>
    );
}