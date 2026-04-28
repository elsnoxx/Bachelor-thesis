export interface BalloonPlayer {
    email: string;
    altitude: number;
    distance: number;
    progress: number;
}

export interface BalloonGameState {
    roomId: string;
    isStarted: boolean;
    endReason: string | null;
    isFinished: boolean;
    winner: string | null;
    players: BalloonPlayer[];
}