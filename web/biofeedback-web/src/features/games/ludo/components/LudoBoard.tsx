import React from "react";
import LudoCell from "./LudoCell";

export default function LudoBoard() {
  // Vytvoření herní desky 15x15
  const createBoard = () => {
    const board = [];
    
    for (let row = 0; row < 15; row++) {
      const rowCells = [];
      for (let col = 0; col < 15; col++) {
        const cellType = getCellType(row, col);
        rowCells.push(
          <LudoCell key={`${row}-${col}`} type={cellType} text={getText(row, col)}>
            {/* Zde můžete přidat figurky */}
          </LudoCell>
        );
      }
      board.push(
        <div key={row} className="d-flex">
          {rowCells}
        </div>
      );
    }
    return board;
  };

  const getText = (row, col) => {
    if (row === 1 && col === 6) return "Start";
    if (row === 6 && col === 13) return "Start";
    if (row === 8 && col === 1) return "Start";
    if (row === 13 && col === 8) return "Start";

    return null;
  };

  const getCellType = (row, col) => {

    // Domečky - cílové čtverce v centru
    if (col === 7 && (row > 0 && row < 6) || (row === 1 && col === 6)) return "SAFE-RED";
    if (row === 7 && (col > 8 && col < 14) || (row === 6 && col === 13)) return "SAFE-BLUE";
    if (row === 7 && (col > 0 && col < 6) || (row === 8 && col === 1)) return "SAFE-GREEN";
    if (col === 7 && (row > 8 && row < 14) || (row === 13 && col === 8)) return "SAFE-YELLOW";

    // Rohové čtverce (startovní pozice)
    if (row < 6 && col < 6) return "RED";
    if (row < 6 && col > 8) return "BLUE";
    if (row > 8 && col < 6) return "GREEN";
    if (row > 8 && col > 8) return "YELLOW";
    
    // Herní dráha
    if (row === 6 && (col < 6 || col > 8)) return "NORMAL";
    if (row === 8 && (col < 6 || col > 8)) return "NORMAL";
    if (col === 6 && (row < 6 || row > 8)) return "NORMAL";
    if (col === 8 && (row < 6 || row > 8)) return "NORMAL";

    // Herní dráha (vnější okraj)
    if (row === 6 && (col >= 0 && col <= 5)) return "NORMAL";
    if (row === 6 && (col >= 9 && col <= 14)) return "NORMAL";
    if (row === 8 && (col >= 0 && col <= 5)) return "NORMAL";
    if (row === 8 && (col >= 9 && col <= 14)) return "NORMAL";
    if (col === 6 && (row >= 0 && row <= 5)) return "NORMAL";
    if (col === 6 && (row >= 9 && row <= 14)) return "NORMAL";
    if (col === 8 && (row >= 0 && row <= 5)) return "NORMAL";
    if (col === 8 && (row >= 9 && row <= 14)) return "NORMAL";
    
    // Domečky (střed)
    if (row >= 6 && row <= 8 && col >= 6 && col <= 8) return "SAFE";

    
    
    return "NORMAL";
  };

  return (
    <div className="d-flex flex-column align-items-center">
      {createBoard()}
    </div>
  );
}