import React, { useState, useEffect, useCallback } from 'react';
import { Table, Alert, Spinner, Button, Badge } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import PasswordModal from './PasswordModal';
import { RoomService } from '../../../api/RoomService';

interface GameRoom {
    id: string;
    name: string;
    gameType: string;
    password: string | null;
    maxPlayers: number;
    currentPlayers: number;
}

interface GameRoomsTableProps {
    gameType: 'energybattle' | 'ballance' | 'ballon';
    redirectPath: string;
}

export default function GameRoomsTable({ gameType, redirectPath }: GameRoomsTableProps) {
    const [gameRooms, setGameRooms] = useState<GameRoom[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [joining, setJoining] = useState(false);
    const [showPasswordModal, setShowPasswordModal] = useState(false);
    const [selectedRoom, setSelectedRoom] = useState<{ id: string, name: string } | null>(null);
    const [apiMessage, setApiMessage] = useState<string | null>(null);

    const navigate = useNavigate();

    // Pomocná funkce pro získání emailu (můžeš ji vyhodit do utility.ts)
    const getUserEmail = (): string | null => {
        const userJson = localStorage.getItem('user');
        if (userJson) try { return JSON.parse(userJson).email || null; } catch { }
        const token = localStorage.getItem('token');
        if (token) try { return JSON.parse(atob(token.split('.')[1])).email || null; } catch { }
        return null;
    };

    const fetchGameRooms = useCallback(async () => {
        try {
            setLoading(true);
            const rooms = await RoomService.getGameRooms(gameType);
            setGameRooms(rooms);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Chyba při načítání');
        } finally {
            setLoading(false);
        }
    }, [gameType]);

    useEffect(() => { fetchGameRooms(); }, [fetchGameRooms]);

    const executeJoin = async (roomId: string, password: string | null, roomName: string) => {
        const userEmail = getUserEmail();
        if (!userEmail) return alert('Musíš být přihlášen.');
        setJoining(true);
        try {
            const res = await RoomService.joinRoom(roomId, userEmail, password ?? '');
            setApiMessage(res?.message ?? 'Úspěšně připojeno.');
            sessionStorage.setItem(`roomName_${roomId}`, roomName);
            navigate(`${redirectPath}/${roomId}`, { state: { roomName } });
        } catch (err) {
            alert(err instanceof Error ? err.message : 'Chyba připojení');
            fetchGameRooms();
        } finally {
            setJoining(false);
            setShowPasswordModal(false);
        }
    };

    if (loading) return <div className="text-center my-4"><Spinner animation="border" /><p>Načítání...</p></div>;
    if (error) return <Alert variant="danger">Chyba: {error} <Button onClick={fetchGameRooms} size="sm">Zkusit znovu</Button></Alert>;
    if (gameRooms.length === 0) return <Alert variant="info">Žádné volné místnosti pro {gameType}.</Alert>;

    return (
        <div className="my-4">
            <div className="d-flex justify-content-end mb-2">
                <Button variant="outline-primary" size="sm" onClick={fetchGameRooms}>Obnovit</Button>
            </div>
            <Table striped bordered hover responsive>
                <thead className="table-dark">
                    <tr>
                        <th>Název místnosti</th>
                        <th>Hráči</th>
                        <th>Status</th>
                        <th>Akce</th>
                    </tr>
                </thead>
                <tbody>
                    {gameRooms.map((room) => {
                        const isFull = room.currentPlayers >= room.maxPlayers;
                        return (
                            <tr key={room.id}>
                                <td><strong>{room.name}</strong></td>
                                <td>{room.currentPlayers}/{room.maxPlayers}</td>
                                <td>
                                    <Badge bg={room.password ? "warning" : "success"} className="me-1">
                                        {room.password ? "🔒 Heslo" : "🔓 Veřejná"}
                                    </Badge>
                                    {isFull && <Badge bg="danger">Plno</Badge>}
                                </td>
                                <td>
                                    <Button
                                        variant="success"
                                        size="sm"
                                        disabled={isFull || joining}
                                        onClick={() => {
                                            if (room.password) {
                                                setSelectedRoom({ id: room.id, name: room.name });
                                                setShowPasswordModal(true);
                                            } else {
                                                executeJoin(room.id, null, room.name);
                                            }
                                        }}
                                    >
                                        Připojit se
                                    </Button>
                                </td>
                            </tr>
                        );
                    })}
                </tbody>
            </Table>

            <PasswordModal
                show={showPasswordModal}
                onHide={() => setShowPasswordModal(false)}
                roomName={selectedRoom?.name}
                submitting={joining}
                onSubmit={(pass) => selectedRoom && executeJoin(selectedRoom.id, pass, selectedRoom.name)}
            />
            {apiMessage && <Alert variant="info">{apiMessage}</Alert>}
        </div>
    );
}