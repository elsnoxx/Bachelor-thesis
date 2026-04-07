import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";
import BalloonPlayground from "./components/BalloonPlayground";
import { Container, Alert, Spinner } from "react-bootstrap";

export default function BalloonGame() {
    const { roomId } = useParams<{ roomId: string }>();
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [gameState, setGameState] = useState<any>(null);

    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL}/gamehub`, {
                accessTokenFactory: () => localStorage.getItem("token") || ""
            })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    connection.invoke("JoinRoom", roomId);
                    
                    connection.on("ReceiveGameState", (state) => {
                        setGameState(state);
                    });

                    // Simulace odesílání dat ze senzoru (v reálu nahraď eventem ze senzoru)
                    const interval = setInterval(() => {
                        const mockGSR = Math.random() * 400; // Náhodná výška pro test
                        connection.invoke("SendGameData", roomId, "balloon", mockGSR);
                    }, 100);

                    return () => clearInterval(interval);
                })
                .catch(e => console.log('Connection failed: ', e));
        }
    }, [connection, roomId]);

    if (!gameState) return <Spinner animation="border" />;

    return (
        <Container fluid className="mt-4">
            {gameState.isFinished && (
                <Alert variant="success">
                    Hra skončila! Vítěz: {gameState.winner}
                </Alert>
            )}
            <BalloonPlayground players={gameState.players} />
        </Container>
    );
}