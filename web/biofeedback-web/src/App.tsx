import React, { useEffect, useState } from 'react';
import ConnectButton from './commpoment/ConnectButton'
import './App.css'
import { startConnection } from './services/GameConnection';

interface BioData {
  playerId: string;
  value: number;
}

function App() {
  const [data, setData] = useState<BioData[]>([]);

  useEffect(() => {
    startConnection("Player1", (playerId: string, value: number) => {
      setData(prev => [...prev, { playerId, value }]);
    });
  }, []);

  return (
    <>
      <ConnectButton />
      <div>
        <h2>Realtime Biofeedback</h2>
        <ul>
          {data.map((d, i) => (
            <li key={i}>{d.playerId}: {d.value.toFixed(2)}</li>
          ))}
        </ul>
      </div>
    </>
  )
}

export default App