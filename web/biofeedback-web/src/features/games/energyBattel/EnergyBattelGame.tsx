import React, { useEffect, useState, useCallback } from "react";
import { Container } from "react-bootstrap";
import { useParams, useLocation } from "react-router-dom";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import GameHeader from "../general/GameHeader";
import { useBle } from "../../../services/BleProvider";
import { PlayerStatus } from "./components/PlayerStatus";
import { BattleControls } from "./components/BattleControls";
import type { GameParticipant, EnergyBattleState } from "./types";
import { GameOverModal } from "./components/GameOverModal";
import api from "../../../api/axiosInstance";


export default function EnergyBattleGame() {
  const { roomId } = useParams<{ roomId: string }>();
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [gameState, setGameState] = useState<EnergyBattleState | null>(null);
  const { gsrValue, isConnected } = useBle();

  const [simulatedGsr, setSimulatedGsr] = useState<number>(0);
  const effectiveGsr = isConnected ? (gsrValue || 0) : simulatedGsr;
  const [myEmail] = useState(() => {
    const userJson = localStorage.getItem('user');
    return userJson ? JSON.parse(userJson).email : null;
  });

  // Pomocný stav pro zastavení simulace
  const isGameOver = gameState?.me.health === 0 || (gameState?.opponent?.health === 0);

  useEffect(() => {
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

    conn.on("ReceiveGameState", (data: any) => {
      if (!data || !data.players) {
        console.warn("Obdržena neúplná data z ReceiveGameState:", data);
        return;
      }

      const myEmail = getUserEmail();
      const players = data.players as any[];

      // Používáme volitelné řetězení (?.) pro jistotu
      const meData = players.find((p) => p.email?.toLowerCase() === myEmail?.toLowerCase());
      const opponentData = players.find((p) => p.email?.toLowerCase() !== myEmail?.toLowerCase());

      if (meData) {
        setGameState({
          isStarted: data.isStarted,
          isFinished: data.isFinished,
          winner: data.winner,
          me: {
            energy: meData.energy,
            health: meData.health,
            target: meData.target,
            targetMin: meData.targetMin || 0,
            targetMax: meData.targetMax || 0,
            nextChangeIn: meData.nextChangeIn || 0,
            isReady: meData.isReady
          },
          opponent: opponentData ? {
            email: opponentData.email,
            energy: opponentData.energy,
            health: opponentData.health
          } : null
        });
      }
    });

    conn.onreconnected((connectionId) => {
      console.log("SignalR Reconnected. Re-joining room...");
      if (conn.state === "Connected") {
        conn.invoke("JoinRoom", roomId).catch(err => console.error("JoinRoom failed:", err));
      }
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
      conn.stop();
    };
  }, [roomId]);

  // SIMULÁTOR (Interval pro odesílání dat)
  useEffect(() => {
    if (isGameOver || !connection) return;

    const interval = setInterval(() => {
      if (connection.state === "Connected") {
        const fakeGsr = Math.random() * 100;

        // AKTUALIZACE LOKÁLNÍHO STAVU PRO UI
        if (!isConnected) {
          setSimulatedGsr(fakeGsr);
        }

        connection.invoke("SendGameData", roomId, "energybattle", fakeGsr)
          .catch(err => console.error("Chyba při odesílání simulace:", err));
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [connection, roomId, isGameOver, isConnected]);

  const handleFire = () => {
    if (connection) {
      // V Hubu se tato metoda jmenuje FireCannon nebo Fire? 
      // Podle tvého kódu Hubu je to FireCannon
      connection.invoke("FireCannon", roomId).catch(console.error);
    }
  };

  const getUserEmail = (): string | null => {
    const userJson = localStorage.getItem('user');
    if (userJson) {
      try { return JSON.parse(userJson).email || null; } catch { }
    }
    return null;
  };

  if (!gameState) return <div className="text-center mt-5">Čekání na data ze serveru...</div>;

  return (
    <Container fluid className="py-4 min-vh-100">
      <GameHeader gameName="energybattle" userEmail={getUserEmail()} />

      <div className="row g-4">
        {/* Levá strana - Moje staty */}
        <div className="col-md-6">
          <PlayerStatus
            name="Ty"
            data={gameState.me}
          />
        </div>

        {/* Pravá strana - Soupeřovy staty */}
        <div className="col-md-6">
          {gameState.opponent ? (
            <PlayerStatus
              name="Soupeř"
              data={gameState.opponent}
              isOpponent
            />
          ) : (
            <div className="p-5 text-center border rounded">Čekání na soupeře...</div>
          )}
        </div>
      </div>

      {/* Spodní část - Ovládání a Target */}
      <div className="row justify-content-center mt-4">
        <div className="col-lg-8">
          {gameState.isStarted ? (
            <BattleControls
              target={gameState.me.target}
              targetMin={gameState.me.targetMin || 0}
              targetMax={gameState.me.targetMax || 0}
              nextChangeIn={gameState.me.nextChangeIn || 0}
              currentValue={effectiveGsr}
              isReady={gameState.me.isReady}
              onFire={handleFire}
            />
          ) : (
            <div className="alert alert-info text-center">
              <h4>Čekání na soupeře...</h4>
              <p>Hra začne automaticky, jakmile se připojí druhý hráč.</p>
            </div>
          )}

        </div>

        <GameOverModal
          show={gameState.isFinished}
          winnerEmail={gameState.winner}
          myEmail={myEmail}
          onClose={() => { }}
        />

      </div>
    </Container>
  );
}