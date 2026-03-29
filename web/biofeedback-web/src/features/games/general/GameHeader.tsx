import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button, Container, Navbar, Badge } from 'react-bootstrap';
import { BoxArrowLeft, Controller } from 'react-bootstrap-icons';
import { useBle } from '../../../services/BleProvider';

interface GameHeaderProps {
    gameName: string;
    userEmail: string; // ID aktuálního uživatele pro API request
}

export default function GameHeader({ gameName, userEmail }: GameHeaderProps) {
    const navigate = useNavigate();
    const { roomId } = useParams<{ roomId: string }>();
    const { isConnected } = useBle();

    const handleLeave = async () => {
        if (!roomId) return;

        try {
            const token = localStorage.getItem('token');
            const apiUrl = import.meta.env.VITE_API_URL;
            console.log(userEmail);
            const response = await fetch(`${apiUrl}/gamerooms/${roomId}/leave`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ userEmail: userEmail })
                          
            });

            if (response.ok) {
                if (gameName == 'energybattle')
                    navigate('/hry'); 
            } else {
                console.error("Chyba při opouštění místnosti");
            }
        } catch (error) {
            console.error("Network error při opouštění místnosti:", error);
        }
    };

    return (
        <Navbar bg="dark" variant="dark" className="mb-4 shadow-sm rounded">
            <Container fluid>
                <Navbar.Brand>
                    <Controller className="me-2" />
                    {gameName} <small className="text-muted fs-6">| ID: {roomId?.slice(0,8)}...</small>
                </Navbar.Brand>

                <div className="d-flex align-items-center gap-3">
                    {/* Indikátor Bluetooth - užitečné vidět i během hry */}
                    <Badge bg={isConnected ? "success" : "danger"}>
                        BLE: {isConnected ? "ONLINE" : "OFFLINE"}
                    </Badge>

                    <Button variant="outline-danger" size="sm" onClick={handleLeave}>
                        <BoxArrowLeft className="me-1" /> Opustit hru
                    </Button>
                </div>
            </Container>
        </Navbar>
    );
}