import { Button } from "react-bootstrap";

type Props = {
  playerOnTurn: boolean;
  dice: number | null;
  onRoll: () => void;
};

export default function LudoTurn({ playerOnTurn, dice, onRoll }: Props) {
  return (
    <div className="text-center py-2">
      {playerOnTurn ? (
        <>
          <h4 className="fw-bold mb-1">Jsi na řadě!</h4>
          <p className="text-muted small mb-3">Klikni na kostku a zkus štěstí</p>

          <div className="d-flex gap-2">
            <Button
              variant="primary"
              className="w-100 py-3 shadow-sm fw-bold border-0"
              style={{
                background: "linear-gradient(45deg, #007bff, #0056b3)",
                fontSize: "1.2rem",
                borderRadius: "12px",
              }}
              onClick={onRoll}
            >
              🎲 HODIT KOSTKOU
            </Button>
            <div className="align-self-center">Dice: {dice ?? "-"}</div>
          </div>
        </>
      ) : (
        <div className="py-2">
          <div className="spinner-grow text-secondary mb-2" size="sm" role="status" />
          <h5 className="text-muted">Soupeř hází...</h5>
        </div>
      )}
    </div>
  );
}