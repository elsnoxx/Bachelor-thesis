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
    // Přidána funkce connect z našeho BleProvideru
    const { isConnected, connect, error } = useBle();
    const handleLeave = async () => {
        if (!roomId) return;
        try {
            await RoomService.leaveRoom(roomId, userEmail ?? '');
            if(gameName == 'ballance') navigate('/games/balance');
            if(gameName == 'energybattle') navigate('/games/energybattle');
            if(gameName == 'ballon') navigate('/games/ballon'); 
        } catch (error) {
            console.error('Chyba při opouštění místnosti:', error);
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

                    {/* DYNAMICKÉ TLAČÍTKO / BADGE */}
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
                        <Badge bg="success" className="p-2">
                            BLE: ONLINE
                        </Badge>
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