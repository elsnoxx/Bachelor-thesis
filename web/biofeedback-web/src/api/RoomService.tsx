import api from "./axiosInstance";

export type GameType = 'ludo' | 'ballance' | 'energybattle';

export interface CreateRoomRequest {
    userId: string;
    name: string;
    gameType: GameType;
    password?: string;
    maxPlayers: number;
}

export interface GameRoom {
    id: string;
    name: string;
    gameType: string;
    password: string | null;
    maxPlayers: number;
    currentPlayers: number;
}

export const RoomService = {
    getGameRooms: async (gameType: string): Promise<GameRoom[]> => {
        const response = await api.get('/gamerooms', { params: { gameType } });
        // Mapování dat (ošetření velkých/malých písmen z C# API)
        const rooms = (response.data.data || []).map((r: any) => ({
            id: r.id,
            name: r.name,
            gameType: r.gameType,
            password: r.password ?? null,
            maxPlayers: r.maxPlayers ?? r.MaxPlayers ?? 0,
            currentPlayers: r.currentPlayers ?? r.CurrentPlayers ?? 0
        }));
        return rooms;
    },

    // Připojení do místnosti
    joinRoom: async (roomId: string, userEmail: string, password?: string) => {
        const response = await api.post(`/gamerooms/${roomId}/join`, {
            UserEmail: userEmail,
            Password: password ?? ''
        });
        return response.data; // Vrací body s message/title
    },

    createRoom: async (data: CreateRoomRequest) => {
        // Používáme tvoji axios instanci, která automaticky:
        // 1. Přidá Bearer Token
        // 2. Vyřeší 401 a Refresh Token
        const response = await api.post('/gamerooms', data);
        return response.data;
    },

    leaveRoom: async (roomId: string, userEmail: string) => {
        // Axios automaticky přidá Authorization: Bearer <token> díky interceptoru
        const response = await api.post(`/gamerooms/${roomId}/leave`, { userEmail });
        return response.data;
    }
    
};