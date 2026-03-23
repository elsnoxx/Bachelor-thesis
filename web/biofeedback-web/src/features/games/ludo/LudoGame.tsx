import { useState } from "react";
import LudoBoard from "./components/LudoBoard";
import { Container, Row, Col, Card, Button } from "react-bootstrap";
import LudoTurn from "./components/LudoTurn";
import LudoChat from "./components/LudoChat";
import type { Piece } from "./logic/LudoEngine";
import { rollDice, applyMove, canMovePiece, getStartForPlayer } from "./logic/LudoEngine";

const initialPieces: Piece[] = [
  { id: "r1", playerId: "r", posIndex: 0 },
  { id: "r2", playerId: "r", posIndex: 1 },
  { id: "b1", playerId: "b", posIndex: 0 },
  { id: "g1", playerId: "g", posIndex: 0 },
];

export default function LudoGame() {
  const [pieces, setPieces] = useState<Piece[]>(initialPieces);
  const [selected, setSelected] = useState<string | null>(null);
  const [dice, setDice] = useState<number | null>(null);
  const currentPlayer = { id: "r", color: "RED", startIndex: getStartForPlayer("r") };

  const handleRoll = () => setDice(rollDice());

  const handleSelectOrMove = (pieceIdOrRow: number | string, col?: number) => {
    // pokud klik na políčko (row,col) tak koliduje s koncepcí posIndex; tento handler budeme volat z LudoBoard
  };

  const handlePieceClick = (pieceId: string) => {
    if (!dice) return setSelected(pieceId);
    const piece = pieces.find(p => p.id === pieceId);
    if (!piece) return;
    if (!canMovePiece(piece, dice, currentPlayer as any, pieces)) return;
    const res = applyMove(piece, dice, currentPlayer as any, pieces);
    if (!res.success) return;
    setPieces(prev => {
      const others = prev.filter(p => p.id !== pieceId && !res.captures.find(c => c.id === p.id));
      const updated = [res.moved, ...others, ...res.captures];
      return updated;
    });
    setDice(null);
    setSelected(null);
  };

  return (
    <Container fluid className="py-4">
      <Row className="justify-content-center g-4">
        <Col xs={12} lg="auto" className="d-flex justify-content-center">
          <Card className="shadow-lg border-0 overflow-hidden">
            <Card.Body className="p-2 p-sm-4">
              <div style={{ width: "100%", maxWidth: "min(90vw,650px)", aspectRatio: "1/1" }}>
                <LudoBoard pieces={pieces} onPieceClick={handlePieceClick} />
              </div>
            </Card.Body>
          </Card>
        </Col>
        <Col xs={12} lg={4} xl={3}>
          <div className="d-flex flex-column gap-3">
            <Card>
              <Card.Body>
                <LudoTurn playerOnTurn={true} dice={dice} onRoll={handleRoll} />
              </Card.Body>
            </Card>
            <Card className="flex-grow-1" style={{ minHeight: 400 }}>
              <Card.Header>Chat</Card.Header>
              <Card.Body className="p-0"><LudoChat/></Card.Body>
            </Card>
          </div>
        </Col>
      </Row>
    </Container>
  );
}