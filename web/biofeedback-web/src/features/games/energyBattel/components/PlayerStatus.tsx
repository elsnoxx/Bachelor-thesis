import { ProgressBar } from "react-bootstrap";
import type { GameParticipant } from "../types";

interface PlayerStatusProps {
  data: GameParticipant;
  isOpponent?: boolean;
  name: string;
}

export const PlayerStatus = ({ data, isOpponent, name }: PlayerStatusProps) => {
  return (
    <div className={`p-3 rounded shadow-sm ${isOpponent ? 'bg-light' : 'bg-white border-primary border'}`}>
      <h5>{name} {isOpponent && "(Soupeř)"}</h5>
      
      <div className="mb-3">
        <small>Životy: {data.health}%</small>
        <ProgressBar 
          variant="danger" 
          now={data.health} 
          animated={data.health < 30} 
          style={{ height: '20px' }}
        />
      </div>

      <div>
        <small>Energie (Nabíjení): {data.energy.toFixed(0)}%</small>
        <ProgressBar 
          variant="warning" 
          now={data.energy} 
          label={`${data.energy.toFixed(0)}%`}
          style={{ height: '25px' }}
        />
      </div>
    </div>
  );
};