import React, { useEffect, useState } from "react";
import { Container } from "react-bootstrap";
import { useParams, useLocation } from "react-router-dom";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

interface EnergyBattlePacket {
  playerId: string;
  value: number;
  timestamp?: string;
}

export default function EnergyBattelGame() {
  const { roomId } = useParams<{ roomId: string }>();
  const location = useLocation();
    const passedRoomName = (location.state as any)?.roomName as string | undefined;
    const [roomName, setRoomName] = useState<string | null>(passedRoomName ?? null);
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [packets, setPackets] = useState<EnergyBattlePacket[]>([]);

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

    // Definice handleru
    conn.on("ReceiveEnergyData", (data: EnergyBattlePacket) => {
      console.log("Nová data:", data);
      setPackets(prev => [...prev, data].slice(-20)); // Necháme si jen posledních 20 pro výkon
    });

    const start = async () => {
      try {
        await conn.start();
        console.log("SignalR Connected.");
        // Klíčové: Musí se jmenovat stejně jako v C# (JoinRoom)
        await conn.invoke("JoinRoom", roomId);
      } catch (err) {
        console.error("SignalR Error:", err);
      }
    };

    start();
    setConnection(conn);

    return () => {
      conn.stop();
    };
  }, [roomId]);

  // Funkce pro testovací odeslání dat (simulace senzoru)
  const sendTestData = () => {
    if (connection && roomId) {
      connection.invoke("SendEnergyData", roomId, Math.random() * 100)
        .catch(err => console.error(err));
    }
  };

  return (
    <Container fluid className="py-4">
      <h1>Energy battle — místnost {roomName ?? roomId}</h1>
      <button onClick={sendTestData} className="btn btn-primary mb-3">
        Poslat náhodná data (Test)
      </button>
      
      <div className="mt-4">
        <h3>Live Data Stream:</h3>
        <ul className="list-group">
          {packets.map((p, i) => (
            <li key={i} className="list-group-item d-flex justify-content-between">
              <span>Hráč: {p.playerId}</span>
              <strong>{p.value.toFixed(2)} GSR</strong>
            </li>
          ))}
        </ul>
      </div>
    </Container>
  );
}