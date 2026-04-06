import React from "react";
import { Modal, Button } from "react-bootstrap";

interface GameOverModalProps {
  show: boolean;
  winnerEmail: string | null;
  myEmail: string | null;
  onClose: () => void;
}

export const GameOverModal = ({ show, winnerEmail, myEmail, onClose }: GameOverModalProps) => {
  // Převedeme na malé písmena pro jistotu při porovnávání
  const isIWinner = winnerEmail?.toLowerCase() === myEmail?.toLowerCase();

  return (
    <Modal show={show} centered backdrop="static" keyboard={false}>
      <Modal.Header className={isIWinner ? "bg-success text-white" : "bg-danger text-white"}>
        <Modal.Title>{isIWinner ? "Vítězství!" : "Porážka..."}</Modal.Title>
      </Modal.Header>
      <Modal.Body className="text-center py-4">
        <div className="display-1 mb-3">{isIWinner ? "🏆" : "💀"}</div>
        <h4 className={isIWinner ? "text-success" : "text-danger"}>
            {isIWinner ? "Skvělá práce!" : "Tvůj soupeř byl rychlejší..."}
        </h4>
        <p className="text-muted">
          {winnerEmail ? `Vítěz: ${winnerEmail}` : "Hra skončila."}
        </p>
      </Modal.Body>
      <Modal.Footer className="justify-content-center">
        {/* OPRAVA CESTY: Pokud se chceš vrátit na výběr her, zkontroluj si routu (pravděpodobně /games) */}
        <Button variant="secondary" size="lg" onClick={() => window.location.href = "/games/energybattle"}>
          Zpět do menu her
        </Button>
      </Modal.Footer>
    </Modal>
  );
};