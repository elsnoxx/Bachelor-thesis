import { useEffect, useRef, useState } from "react";
import { Container, Row, Col, Spinner } from "react-bootstrap";
import { useParams, useLocation } from "react-router-dom";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import PlayerPanel from "./components/PlayerPanel";
import BalanceArena from "./components/BalancePanet";
import GameHeader from "../general/GameHeader";
import GameOverModal from "../general/GameOverModal";
import api from "../../../api/axiosInstance";
import { useBle } from "../../../services/BleProvider";

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
    leftScl: number;
    rightScl: number;
    leftScr: boolean;
    rightScr: boolean;
    isCalibrating: boolean;
    calibrationTimeLeft: number;
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
    const [gameState, setGameState] = useState<BallanceGameState | null>(null);

    // Stavy pro hráče a kuličku (nyní synchronizované se serverem)
    const [leftPlayer, setLeftPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [rightPlayer, setRightPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [ballPos, setBallPos] = useState<number>(50); // 50 je střed
    const [targetLimits, setTargetLimits] = useState({ min: 40, max: 60 });
    const [playerLimits, setPlayerLimits] = useState({ min: 400, max: 600 });

    const { gsrValue, isConnected } = useBle();
    const simulatedValueRef = useRef<number>(500);

    const audioRef = useRef<HTMLAudioElement | null>(null);
    // Audio efekt - hluk narůstá s blížícím se okrajem
    useEffect(() => {
        if (!audioRef.current || !gameState) return;

        // Hlasitost od 0 do 1 podle toho, jak moc je kulička v "nebezpečí" (nad targetMax)
        const dangerLevel = Math.max(0, (gameState.ballPosition / 100));
        audioRef.current.volume = Math.min(1, dangerLevel);
    }, [gameState?.ballPosition]);
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

        conn.on("ReceiveGameState", (state: BallanceGameState) => {
            setGameState(state);

            setLeftPlayer({ id: state.leftPlayerId, value: state.leftValue });
            setRightPlayer({ id: state.rightPlayerId, value: state.rightValue });
            setBallPos(state.ballPosition);
            setTimeLeft(state.remainingTime);

            if (state.targetMin !== undefined) {
                // Limity pro arénu (0-100)
                setTargetLimits({ min: state.targetMin, max: state.targetMax });
                // Limity pro hráče (0-1000)
                setPlayerLimits({ min: state.targetMinPlayer, max: state.targetMaxPlayer });
            }

            if (state.isGameOver) {
                setGameOver(true);
                setGameResult({
                    isWin: state.isWin,
                    reason: state.endReason
                });
                console.log("Hra skončila!", state.isWin);
            }
        });

        conn.onreconnected((connectionId) => {
            console.log("SignalR reconnected:", connectionId, "Re-joining room...");
            if (conn.state === "Connected") {
                conn.invoke("JoinRoom", roomId).catch(err => console.error("JoinRoom failed:", err));
            }
        });

        conn.onclose((err) => {
            console.warn("SignalR closed:", err);
        });

        const startConnection = async () => {
            try {
                await conn.start();
                console.log("SignalR Connected.");
                await conn.invoke("JoinRoom", roomId);
            } catch (err) {
                console.error("SignalR Connection Error: ", err);
            }
        };

        startConnection();
        setConnection(conn);

        return () => {
            conn.stop().catch(() => { });
        };
    }, [roomId]);



    // SIMULACE ODESÍLÁNÍ DAT
    useEffect(() => {
        // Pokud hra skončila, nic neposíláme
        if (gameOver) return;

        const interval = setInterval(() => {
            if (!connection || connection.state !== "Connected") return;

            let valueToEmit: number | null = null;

            if (isConnected && gsrValue !== null && gsrValue !== undefined) {
                // PŘÍPAD A: Senzor je připojen a dává reálná data
                valueToEmit = gsrValue;
            }
            /* --- TESTOVACÍ SIMULÁTOR: ZAKOMENTOVÁNO PRO PRODUKCI (POUŽITÍ POUZE S REÁLNÝM SENZOREM) ---
            else {
                // PŘÍPAD B: Senzor není, simulujeme plynulou "náhodnou procházku"
                const change = (Math.random() - 0.5) * 20;
                simulatedValueRef.current = Math.max(0, Math.min(1000, simulatedValueRef.current + change));
                valueToEmit = simulatedValueRef.current;
            }
            ------------------------------------------------------------------------------------------ */

            // Data odesíláme pouze pokud byla získána ze senzoru (valueToEmit není null)
            if (valueToEmit !== null) {
                console.log("Odesílám reálnou hodnotu ze senzoru:", valueToEmit);
                connection.invoke("SendGameData", roomId, "ballance", valueToEmit)
                    .catch(err => console.error("Error sending data:", err));
            }

        }, 200);

        return () => clearInterval(interval);
    }, [connection, roomId, gameOver, isConnected, gsrValue]);

    const getUserEmail = (): string | null => {
        const userJson = localStorage.getItem('user');
        if (userJson) {
            try { return JSON.parse(userJson).email || null; } catch { }
        }
        return null;
    };

    const isConnReady = !!connection && connection.state === "Connected";

    // if (!connection || connection.state !== "Connected")
    //     return <div className="text-center p-5"><Spinner /> Připojování k aréně...</div>;

    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
    const currentUserId = currentUser.id;

    return (
        <Container fluid className="h-screen py-4">
            <audio ref={audioRef} src="/sounds/stress_noise.mp3" loop autoPlay />

            <GameHeader gameName="ballance" userEmail={getUserEmail()} />

            {!isConnReady ? (
                <div className="text-center p-5">
                    <Spinner /> Připojování k aréně...
                </div>
            ) : (
                <>
                    {/* sem vlož dosavadní obsah stránky (timery, aréna, panely) */}
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
                                    {gameState?.isCalibrating ? (
                                        <>
                                            <span className="text-xs font-bold text-primary uppercase">Kalibrace (buďte v klidu):</span>
                                            <span className="text-4xl font-black text-primary">
                                                {gameState.calibrationTimeLeft}s
                                            </span>
                                        </>
                                    ) : (
                                        <>
                                            <span className="text-xs font-bold text-gray-400 uppercase">Zbývající čas:</span>
                                            <span className={`text-4xl font-black ${timeLeft < 10 ? 'text-red-500 animate-pulse' : 'text-gray-800'}`}>
                                                {formatTime(timeLeft)}
                                            </span>
                                        </>
                                    )}
                                </div>
                            </Col>
                        </Row>
                    </Container>

                    <Row className="mt-4">
                        <Col md={3}>
                            <PlayerPanel
                                value={gameState?.leftScl ?? 0}
                                label="Hráč (L)"
                                isCalibrating={gameState?.isCalibrating}
                                showFlash={gameState?.leftScr}
                                max={300} // Relativní stupnice
                            />
                        </Col>

                        <Col md={6}>
                            <BalanceArena
                                ballPos={gameState?.ballPosition}
                                targetMin={0}
                                targetMax={gameState?.targetMax ?? 30}
                                isCalibrating={gameState?.isCalibrating}
                            />
                        </Col>

                        <Col md={3}>
                            <PlayerPanel
                                value={gameState?.rightScl ?? 0}
                                label="Hráč (P)"
                                isCalibrating={gameState?.isCalibrating}
                                showFlash={gameState?.rightScr}
                                max={300}
                            />
                        </Col>
                    </Row>

                    <GameOverModal
                        show={gameOver}
                        result={gameResult}
                        currentPlayerId={currentUserId}
                        onRetry={() => window.location.href = '/games/balance'}
                    />
                </>
            )}
        </Container>
    );
}