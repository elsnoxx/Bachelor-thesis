import React, { useMemo } from "react";
import { Stage, Layer, Rect, Circle, Group, Text } from "react-konva";
import type { Piece } from '../logic/LudoEngine'

// PATH_COORDS: pole 52 položek: [ {r,c}, ... ] — upravte podle vaší desky.
// Níže je příklad mapování okolo cross v 15x15. Pokud nebude přesné, doladíme.
const PATH_COORDS: { r:number; c:number }[] = [
  {r:1,c:6},{r:2,c:6},{r:3,c:6},{r:4,c:6},{r:5,c:6}, // přibližně nahoru od startu
  {r:6,c:6},{r:6,c:5},{r:6,c:4},{r:6,c:3},{r:6,c:2},{r:6,c:1},
  {r:6,c:0},{r:7,c:0},{r:8,c:0},{r:8,c:1},{r:8,c:2},{r:8,c:3},
  {r:8,c:4},{r:8,c:5},{r:8,c:6},{r:9,c:6},{r:10,c:6},{r:11,c:6},
  {r:12,c:6},{r:13,c:6},{r:14,c:6},{r:14,c:7},{r:13,c:7},{r:12,c:7},
  {r:11,c:7},{r:10,c:7},{r:9,c:7},{r:8,c:7},{r:8,c:8},{r:8,c:9},
  {r:8,c:10},{r:8,c:11},{r:8,c:12},{r:8,c:13},{r:8,c:14},{r:7,c:14},
  {r:6,c:14},{r:6,c:13},{r:6,c:12},{r:6,c:11},{r:6,c:10},{r:6,c:9},
  {r:6,c:8},{r:5,c:8}
];
// Poznámka: doplňte/přesně mapujte na 52 položek

interface LudoBoardProps { boardSize?: number; pieces?: Piece[]; onPieceClick?: (id:string)=>void; }

export default function LudoBoard({ boardSize=600, pieces=[], onPieceClick=()=>{} }: LudoBoardProps) {
  const cellSize = boardSize / 15;
  const pixelRatio = typeof window !== "undefined" ? window.devicePixelRatio || 1 : 1;

  const cells = useMemo(() => {
    const out = [];
    for (let r=0;r<15;r++){
      for (let c=0;c<15;c++){
        out.push({r,c});
      }
    }
    return out;
  },[]);

  const coordFor = (posIndex:number) => {
    if (posIndex < 0) return {r:0,c:0}; // doma -> ignorujte nebo zobrazte v domečku
    if (posIndex < PATH_COORDS.length) return PATH_COORDS[posIndex];
    // home steps: umístit na center path (nutno doplnit)
    return { r:7, c:7 };
  };

  return (
    <Stage width={boardSize} height={boardSize} pixelRatio={pixelRatio}>
      <Layer>
        {cells.map(({r,c})=>(
          <Rect key={`${r}-${c}`} x={c*cellSize} y={r*cellSize} width={cellSize} height={cellSize} stroke="#aaa" />
        ))}

        {pieces.map(p=>{
          const coord = coordFor(p.posIndex);
          const x = coord.c * cellSize + cellSize/2;
          const y = coord.r * cellSize + cellSize/2;
          return (
            <Group key={p.id} x={x} y={y} onClick={()=>onPieceClick(p.id)}>
              <Circle radius={cellSize*0.38} fill={p.playerId === 'r' ? '#f44336' : p.playerId==='b' ? '#2196f3' : '#4caf50'} />
              <Text text={p.id} fontSize={cellSize*0.18} x={-cellSize*0.15} y={-cellSize*0.12} />
            </Group>
          );
        })}
      </Layer>
    </Stage>
  );
}