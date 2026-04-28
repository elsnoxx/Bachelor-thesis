import { useEffect, useState, useCallback } from "react";
import { useParams } from "react-router-dom";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { Container, Row, Col, Spinner } from "react-bootstrap";
import BalloonPlayground from "./components/BalloonPlayground";
import GameHeader from "../general/GameHeader";
import GameOverModal from "../general/GameOverModal";
import type { BalloonGameState } from "./balloonsTypes";
import { useBle } from "../../../services/BleProvider";
import { PlayerInfoCard } from "./components/PlayerInfoCard";
import api from "../../../api/axiosInstance";

export default function BalloonGame() {
    const { roomId } = useParams<{ roomId: string }>();
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [gameState, setGameState] = useState<BalloonGameState | null>(null);

    const { gsrValue, isConnected } = useBle();
    const [simulatedGsr, setSimulatedGsr] = useState<number>(0);
    const userEmail = JSON.parse(localStorage.getItem('user') || '{}').email;

    // --- SignalR Connection ---
    useEffect(() => {
        if (!roomId) return;

        const conn = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL}/gamehub`, {
                accessTokenFactory: async () => {
                    let token = localStorage.getItem("token");
                    if (token) {
                        try {
                            const payload = JSON.parse(atob(token.split(".")[1]));
                            if (payload.exp * 1000 < Date.now()) {
                                const res = await api.post('/refresh');
                                token = res.data.token || res.data.accessToken;
                                if (token) localStorage.setItem("token", token);
                            }
                        } catch (e) { console.error("Token refresh error", e); }
                    }
                    return token || "";
                }
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        conn.on("ReceiveGameState", (state: BalloonGameState) => {
            setGameState(state);
        });

        conn.start()
            .then(() => conn.invoke("JoinRoom", roomId))
            .catch(err => console.error("SignalR Error: ", err));

        setConnection(conn);
        return () => { conn.stop(); };
    }, [roomId]);

    // --- Biofeedback Loop ---
    useEffect(() => {
        if (!connection || (gameState && gameState.isFinished)) return;

        const interval = setInterval(() => {
            if (connection.state === "Connected") {
                let valueToSend: number;

                if (isConnected) {
                    // ODESÍLÁNÍ REÁLNÝCH DAT (Běží pouze, pokud je BLE připojeno)
                    valueToSend = gsrValue || 0;

                    connection.invoke("SendGameData", roomId, "balloon", valueToSend)
                        .catch(err => console.error("Send error:", err));
                } else {
                    /* --- TESTOVACÍ SIMULÁTOR: ZAKOMENTOVÁNO ---
                    // Tato část simulovala pohyb balónku bez senzoru.
                    
                    const fake = 400 + Math.random() * 200;
                    setSimulatedGsr(fake);
                    valueToSend = fake;

                    connection.invoke("SendGameData", roomId, "balloon", valueToSend)
                        .catch(err => console.error("Send error:", err));
                    -------------------------------------------- */
                }
            }
        }, 500);

        return () => clearInterval(interval);
    }, [connection, roomId, gameState?.isFinished, isConnected, gsrValue]);

    if (!gameState) {
        return (
            <div className="d-flex justify-content-center align-items-center bg-dark text-white" style={{ height: '100vh' }}>
                <div className="text-center">
                    <Spinner animation="border" variant="primary" className="mb-3" />
                    <h4>Připojování k závodu...</h4>
                </div>
            </div>
        );
    }

    // Rozdělení hráčů pro UI (max 4 hráči)
    // Hráč 1, 3 vlevo | Hráč 2, 4 vpravo
    const leftPlayers = gameState.players.filter((_, i) => i === 0 || i === 2);
    const rightPlayers = gameState.players.filter((_, i) => i === 1 || i === 3);

    return (
        <Container fluid className="py-4 bg-dark text-white" style={{ minHeight: '100vh' }}>
            <GameHeader gameName="balloon" userEmail={userEmail} />

            <div className="text-center mb-4">
                <span className={`badge p-2 ${isConnected ? 'bg-success' : 'bg-warning text-dark'}`}>
                    {isConnected ? `📡 Senzor: ${gsrValue?.toFixed(0)}` : '🧪 Simulační režim'}
                </span>
            </div>

            <Row className="mt-2 align-items-start">
                {/* LEVÝ SLOUPEC */}
                <Col md={3}>
                    {leftPlayers.map((p, i) => (
                        <PlayerInfoCard
                            key={p.email}
                            player={p}
                            playerNumber={(i * 2) + 1}
                            isMe={p.email === userEmail}
                        />
                    ))}
                </Col>

                {/* STŘED - HŘIŠTĚ */}
                <Col md={6} className="px-3">
                    <div className="bg-secondary bg-opacity-10 rounded p-3 shadow-lg border border-secondary">
                        <BalloonPlayground players={gameState.players} />
                        <div className="text-center mt-3 text-white-50 small fw-bold">
                            ZÁVODNÍ PROSTOR ({gameState.players.length} / 4)
                        </div>
                    </div>
                </Col>

                {/* PRAVÝ SLOUPEC */}
                <Col md={3}>
                    {rightPlayers.map((p, i) => (
                        <PlayerInfoCard
                            key={p.email}
                            player={p}
                            playerNumber={(i * 2) + 2}
                            isMe={p.email === userEmail}
                        />
                    ))}
                </Col>
            </Row>

            <GameOverModal
                show={gameState.isFinished}
                result={{
                    winnerId: gameState.winner,
                    isWin: gameState.winner === userEmail,
                    reason: gameState.endReason || "Závod skončil!"
                }}
                currentPlayerId={userEmail}
                onRetry={() => window.location.href = '/games/balloon'}
            />
        </Container>
    );
}