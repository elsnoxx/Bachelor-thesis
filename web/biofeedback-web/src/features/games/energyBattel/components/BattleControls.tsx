import { Button } from "react-bootstrap";

interface BattleControlsProps {
  target: number;
  targetMin: number;
  targetMax: number;
  nextChangeIn: number;
  currentValue: number;
  isReady: boolean;
  onFire: () => void;
}

export const BattleControls = ({ target, targetMin, targetMax, nextChangeIn, currentValue, isReady, onFire }: BattleControlsProps) => {
  // Přepočet na procenta (předpokládáme, že GSR i Target jsou 0-100)
  const leftPos = Math.max(0, Math.min(100, targetMin));
  const widthPos = Math.max(1, Math.min(100, targetMax - targetMin));
  const currentPos = Math.max(0, Math.min(100, currentValue));

  return (
    <div className="text-center p-4 mt-3 bg-dark text-white rounded shadow">
      {/* Debug výpis - pokud uvidíš nuly, data ze serveru nechodí */}
      <div style={{ fontSize: '10px' }} className="text-muted mb-2">
        Range: {targetMin.toFixed(1)} - {targetMax.toFixed(1)} | Next: {nextChangeIn}s
      </div>

      {/* Horizontální bar */}
      <div className="position-relative mb-4" style={{ height: '20px', background: '#444', borderRadius: '10px' }}>
        {/* Cílová zóna */}
        <div
          className="position-absolute bg-success"
          style={{
            left: `${leftPos}%`,
            width: `${widthPos}%`,
            height: '100%',
            opacity: 0.6,
            transition: 'all 0.5s ease' // Plynulý posun zóny
          }}
        />
        {/* Ukazatel tvé hodnoty */}
        <div
          className="position-absolute bg-warning"
          style={{
            left: `${currentPos}%`,
            width: '4px',
            height: '140%',
            top: '-20%',
            transition: 'left 0.1s linear'
          }}
        />
      </div>

      <div className="row">
        <div className="col text-info">Target: {target.toFixed(2)}</div>
        <div className="col">Next change: {nextChangeIn}s</div>
        <div className="col text-warning">Current: {currentValue.toFixed(2)}</div>
      </div>

      <Button
        variant={isReady ? "danger" : "secondary"}
        disabled={!isReady}
        onClick={onFire}
        className="mt-3 w-100 py-3 fw-bold"
      >
        {isReady ? "PAL!" : "NABÍJENÍ..."}
      </Button>
    </div>
  );
};