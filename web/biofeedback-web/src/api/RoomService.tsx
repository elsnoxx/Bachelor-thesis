import api from "./axiosInstance";

export type GameType = 'balloon' | 'balance' | 'energybattle';

/**
 * Request payload for creating a room.
 */
export interface CreateRoomRequest {
    userId: string;
    name: string;
    gameType: GameType;
    password?: string;
    maxPlayers: number;
}

/**
 * Representation of a game room returned by the API (normalized for the client).
 */
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
        // Map data (handle property casing differences from the C# API)
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

    // Join a room
    joinRoom: async (roomId: string, userEmail: string, password?: string) => {
        const response = await api.post(`/gamerooms/${roomId}/join`, {
            UserEmail: userEmail,
            Password: password ?? ''
        });
        return response.data; // Returns response body containing message/title
    },

    createRoom: async (data: CreateRoomRequest) => {
        // We use the shared axios instance which automatically:
        // 1. Adds the Bearer token to requests
        // 2. Handles 401 responses by attempting a refresh token flow
        const response = await api.post('/gamerooms', data);
        return response.data;
    },

    leaveRoom: async (roomId: string, userEmail: string) => {
        // Axios automatically adds `Authorization: Bearer <token>` via the interceptor
        const response = await api.post(`/gamerooms/${roomId}/leave`, { userEmail });
        return response.data;
    }
    
};