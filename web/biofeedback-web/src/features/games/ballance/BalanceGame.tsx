import { useEffect, useState } from "react";
import { Container, Row, Col, Spinner } from "react-bootstrap";
import { useParams, useLocation } from "react-router-dom";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import PlayerPanel from "./components/PlayerPanel";
import BalanceArena from "./components/BalancePanet";
import GameHeader from "../general/GameHeader";

// Definujeme rozhraní pro stav, který nám teď posílá C# server
interface BallanceGameState {
    ballPosition: number;
    leftValue: number;
    rightValue: number;
    leftPlayerId: string | null;
    rightPlayerId: string | null;
    isGameOver: boolean;
    remainingTime: number;
}

export default function BalanceGame() {
    const { roomId } = useParams<{ roomId: string }>();
    const location = useLocation();
    const passedRoomName = (location.state as any)?.roomName as string | undefined;
    const [roomName, setRoomName] = useState<string | null>(passedRoomName ?? null);
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [timeLeft, setTimeLeft] = useState<number>(120);
    const [gameOver, setGameOver] = useState(false);

    // Stavy pro hráče a kuličku (nyní synchronizované se serverem)
    const [leftPlayer, setLeftPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [rightPlayer, setRightPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [ballPos, setBallPos] = useState<number>(50); // 50 je střed

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
                accessTokenFactory: () => localStorage.getItem("token") ?? ""
            })
            .withAutomaticReconnect()
            .build();

        // POSLOUCHÁME SERVER: Přijímáme kompletní vypočítaný stav
        conn.on("ReceiveGameState", (state: BallanceGameState) => {
            setLeftPlayer({ id: state.leftPlayerId, value: state.leftValue });
            setRightPlayer({ id: state.rightPlayerId, value: state.rightValue });
            setBallPos(state.ballPosition);
            setTimeLeft(state.remainingTime);

            if (state.isGameOver) {
                setGameOver(true);
                console.log("Hra skončila!");
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
        // Pokud je hra u konce, interval vůbec nespouštěj
        if (gameOver) return;

        const interval = setInterval(() => {
            if (connection && connection.state === "Connected") {
                connection.invoke("SendGameData", roomId, "ballance", Math.random() * 1000)
                    .catch(err => console.error("Error sending data:", err));
            }
        }, 1000);

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
                        recentMin={0}
                        recentMax={1000}
                    />
                </Col>

                {/* Centrální aréna - využívá ballPos ze serveru */}
                <Col md={6}>
                    <BalanceArena
                        leftValue={leftPlayer.value}
                        rightValue={rightPlayer.value}
                    // Pokud tvůj BalanceArena komponent podporuje přímé nastavení pozice, 
                    // můžeš mu ji předat, jinak si ji teď vypočítá z průměrů, které mu posílá server.
                    />
                </Col>

                {/* Hráč 2 (Pravý) */}
                <Col md={3}>
                    <PlayerPanel
                        value={rightPlayer.value}
                        label={rightPlayer.id ? `Hráč (P)` : "Volné místo"}
                        recentMin={0}
                        recentMax={1000}
                    />
                </Col>
            </Row>
            {gameOver && (
                <div className="absolute inset-0 bg-white/50 flex items-center justify-center z-50">
                    <div className="bg-white p-8 rounded-2xl shadow-2xl text-center border-4 border-red-500">
                        <h2 className="text-4xl font-black text-red-500 mb-2">KONEC HRY!</h2>
                        <p className="text-gray-600">Čas vypršel nebo kulička vypadla z arény.</p>
                        <button
                            onClick={() => window.location.reload()}
                            className="mt-4 px-6 py-2 bg-blue-500 text-white rounded-full font-bold"
                        >
                            Hrát znovu
                        </button>
                    </div>
                </div>
            )}
        </Container>
    );
}