import React from "react";
import { Modal, Button } from "react-bootstrap";

interface ResultShape {
  isWin?: boolean;
  reason?: string | null;
  // volitelně může obsahovat další pole (např. winnerId), ale preferujeme
  // explicitní props níže pro rozlišení
  [key: string]: any;
}

interface GameOverModalProps {
  show: boolean;
  // Primární způsob: poskytni `result` s `isWin` a (nepovinně) `reason`
  result?: ResultShape | null;
  // Alternativní (pokud server vrací jen winner id/email):
  winnerId?: string | null;
  currentPlayerId?: string | null;
  // Handlery
  onRetry?: () => void; // potvrzení restartu / návratu do lobby
  onClose?: () => void; // zavření modalu bez redirectu (volitelné)
}

export default function GameOverModal({
  show,
  result = null,
  winnerId,
  currentPlayerId,
  onRetry,
  onClose,
}: GameOverModalProps) {
  // Rozhodneme, jestli hráč vyhrál:
  let isWin: boolean | null = null;
  let reason: string | null = null;

  if (result && typeof result.isWin === "boolean") {
    isWin = result.isWin;
    reason = result.reason ?? null;
  } else if (winnerId !== undefined && currentPlayerId !== undefined) {
    isWin = Boolean(winnerId) && winnerId.toLowerCase() === (currentPlayerId ?? "").toLowerCase();
    reason = result?.reason ?? null;
  }

  // Fallback: pokud nelze rozhodnout, zobraz neutrální zprávu
  const title = isWin === true ? "Vítězství!" : isWin === false ? "Prohra..." : "Konec hry";
  const icon = isWin === true ? "🏆" : isWin === false ? "😞" : "⚑";
  const subtitle =
    isWin === true
      ? "Skvělá práce!"
      : isWin === false
      ? "Hra skončila."
      : "Hra skončila.";

  const handlePrimary = () => {
    if (onRetry) onRetry();
    else {
      // zabezpečený fallback: přesměruj na výběr her
      window.location.href = "/games";
    }
  };

  const handleClose = () => {
    if (onClose) onClose();
  };

  return (
    <Modal show={show} centered backdrop="static" keyboard={false}>
      <Modal.Header className={isWin === true ? "bg-success text-white" : isWin === false ? "bg-danger text-white" : "bg-secondary text-white"}>
        <Modal.Title>{title}</Modal.Title>
      </Modal.Header>

      <Modal.Body className="text-center py-4">
        <div className="display-1 mb-3">{icon}</div>
        <h4 className={isWin === true ? "text-success" : isWin === false ? "text-danger" : "text-muted"}>{subtitle}</h4>
        {reason ? <p className="text-muted mt-2">{reason}</p> : null}
        {winnerId && currentPlayerId && (
          <p className="text-muted small mt-2">Vítěz: {winnerId}</p>
        )}
      </Modal.Body>

      <Modal.Footer className="justify-content-center">
        <Button variant={isWin === true ? "success" : "primary"} size="lg" onClick={handlePrimary} className="rounded-pill px-4 me-2">
          Zpět do lobby
        </Button>
        {onClose && (
          <Button variant="outline-secondary" size="lg" onClick={handleClose} className="rounded-pill px-4">
            Zavřít
          </Button>
        )}
      </Modal.Footer>
    </Modal>
  );
}