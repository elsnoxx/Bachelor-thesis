import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button, Container, Navbar, Badge } from 'react-bootstrap';
import { BoxArrowLeft, Controller, Bluetooth } from 'react-bootstrap-icons'; // Přidán Bluetooth icon
import { useBle } from '../../../services/BleProvider';
import { RoomService } from '../../../api/RoomService';

interface GameHeaderProps {
    gameName: string;
    userEmail: string | null;
}

export default function GameHeader({ gameName, userEmail }: GameHeaderProps) {
    const navigate = useNavigate();
    const { roomId } = useParams<{ roomId: string }>();
    const { isConnected, connect, error, batteryLevel, disconnect } = useBle();
    const handleLeave = async () => {
        if (!roomId) return;
        try {
            await RoomService.leaveRoom(roomId, userEmail ?? '');
            if(gameName == 'ballance') navigate('/games/balance');
            if(gameName == 'energybattle') navigate('/games/energybattle');
            if(gameName == 'balloon') navigate('/games/balloon'); 
        } catch (error) {
            console.error('Chyba při opouštění místnosti:', error);
        }
    };
    const handleDisconnectClick = () => {
        if (window.confirm('Opravdu odpojit senzor?')) {
            disconnect();
        }
    };

    return (
        <Navbar bg="dark" variant="dark" className="mb-4 shadow-sm rounded">
            <Container fluid>
                <Navbar.Brand>
                    <Controller className="me-2" />
                    {gameName} <small className="text-muted fs-6">| ID: {roomId?.slice(0,8)}...</small>
                </Navbar.Brand>

                <div className="d-flex align-items-center gap-2">
                    {/* Zobrazení chyby, pokud se nepodaří připojit */}
                    {error && <small className="text-danger me-2" style={{fontSize: '10px'}}>{error}</small>}

                    {!isConnected ? (
                        <Button 
                            variant="primary" 
                            size="sm" 
                            onClick={connect} 
                            className="d-flex align-items-center"
                        >
                            <Bluetooth className="me-1" /> Připojit senzor
                        </Button>
                    ) : (
                        <div className="d-flex align-items-center gap-2">
                            <Badge bg="success" className="p-2">BLE: ONLINE</Badge>
                            {batteryLevel !== null && (
                                <Badge bg="secondary" className="p-2">
                                    🔋 {batteryLevel}%
                                </Badge>
                            )}
                            <Button variant="outline-danger" size="sm" onClick={handleDisconnectClick}>
                                Odpojit
                            </Button>
                        </div>
                    )}

                    <div className="vr mx-2 text-white-50" style={{height: '20px'}}></div>

                    <Button variant="outline-danger" size="sm" onClick={handleLeave}>
                        <BoxArrowLeft className="me-1" /> Opustit hru
                    </Button>
                </div>
            </Container>
        </Navbar>
    );
}