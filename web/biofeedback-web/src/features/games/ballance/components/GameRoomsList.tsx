import React, { useState, useEffect } from 'react';
import { Table, Alert, Spinner, Button, Badge } from 'react-bootstrap';

interface GameRoom {
    id: string;
    name: string;
    gameType: string;
    password: string | null;
    maxPlayers: number;
}

interface ApiResponse {
    error: any;
    data: GameRoom[];
}

export default function GameRoomsList() {
    const [gameRooms, setGameRooms] = useState<GameRoom[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchGameRooms = async () => {
        try {
            setLoading(true);
            setError(null);
            
            const token = localStorage.getItem('token');
            const apiUrl = import.meta.env.VITE_API_URL;
            const response = await fetch(`${apiUrl}/gamerooms?gameType=ballance`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result: ApiResponse = await response.json();
            
            if (result.error) {
                throw new Error(result.error);
            }

            setGameRooms(result.data || []);
        } catch (err) {
            console.error('Chyba při načítání herních místností:', err);
            setError(err instanceof Error ? err.message : 'Nastala neočekávaná chyba');
        } finally {
            setLoading(false);
        }
    };

    const handleJoinRoom = (roomId: string) => {
        // Zde by byla logika pro připojení do místnosti
        console.log('Připojování do místnosti:', roomId);
        // Například redirect na herní obrazovku
        // navigate(`/ludo/game/${roomId}`);
    };

    useEffect(() => {
        fetchGameRooms();
    }, []);

    if (loading) {
        return (
            <div className="text-center my-4">
                <Spinner animation="border" role="status">
                    <span className="visually-hidden">Načítání...</span>
                </Spinner>
                <p className="mt-2">Načítání herních místností...</p>
            </div>
        );
    }

    if (error) {
        return (
            <Alert variant="danger" className="my-4">
                <Alert.Heading>Chyba při načítání</Alert.Heading>
                <p>{error}</p>
                <Button variant="outline-danger" onClick={fetchGameRooms}>
                    Zkusit znovu
                </Button>
            </Alert>
        );
    }

    if (gameRooms.length === 0) {
        return (
            <Alert variant="info" className="my-4">
                <Alert.Heading>Žádné herní místnosti</Alert.Heading>
                <p>Momentálně nejsou k dispozici žádné herní místnosti. Vytvořte novou!</p>
            </Alert>
        );
    }

    return (
        <div className="my-4">
            <div className="d-flex justify-content-between align-items-center mb-3">
                <Button variant="outline-primary" size="sm" onClick={fetchGameRooms}>
                    Obnovit
                </Button>
            </div>
            
            <Table striped bordered hover responsive>
                <thead className="table-dark">
                    <tr>
                        <th>Název místnosti</th>
                        <th>Typ hry</th>
                        <th>Hráči</th>
                        <th>Ochrana heslem</th>
                        <th>Akce</th>
                    </tr>
                </thead>
                <tbody>
                    {gameRooms.map((room) => (
                        <tr key={room.id}>
                            <td>
                                <strong>{room.name}</strong>
                            </td>
                            <td>
                                <Badge bg="primary" className="text-uppercase">
                                    {room.gameType}
                                </Badge>
                            </td>
                            <td>
                                <span className="text-muted">
                                    0/{room.maxPlayers}
                                </span>
                            </td>
                            <td>
                                {room.password ? (
                                    <Badge bg="warning">
                                        🔒 Chráněno heslem
                                    </Badge>
                                ) : (
                                    <Badge bg="success">
                                        🔓 Volný přístup
                                    </Badge>
                                )}
                            </td>
                            <td>
                                <Button
                                    variant="success"
                                    size="sm"
                                    onClick={() => handleJoinRoom(room.id)}
                                >
                                    Připojit se
                                </Button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
}