import React from "react";
import LudoCell from "./LudoCell";
import PlayerPiece from "./PlayerPiece";

export default function LudoBoard({ board, players }) {
  return (
    <div className="grid grid-cols-15 grid-rows-15 gap-0 border-4 border-gray-700 bg-gray-50 w-[600px] h-[600px]">
      {board.map((row, y) =>
        row.map((cell, x) => (
          <LudoCell key={`${x}-${y}`} type={cell.type}>
            {cell.player && (
              <PlayerPiece color={cell.player.color} id={cell.player.id} />
            )}
          </LudoCell>
        ))
      )}
    </div>
  );
}
