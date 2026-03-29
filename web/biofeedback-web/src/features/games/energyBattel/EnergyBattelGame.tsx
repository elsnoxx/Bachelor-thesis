import React, { useEffect, useState } from "react";
import { Container } from "react-bootstrap";
import { useParams, useLocation } from "react-router-dom";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import GameHeader from "../general/GameHeader";
import { useBle } from "../../../services/BleProvider";

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
  const { gsrValue, isConnected } = useBle();

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
      setPackets(prev => [...prev, data].slice(-20));
    });

    const start = async () => {
      try {
        await conn.start();
        console.log("SignalR Connected.");
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

  const getUserEmail = (): string | null => {
    const userJson = localStorage.getItem('user');
    if (userJson) {
      try { return JSON.parse(userJson).email || null; } catch { }
    }
    const token = localStorage.getItem('token');
    if (token) {
      try { const p = JSON.parse(atob(token.split('.')[1])); return p.email || p.sub || null; } catch { }
    }
    return null;
  };

  

  useEffect(() => {
    if (connection && isConnected && gsrValue !== null) {
      connection.invoke("SendEnergyData", roomId, gsrValue)
        .catch(err => console.error(err));
    }
  }, [gsrValue, isConnected, connection, roomId]);

  return (
    <Container fluid className="py-4">
      <GameHeader gameName="energybattle" userEmail={getUserEmail()} />
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