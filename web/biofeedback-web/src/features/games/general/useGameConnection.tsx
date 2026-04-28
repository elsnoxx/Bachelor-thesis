import { useEffect, useState, useRef } from "react";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useBle } from "../../../services/BleProvider";
import api from "../../../api/axiosInstance";

interface UseGameConnectionProps {
    roomId: string | undefined;
    gameType: "balloon" | "energybattle" | "ballance";
    onStateReceived: (state: any) => void;
}


export function useGameConnection({ roomId, gameType, onStateReceived }: UseGameConnectionProps) {
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const connRef = useRef<HubConnection | null>(null);
    const { gsrValue, isConnected } = useBle();
    const [simulatedGsr, setSimulatedGsr] = useState<number>(0);

    useEffect(() => {
        if (!roomId || connRef.current) return;

        const token = localStorage.getItem("token");
        if (!token) {
            console.error("No token found in localStorage!");
            return;
        }

        const newConnection = new HubConnectionBuilder()
            .withUrl(`${import.meta.env.VITE_API_URL}/gamehub`, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        newConnection.on("ReceiveGameState", (state) => {
            onStateReceived(state);
        });

        newConnection.on("PlayerJoined", () => {});
        newConnection.on("PlayerLeft", () => {});

        const start = async () => {
            try {
                await newConnection.start();
                console.log("SignalR Connected to room:", roomId);
                await newConnection.invoke("JoinRoom", roomId);
                connRef.current = newConnection;
                setConnection(newConnection);
            } catch (err) {
                console.error("Start failed:", err);
                setTimeout(start, 3000);
            }
        };

        start();

        return () => {
            if (connRef.current) {
                connRef.current.stop();
                connRef.current = null;
            }
        };
    }, [roomId]); // Důležité: roomId je jediná závislost

    // odesílání dat
    useEffect(() => {
        if (!connection || connection.state !== "Connected" || !roomId) return;

        const interval = setInterval(() => {
            let valueToSend: number;
            if (isConnected && gsrValue !== null) {
                valueToSend = gsrValue;
            } else {
                const fake = Math.random() * (gameType === "ballance" ? 1000 : 100);
                setSimulatedGsr(fake);
                valueToSend = fake;
            }
            if (connection.state === "Connected") {
                connection.invoke("SendGameData", roomId, gameType, valueToSend)
                    .catch(err => console.error(`Error sending ${gameType} data:`, err));
            }
        }, gameType === "ballance" ? 100 : 500);

        return () => clearInterval(interval);
    }, [connection, roomId, isConnected, gsrValue, gameType]);

    return {
        connection,
        effectiveGsr: isConnected ? (gsrValue || 0) : simulatedGsr,
        isConnected
    };
}