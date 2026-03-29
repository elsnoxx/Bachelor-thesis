import React, { useEffect, useState, useMemo } from "react";
import { Container, Row, Col, Spinner } from "react-bootstrap";
import { useParams } from "react-router-dom";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import PlayerPanel from "./components/PlayerPanel";
import BalanceArena from "./components/BalancePanet";

interface EnergyBattlePacket {
    playerId: string;
    value: number;
    timestamp: string;
}

export default function BalanceGame() {
    const { roomId } = useParams<{ roomId: string }>();
    const [connection, setConnection] = useState<HubConnection | null>(null);
    
    // Stavy pro oba hráče
    const [leftPlayer, setLeftPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });
    const [rightPlayer, setRightPlayer] = useState<{ id: string | null, value: number }>({ id: null, value: 500 });

    useEffect(() => {
        if (!roomId) return;

        const conn = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL}/gamehub`, {
                accessTokenFactory: () => localStorage.getItem("token") ?? ""
            })
            .withAutomaticReconnect()
            .build();

        conn.on("ReceiveEnergyData", (packet: EnergyBattlePacket) => {
            setLeftPlayer(prev => {
                // Pokud je to první hráč, co poslal data, nebo už existující "levý" hráč
                if (prev.id === null || prev.id === packet.playerId) {
                    return { id: packet.playerId, value: packet.value };
                }
                // Pokud to není levý hráč, zkusíme pravého
                setRightPlayer(rightPrev => {
                    if (rightPrev.id === null || rightPrev.id === packet.playerId) {
                        return { id: packet.playerId, value: packet.value };
                    }
                    return rightPrev;
                });
                return prev;
            });
        });

        conn.start()
            .then(() => conn.invoke("JoinRoom", roomId))
            .catch(err => console.error("SignalR Connection Error: ", err));

        setConnection(conn);

        return () => {
            conn.stop();
        };
    }, [roomId]);

    // Simulace odesílání vlastních dat (pokud bys měl senzor, voláš tohle)
    // useEffect(() => {
    //     const interval = setInterval(() => {
    //         if (connection) connection.invoke("SendEnergyData", roomId, Math.random() * 1000);
    //     }, 1000);
    //     return () => clearInterval(interval);
    // }, [connection, roomId]);

    if (!connection) return <div className="text-center p-5"><Spinner /> Připojování k aréně...</div>;

    return (
        <Container fluid className="h-screen py-4">
            <Row className="mb-4">
                <Col className="bg-light rounded-lg shadow p-4 text-center">
                    <h3 className="font-semibold">Biofeedback Balance Aréna</h3>
                    <p className="text-sm text-muted">Místnost: {roomId}</p>
                    <small>Uklidni se, abys přetáhl váhu na svou stranu</small>
                </Col>
            </Row>

            <Row className="mb-4 align-items-stretch">
                {/* Hráč 1 (Levý) */}
                <Col md={3}>
                    <PlayerPanel 
                        value={leftPlayer.value} 
                        label={leftPlayer.id ? `Hráč (L)` : "Čekání na hráče..."} 
                        recentMin={400} 
                        recentMax={800} 
                    />
                </Col>

                {/* Centrální aréna - přepočítává rozdíl mezi GSR */}
                <Col md={6}>
                    <BalanceArena 
                        leftValue={leftPlayer.value} 
                        rightValue={rightPlayer.value} 
                    />
                </Col>

                {/* Hráč 2 (Pravý) */}
                <Col md={3}>
                    <PlayerPanel 
                        value={rightPlayer.value} 
                        label={rightPlayer.id ? `Hráč (P)` : "Volné místo"} 
                        recentMin={400} 
                        recentMax={800} 
                    />
                </Col>
            </Row>
        </Container>
    );
}