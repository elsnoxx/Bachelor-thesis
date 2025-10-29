import React, { useEffect, useState } from "react";
import LudoBoard from "./components/LudoBoard";

export default function LudoGame() {
  return (
    <div>
      <h2 className="font-bold">Ludo Game</h2>
        <LudoBoard />
    </div>
  );
}
