type Props = {
    value: number;
    label: string;
    isCalibrating?: boolean;
    showFlash?: boolean;
    max?: number;
};

export default function PlayerPanel({ value, label, isCalibrating, showFlash, max = 300 }: Props) {
    const chartH = 200;
    const valueH = (Math.min(value, max) / max) * chartH;

    return (
        <div className={`p-3 rounded shadow-sm border ${showFlash ? 'bg-danger-subtle' : 'bg-light'}`} 
             style={{ transition: 'background-color 0.2s', position: 'relative' }}>
            
            {/* Indikátor blesku (SCR) */}
            {showFlash && (
                <div style={{ position: 'absolute', top: 10, right: 10, fontSize: '24px' }}>⚡</div>
            )}

            <h5 className="text-center">{label}</h5>
            
            <div className="d-flex justify-content-center align-items-end" style={{ height: chartH, background: '#eee', borderRadius: '20px', overflow: 'hidden' }}>
                <div style={{ 
                    width: '40px', 
                    height: `${valueH}px`, 
                    backgroundColor: isCalibrating ? '#6c757d' : '#3b82f6',
                    transition: 'height 0.3s ease-out'
                }} />
            </div>
            
            <div className="text-center mt-2">
                <small>{isCalibrating ? "Kalibruji..." : `Stres: ${Math.round(value)}`}</small>
            </div>
        </div>
    );
}