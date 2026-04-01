import React from "react";
import { Modal, Button } from "react-bootstrap";

interface GameOverModalProps {
  show: boolean;
  winnerEmail: string | null;
  myEmail: string | null;
  onClose: () => void;
}

export const GameOverModal = ({ show, winnerEmail, myEmail, onClose }: GameOverModalProps) => {
  const isIWinner = winnerEmail === myEmail;

  return (
    <Modal show={show} centered backdrop="static" keyboard={false}>
      <Modal.Header className={isIWinner ? "bg-success text-white" : "bg-danger text-white"}>
        <Modal.Title>{isIWinner ? "Vítězství!" : "Porážka..."}</Modal.Title>
      </Modal.Header>
      <Modal.Body className="text-center py-4">
        <div className="display-1 mb-3">{isIWinner ? "🏆" : "💀"}</div>
        <h4>{isIWinner ? "Skvělá práce!" : "Zkus to znovu!"}</h4>
        <p className="text-muted">
          {winnerEmail ? `Vítěz: ${winnerEmail}` : "Hra skončila remízou."}
        </p>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="primary" onClick={() => window.location.href = "/energybattle"}>
          Zpět do menu
        </Button>
      </Modal.Footer>
    </Modal>
  );
};