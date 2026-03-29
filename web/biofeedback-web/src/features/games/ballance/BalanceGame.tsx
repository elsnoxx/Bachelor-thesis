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
}

export default function BalanceGame() {
    const { roomId } = useParams<{ roomId: string }>();
    const location = useLocation();
    const passedRoomName = (location.state as any)?.roomName as string | undefined;
    const [roomName, setRoomName] = useState<string | null>(passedRoomName ?? null);
    const [connection, setConnection] = useState<HubConnection | null>(null);

    // Stavy pro hráče a kuličku (nyní synchronizované se serverem)
    const [leftPlayer, setLeftPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [rightPlayer, setRightPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [ballPos, setBallPos] = useState<number>(50); // 50 je střed

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
            
            if (state.isGameOver) {
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
        const interval = setInterval(() => {
            if (connection && connection.state === "Connected") {
                // Voláme novou metodu SendGameData s typem "ballance"
                connection.invoke("SendGameData", roomId, "ballance", Math.random() * 1000)
                    .catch(err => console.error("Error sending data:", err));
            }
        }, 1000); // každou sekundu pošle náhodné číslo 0-1000

        return () => clearInterval(interval);
    }, [connection, roomId]);

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
            
            <Row className="mb-4">
                <Col className="bg-light rounded-lg shadow p-4 text-center">
                    <h3 className="font-semibold">Biofeedback Balance Aréna</h3>
                    <p className="text-sm text-muted">Místnost: {roomName ?? roomId}</p>
                    <small>Uklidni se, abys udržel kuličku uprostřed. Server průměruje tvých posledních 30 hodnot.</small>
                </Col>
            </Row>

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
        </Container>
    );
}