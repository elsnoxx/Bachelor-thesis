import { useCallback, useMemo } from "react";

export type PlayerColor = "RED"|"BLUE"|"GREEN"|"YELLOW";
export type Piece = { id: string; playerId: string; posIndex: number; finished?: boolean };
export type Player = { id: string; color: PlayerColor; startIndex: number };

export const BOARD_LEN = 52;
export const HOME_LEN = 6;

export function buildPlayerPath(startIndex: number) {
  const path: number[] = [];
  for (let i = 0; i < BOARD_LEN; i++) path.push((startIndex + i) % BOARD_LEN);
  for (let h = 0; h < HOME_LEN; h++) path.push(BOARD_LEN + h);
  return path;
}

export const SAFE_GLOBAL = new Set<number>([
  // upravte dle potřeby; příklad: startovy indexy
  0, 8, 13, 21, 26, 34, 39, 47
]);

export const rollDice = () => Math.floor(Math.random() * 6) + 1;

export function canMovePiece(piece: Piece, dice: number, player: Player, allPieces: Piece[]) {
  if (piece.finished) return false;
  const path = buildPlayerPath(player.startIndex);
  if (piece.posIndex < 0) return dice === 6;
  const newIndex = piece.posIndex + dice;
  if (newIndex >= path.length) return false;
  // nesmí stoupnout na vlastní figurku
  const own = allPieces.find(p => p.playerId === piece.playerId && p.posIndex === newIndex);
  if (own) return false;
  return true;
}

export function applyMove(piece: Piece, dice: number, player: Player, allPieces: Piece[]) {
  const path = buildPlayerPath(player.startIndex);
  if (piece.posIndex < 0 && dice === 6) {
    const moved = { ...piece, posIndex: 0 };
    return applyCaptures(moved, player, allPieces, path);
  }
  const newIndex = piece.posIndex + dice;
  if (newIndex >= path.length) return { success: false };
  const moved = { ...piece, posIndex: newIndex, finished: newIndex === path.length - 1 };
  return applyCaptures(moved, player, allPieces, path);
}

function applyCaptures(moved: Piece, player: Player, allPieces: Piece[], path: number[]) {
  const resultMoved = { ...moved };
  const targetGlobal = path[resultMoved.posIndex];
  const captures: Piece[] = [];
  if (targetGlobal < BOARD_LEN && !SAFE_GLOBAL.has(targetGlobal)) {
    for (const p of allPieces) {
      if (p.playerId === resultMoved.playerId) continue;
      if (p.posIndex >= 0) {
        const theirPath = buildPlayerPath(getStartForPlayer(p.playerId));
        const theirGlobal = theirPath[p.posIndex];
        if (theirGlobal === targetGlobal) {
          captures.push({ ...p, posIndex: -1, finished: false });
        }
      }
    }
  }
  return { success: true, moved: resultMoved, captures };
}

// Placeholder: mapujte playerId -> startIndex podle vaší konfigurace
export function getStartForPlayer(playerId: string) {
  if (playerId.startsWith("r")) return 0;
  if (playerId.startsWith("b")) return 13;
  if (playerId.startsWith("g")) return 26;
  return 39;
}