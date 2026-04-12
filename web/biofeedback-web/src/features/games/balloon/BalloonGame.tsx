import { useEffect, useState, useCallback } from "react";
import { useParams, useLocation } from "react-router-dom";
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

    // Získání dat z Bluetooth senzoru
    const { gsrValue, isConnected } = useBle();
    const [simulatedGsr, setSimulatedGsr] = useState<number>(0);

    // Pokud je BLE připojeno, použijeme gsrValue, jinak náhodnou simulaci
    const effectiveGsr = isConnected ? (gsrValue || 0) : simulatedGsr;

    const userEmail = JSON.parse(localStorage.getItem('user') || '{}').email;

    // Funkce pro získání tokenu (stejná jako v EnergyBattle)
    const getValidToken = useCallback(async (): Promise<string> => {
        const token = localStorage.getItem("token");
        return token || "";
    }, []);

    useEffect(() => {
        if (!roomId) return;

        const conn = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL}/gamehub`, {
                accessTokenFactory: async () => {
                    let token = localStorage.getItem("token");
                    if (token) {
                        try {
                            const payload = JSON.parse(atob(token.split(".")[1]));
                            const isExpired = payload.exp * 1000 < Date.now();

                            if (isExpired) {
                                console.log("Token v SignalR vypršel, volám refresh...");
                                const res = await api.post('/refresh');
                                token = res.data.token || res.data.Token || res.data.accessToken;
                            }
                        } catch (e) {
                            console.error("Chyba při refreshování tokenu pro SignalR:", e);
                        }
                    }
                    return token || "";
                }
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Critical)
            .build();

        conn.onreconnected((connectionId) => {
            console.log("SignalR Reconnected. Re-joining room...");
            if (conn.state === "Connected") {
                conn.invoke("JoinRoom", roomId).catch(err => console.error("JoinRoom failed:", err));
            }
        });

        conn.on("ReceiveGameState", (state: BalloonGameState) => {
            setGameState(state);
        });

        conn.start()
            .then(() => conn.invoke("JoinRoom", roomId))
            .catch(err => console.error("SignalR Connection Error: ", err));

        setConnection(conn);

        return () => { conn.stop(); };
    }, [roomId, getValidToken]);

    // INTERVAL PRO ODESÍLÁNÍ BIOFEEDBACKU
    useEffect(() => {
        // Změna: Odebrali jsme podmínku !gameState
        // Hra se musí "nakopnout" prvním odesláním dat
        if (!connection || (gameState && gameState.isFinished)) return;

        const interval = setInterval(() => {
            if (connection.state === "Connected") {
                let valueToSend: number;
                if (isConnected) {
                    valueToSend = gsrValue || 0;
                } else {
                    const fake = Math.random() * 1000;
                    setSimulatedGsr(fake);
                    valueToSend = fake;
                }

                connection.invoke("SendGameData", roomId, "balloon", valueToSend)
                    .catch(err => console.error("Chyba odesílání:", err));
            }
        }, 500);

        return () => clearInterval(interval);
    }, [connection, roomId, gameState?.isFinished, isConnected, gsrValue]);

    if (!gameState) return <div className="text-center p-5"><Spinner /> Načítání hry...</div>;

    const leftPlayers = gameState.players.filter((_, i) => i % 2 === 0);
    const rightPlayers = Array.from({ length: Math.max(gameState.players.length, 2) })
        .map((_, i) => gameState.players[i])
        .filter((_, i) => i % 2 !== 0);

    return (
        <Container fluid className="py-4 bg-dark text-white" style={{ minHeight: '100vh' }}>
            <GameHeader gameName="balloon" userEmail={userEmail} />

            <div className="text-center mb-3">
                <span className={`badge ${isConnected ? 'bg-success' : 'bg-warning text-dark'}`}>
                    {isConnected ? `Senzor připojen: ${gsrValue}` : 'Simulace senzoru (Demo)'}
                </span>
            </div>

            <Row className="mt-4 g-0">
                {/* LEVÝ SLOUPEC (Hráč 1, Hráč 3) */}
                <Col md={2}>
                    {leftPlayers.map((p, i) => (
                        <div key={i} className="mb-4">
                            <PlayerInfoCard
                                player={p}
                                playerNumber={(i * 2) + 1}
                                isMe={p?.email === userEmail}
                                align="start"
                            />
                        </div>
                    ))}
                </Col>

                {/* STŘED - HŘIŠTĚ */}
                <Col md={8} className="px-3">
                    <BalloonPlayground players={gameState.players} />
                    <div className="text-center mt-2 text-white-50 small">
                        Cíl: {gameState.players.length} / 2-4 hráči připojeni
                    </div>
                </Col>

                {/* PRAVÝ SLOUPEC (Hráč 2, Hráč 4) */}
                <Col md={2}>
                    {rightPlayers.map((p, i) => (
                        <div key={i} className="mb-4">
                            <PlayerInfoCard
                                player={p}
                                playerNumber={(i * 2) + 2}
                                isMe={p?.email === userEmail}
                                align="end"
                            />
                        </div>
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