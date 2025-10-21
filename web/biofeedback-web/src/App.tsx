import React, { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import ConnectButton from './commpoment/ConnectButton';
import AppsNavbar from './commpoment/AppsNavbar';
import AppsFooter from './commpoment/AppsFooter';
import { startConnection } from './services/GameConnection';
import './App.css';

// Stránky (můžeš si je zatím vytvořit jako jednoduché komponenty)
import HryPage from './pages/GamePage';
import StatistikyPage from './pages/StatistickPage';

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
    <BrowserRouter>
      <AppsNavbar />
      {/* <ConnectButton /> */}

      <div className="container mt-4">
        <Routes>
          <Route path="/" element={
            <>
              <h2>Realtime Biofeedback</h2>
              <ul>
                {data.map((d, i) => (
                  <li key={i}>{d.playerId}: {d.value.toFixed(2)}</li>
                ))}
              </ul>
            </>
          } />
          <Route path="/hry" element={<HryPage />} />
          <Route path="/statistiky" element={<StatistikyPage />} />
        </Routes>
      </div>
      <AppsFooter />
    </BrowserRouter>
  );
}

export default App;
