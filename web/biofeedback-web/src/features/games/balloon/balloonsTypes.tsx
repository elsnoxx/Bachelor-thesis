export interface BalloonPlayer {
    email: string;
    altitude: number;  // Y osa (výška)
    distance: number;  // X osa (vzdálenost)
    progress: number;  // 0-100%
}

export interface BalloonGameState {
    roomId: string;
    isStarted: boolean;
    endReason: string | null;
    isFinished: boolean;
    winner: string | null;
    players: BalloonPlayer[];
}