// import React, { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route, Outlet } from 'react-router-dom';
import AppsNavbar from './commpoment/AppsNavbar';
import AppsFooter from './commpoment/AppsFooter';
// import { startConnection } from './services/GameConnection';
import './App.css';
import RequireAuth from './commpoment/RequireAuth';
import StatistikDetailPage from "./pages/StatistikDetailPage";

// Stránky (můžeš si je zatím vytvořit jako jednoduché komponenty)
import HryPage from './pages/GamePage';
import StatistikyPage from './pages/StatistikPage';
import BalloonGame from './features/games/balloon/BalloonGame';
import BalloonGameList from './features/games/balloon/BalloonGameList';
import BalanceGame from './features/games/ballance/BalanceGame';
import BalanceGameList from './features/games/ballance/BalanceGameList';
import EnergyBattelGameList from './features/games/energyBattel/EnergyBattelGameList';
import EnergyBattelGame from './features/games/energyBattel/EnergyBattelGame';
import MainPage from './pages/MainPage';

function App() {
  return (
    <BrowserRouter>
      <AppsNavbar />
      {/* <ConnectButton /> */}

      <div className="container mt-4">
        <Routes>
          <Route path="/" element={<MainPage />} />
          <Route element={<RequireAuth><Outlet /></RequireAuth>}>
            <Route path="/games/energybattle" element={<EnergyBattelGameList />} />
            <Route path="/games/balance" element={<BalanceGameList />} />
            <Route path="/games/balloon" element={<BalloonGameList />} />
            <Route path="/hry" element={<HryPage />} />
            <Route path="/statistiky" element={<StatistikyPage />} />
            <Route path="/stats/detail/:gameType" element={<StatistikDetailPage />} />
            <Route path="/stats/detail/:sessionId" element={<StatistikDetailPage />} />
            <Route path="/ballance/game/:roomId" element={<BalanceGame />} />
            <Route path="/games/balloon/:roomId" element={<BalloonGame />} />
            <Route path="/energybattle/game/:roomId" element={<EnergyBattelGame />} />
          </Route>
        </Routes>
      </div>
      <AppsFooter />
    </BrowserRouter>
  );
}

export default App;
