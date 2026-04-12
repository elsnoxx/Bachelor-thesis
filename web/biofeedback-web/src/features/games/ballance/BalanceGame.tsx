import { useEffect, useState } from "react";
import { Container, Row, Col, Spinner } from "react-bootstrap";
import { useParams, useLocation } from "react-router-dom";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import PlayerPanel from "./components/PlayerPanel";
import BalanceArena from "./components/BalancePanet";
import GameHeader from "../general/GameHeader";
import GameOverModal from "../general/GameOverModal";
import api from "../../../api/axiosInstance";

// Definujeme rozhraní pro stav, který nám teď posílá C# server
interface BallanceGameState {
    ballPosition: number;
    leftValue: number;
    rightValue: number;
    leftPlayerId: string | null;
    rightPlayerId: string | null;
    isGameOver: boolean;
    remainingTime: number;
    isWin: boolean;
    endReason: string | null;
    targetMin: number;
    targetMax: number;
    targetMinPlayer: number;
    targetMaxPlayer: number;
}

export default function BalanceGame() {
    const { roomId } = useParams<{ roomId: string }>();
    const location = useLocation();
    const passedRoomName = (location.state as any)?.roomName as string | undefined;
    const [roomName, setRoomName] = useState<string | null>(passedRoomName ?? null);
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [timeLeft, setTimeLeft] = useState<number>(120);
    const [gameOver, setGameOver] = useState(false);
    const [gameResult, setGameResult] = useState<{ isWin: boolean, reason: string | null } | null>(null);

    // Stavy pro hráče a kuličku (nyní synchronizované se serverem)
    const [leftPlayer, setLeftPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [rightPlayer, setRightPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [ballPos, setBallPos] = useState<number>(50); // 50 je střed
    const [targetLimits, setTargetLimits] = useState({ min: 40, max: 60 });
    const [playerLimits, setPlayerLimits] = useState({ min: 400, max: 600 });

    // setTimeLeft(state.remainingTime);

    const formatTime = (seconds: number) => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    };

    useEffect(() => {
        if (!passedRoomName && roomId) {
            const fallback = sessionStorage.getItem(`roomName_${roomId}`);
            if (fallback) setRoomName(fallback);
        }

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
            .configureLogging(LogLevel.Information)
            .build();

        // POSLOUCHÁME SERVER: Přijímáme kompletní vypočítaný stav
        conn.on("ReceiveGameState", (state: BallanceGameState) => {
            setLeftPlayer({ id: state.leftPlayerId, value: state.leftValue });
            setRightPlayer({ id: state.rightPlayerId, value: state.rightValue });
            setBallPos(state.ballPosition);
            setTimeLeft(state.remainingTime);

            // Synchronizace všech limitů
            if (state.targetMin !== undefined) {
                // Limity pro arénu (0-100)
                setTargetLimits({ min: state.targetMin, max: state.targetMax });
                // Limity pro hráče (0-1000)
                setPlayerLimits({ min: state.targetMinPlayer, max: state.targetMaxPlayer });
            }

            if (state.isGameOver) {
                setGameOver(true);
                setGameResult({
                    winnerId: state.winnerId ?? null,
                    reason: state.endReason ?? null
                });
            }
        });

        conn.onreconnected((connectionId) => {
            console.log("SignalR Reconnected. Re-joining room...");
            if (conn.state === "Connected") {
                conn.invoke("JoinRoom", roomId).catch(err => console.error("JoinRoom failed:", err));
            }
        });

        conn.start()
            .then(() => conn.invoke("JoinRoom", roomId))
            .catch(err => console.error("SignalR Connection Error: ", err));

        setConnection(conn);

        return () => {
            conn.stop();
        };
    }, [roomId]);

    // SIMULACE ODESÍLÁNÍ DAT
    useEffect(() => {
        if (gameOver || !connection || connection.state !== "Connected") return;

        let simulatedValue = 500; // Výchozí hodnota v mS
        const interval = setInterval(() => {
            // Simulace reálnějšího pohybu signálu (náhodná procházka)
            // Hodnota se nemění skokově o 1000, ale postupně +/- 5 jednotek
            const change = (Math.random() - 0.5) * 10;
            simulatedValue = Math.max(0, Math.min(1000, simulatedValue + change));

            connection.invoke("SendGameData", roomId, "ballance", simulatedValue)
                .catch(err => console.error("Simulace selhala:", err));

        }, 100);

        return () => clearInterval(interval);
    }, [connection, roomId, gameOver]);

    const getUserEmail = (): string | null => {
        const userJson = localStorage.getItem('user');
        if (userJson) {
            try { return JSON.parse(userJson).email || null; } catch { }
        }
        return null;
    };

    if (!connection) return <div className="text-center p-5"><Spinner /> Připojování k aréně...</div>;

    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
    const currentUserId = currentUser.id;

    return (
        <Container fluid className="h-screen py-4">
            <GameHeader gameName="ballance" userEmail={getUserEmail()} />

            <Container className="mb-4">
                <Row className="shadow-sm bg-light rounded p-4 align-items-center">
                    <h3 className="font-semibold m-0 text-center mb-3">Biofeedback Balance Aréna</h3>
                    <Col md={8} className="text-start ">

                        <div className="d-flex gap-3 align-items-center">
                            <span className="text-sm text-muted">Místnost: <strong>{roomName ?? roomId}</strong></span>
                            <small className="text-muted border-start ps-3">
                                Uklidni se, udržuj střed. Průměr z 30 hodnot.
                            </small>
                        </div>
                    </Col>

                    {/* Pravá část: Časovač vedle sebe */}
                    <Col md={4} className="text-end">
                        <div className="d-flex align-items-center justify-content-end gap-3">
                            <span className="text-xs font-bold text-gray-400 uppercase tracking-widest">
                                Zbývající čas:
                            </span>
                            <span className={`text-4xl font-black transition-colors ${timeLeft < 10 ? 'text-red-500 animate-pulse' : 'text-gray-800'}`}>
                                {formatTime(timeLeft)}
                            </span>
                        </div>
                    </Col>
                </Row>
            </Container>




            <Row className="mb-4 align-items-stretch">
                {/* Hráč 1 (Levý) */}
                <Col md={3}>
                    <PlayerPanel
                        value={leftPlayer.value}
                        label={leftPlayer.id ? `Hráč (L)` : "Čekání na hráče..."}
                        targetMin={playerLimits.min}
                        targetMax={playerLimits.max}
                        recentMin={0}
                        recentMax={1000}
                    />
                </Col>

                {/* Centrální aréna */}
                <Col md={6}>
                    <BalanceArena
                        leftValue={leftPlayer.value}
                        rightValue={rightPlayer.value}
                        ballPos={ballPos}
                        targetMin={playerLimits.min}
                        targetMax={playerLimits.max}
                        min={0}
                        max={1000}
                    />
                </Col>

                {/* Hráč 2 (Pravý) */}
                <Col md={3}>
                    <PlayerPanel
                        value={rightPlayer.value}
                        label={rightPlayer.id ? `Hráč (P)` : "Volné místo"}
                        targetMin={playerLimits.min}
                        targetMax={playerLimits.max}
                        recentMin={0}
                        recentMax={1000}
                    />
                </Col>
            </Row>
            <GameOverModal
                show={gameOver}
                result={gameResult}
                currentPlayerId={currentUserId}
                onRetry={() => window.location.href = '/games/balance'}
            />
        </Container>
    );
}