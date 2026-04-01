export interface GameParticipant {
  energy: number;
  health: number;
  target: number;
  isReady: boolean;
  targetMin: number;
  targetMax: number;
  nextChangeIn: number;
}

export interface EnergyBattleState {
  isStarted: boolean;
  isFinished: boolean;
  winner: string | null;
  me: GameParticipant;
  opponent: {
    email: string;
    health: number;
    energy: number;
  } | null;
}