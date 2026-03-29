import { useState, useEffect } from "react";
import { Container, Row, Col, Card, Spinner } from "react-bootstrap";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import LudoBoard from "./components/LudoBoard";
import LudoTurn from "./components/LudoTurn";
import LudoChat from "./components/LudoChat";
import type { Piece } from "./logic/LudoEngine";
import { useParams, useLocation } from "react-router-dom";
import GameHeader from "../general/GameHeader";


// DTO, které nám posílá server (Source of Truth)
interface LudoGameState {
  pieces: Piece[];
  currentPlayerId: string;
  diceValue: number | null;
  isWaitingForRoll: boolean;
}

export default function LudoGame() {
  const { roomId } = useParams<{ roomId: string }>();
  const location = useLocation();
  const passedRoomName = (location.state as any)?.roomName as string | undefined;
  const [roomName, setRoomName] = useState<string | null>(passedRoomName ?? null);
  const [connection, setConnection] = useState<HubConnection | null>(null);

  // Všechna herní data teď pocházejí z jednoho objektu stavu
  const [gameState, setGameState] = useState<LudoGameState | null>(null);
  const [myPlayerId, setMyPlayerId] = useState<string>("");

  useEffect(() => {

    if (!roomId) return;

    if (!passedRoomName && roomId) {
      const fallback = sessionStorage.getItem(`roomName_${roomId}`);
      if (fallback) setRoomName(fallback);
    }

    const conn = new HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_URL}/gamehub`, {
        accessTokenFactory: () => localStorage.getItem("token") ?? ""
      })
      .withAutomaticReconnect()
      .build();

    // SERVER NÁM POSÍLÁ AKTUALIZACI CELÉHO STAVU
    conn.on("UpdateLudoState", (newState: LudoGameState) => {
      setGameState(newState);
    });

    conn.start()
      .then(() => {
        conn.invoke("JoinRoom", roomId);
        // Získáme své ID z tokenu/systému (zjednodušeno)
        setMyPlayerId("r");
      })
      .catch(console.error);

    setConnection(conn);
    return () => { conn.stop(); };
  }, [roomId]);

  // LOGIKA: Místo počítání posunu jen pošleme serveru ID figurky
  const handlePieceClick = (pieceId: string) => {
    if (!connection || !gameState || gameState.currentPlayerId !== myPlayerId) return;

    // Server zkontroluje, jestli je tah validní, provede ho a všem pošle UpdateLudoState
    connection.invoke("LudoMovePiece", roomId, pieceId).catch(console.error);
  };

  // LOGIKA: Kostku hází server
  const handleRoll = () => {
    if (!connection || !roomId) return;
    connection.invoke("LudoRollDice", roomId).catch(console.error);
  };

  if (!gameState) return <div className="text-center p-5"><Spinner /> Načítání hry...</div>;

  const isMyTurn = gameState.currentPlayerId === myPlayerId;

  const getUserEmail = (): string | null => {
        const userJson = localStorage.getItem('user');
        if (userJson) {
            try { return JSON.parse(userJson).email || null; } catch { }
        }
        const token = localStorage.getItem('token');
        if (token) {
            try { const p = JSON.parse(atob(token.split('.')[1])); return p.email || p.sub || null; } catch { }
        }
        return null;
    };

  return (
    <Container fluid className="py-4">
      <GameHeader gameName="energybattle" userEmail={getUserEmail()} />
      <p className="text-sm text-muted">Místnost: {roomName ?? roomId}</p>
      <Row className="justify-content-center g-4">
        <Col xs={12} lg="auto" className="d-flex justify-content-center">
          <Card className="shadow-lg border-0">
            <Card.Body className="p-2 p-sm-4">
              <div style={{ width: "100%", maxWidth: "min(90vw,650px)", aspectRatio: "1/1" }}>
                {/* Board jen tupě zobrazuje data z gameState */}
                <LudoBoard
                  pieces={gameState.pieces}
                  onPieceClick={handlePieceClick}
                />
              </div>
            </Card.Body>
          </Card>
        </Col>

        <Col xs={12} lg={4} xl={3}>
          <div className="d-flex flex-column gap-3">
            <Card>
              <Card.Body>
                {/* LudoTurn dostává info, zda je hráč na řadě a co padlo */}
                <LudoTurn
                  playerOnTurn={isMyTurn}
                  dice={gameState.diceValue}
                  onRoll={handleRoll}
                />
              </Card.Body>
            </Card>
            <Card className="flex-grow-1" style={{ minHeight: 400 }}>
              <Card.Header>Chat (Místnost: {roomId})</Card.Header>
              <Card.Body className="p-0"><LudoChat /></Card.Body>
            </Card>
          </div>
        </Col>
      </Row>
    </Container>
  );
}