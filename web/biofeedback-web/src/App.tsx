// import React, { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route, Outlet } from 'react-router-dom';
// import ConnectButton from './commpoment/ConnectButton';
import LudoGameList from './features/games/ludo/LudoGameList';
import AppsNavbar from './commpoment/AppsNavbar';
import AppsFooter from './commpoment/AppsFooter';
// import { startConnection } from './services/GameConnection';
import './App.css';
import RequireAuth from './commpoment/RequireAuth';

// Stránky (můžeš si je zatím vytvořit jako jednoduché komponenty)
import HryPage from './pages/GamePage';
import StatistikyPage from './pages/StatistickPage';
import LudoGame from './features/games/ludo/LudoGame';
import CreateGamePage from './features/games/ludo/components/CreateGameModal';
import BalanceGame from './features/games/ballance/BalanceGame';
import BalanceGameList from './features/games/ballance/BalanceGameList';
import EnergyBattelGameList from './features/games/energyBattel/EnergyBattelGameList';

interface BioData {
  playerId: string;
  value: number;
}

function App() {
  // const [data, setData] = useState<BioData[]>([]);

  // useEffect(() => {
  //   startConnection("Player1", (playerId: string, value: number) => {
  //     setData(prev => [...prev, { playerId, value }]);
  //   });
  // }, []);

  return (
    <BrowserRouter>
      <AppsNavbar />
      {/* <ConnectButton /> */}

      <div className="container mt-4">
        <Routes>
          <Route path="/" element={
            <>
              <h2>Realtime Biofeedback</h2>
              {/* <ul>
                {data.map((d, i) => (
                  <li key={i}>{d.playerId}: {d.value.toFixed(2)}</li>
                ))}
              </ul> */}
            </>
          } />
          <Route element={<RequireAuth><Outlet /></RequireAuth>}>
            <Route path="/games/energybattle" element={<EnergyBattelGameList />} />
            <Route path="/games/balance" element={<BalanceGameList />} />
            <Route path="/games/ludo" element={<LudoGameList />} />
            <Route path="/hry" element={<HryPage />} />
            <Route path="/statistiky" element={<StatistikyPage />} />
          </Route>
        </Routes>
      </div>
      <AppsFooter />
    </BrowserRouter>
  );
}

export default App;
