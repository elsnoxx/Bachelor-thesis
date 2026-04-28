import React, { useState, useEffect } from "react"; // Chybělo useState a useEffect
import { Card, Container, Row, Col, Button, Badge, ListGroup } from "react-bootstrap";
import { Activity, Bluetooth, InfoCircle } from "react-bootstrap-icons";
import * as signalR from "@microsoft/signalr"; // Musíš mít nainstalováno: npm install @microsoft/signalr
import { useBle } from "../services/BleProvider";

export default function MainPage() {
    const { isConnected, gsrValue, batteryLevel, error, connect, disconnect } = useBle();

    const [testHub, setTestHub] = useState(null);
    const [latency, setLatency] = useState(null);
    const [lastEcho, setLastEcho] = useState(null);


    useEffect(() => {
        if (isConnected) {
            // Získání tokenu z localStorage
            const token = localStorage.getItem("token"); 

            const connection = new signalR.HubConnectionBuilder()
                .withUrl(`${import.meta.env.VITE_API_URL}/testhub`, {
                    accessTokenFactory: () => token 
                })
                .withAutomaticReconnect()
                .build();

            connection.on("PongSensorData", (data) => {
                const now = new Date();
                const sentTime = new Date(data.serverTimestamp);
                setLatency(now - sentTime); 
                setLastEcho(data.receivedValue);
            });

            connection.start()
                .then(() => {
                    console.log("Connected to TestHub");
                    setTestHub(connection);
                })
                .catch(err => console.error("TestHub Connection Error: ", err));

            return () => {
                if (connection) {
                    connection.stop();
                }
            };
        } else {
            setTestHub(null);
            setLatency(null);
            setLastEcho(null);
        }
    }, [isConnected]);

    useEffect(() => {
        if (testHub && isConnected && gsrValue !== null) {
            testHub.invoke("PingSensorData", gsrValue)
                .catch(err => console.error("Error invoking PingSensorData:", err));
        }
    }, [gsrValue, testHub, isConnected]);

    return (
        <Container className="mt-4 pb-5">
            {error && <div className="alert alert-danger">{error}</div>}

            {/* Header sekce */}
            <div className="p-4 mb-4 bg-light rounded-3 border shadow-sm">
                <Row className="align-items-center">
                    <Col md={8}>
                        <h1 className="display-5 fw-bold text-primary">Biofeedback Gaming</h1>
                        <p className="lead">Ovládejte hry pomocí svého těla.</p>
                    </Col>
                    <Col md={4} className="text-center">
                        <div className={`p-3 rounded border ${isConnected ? 'bg-success-subtle' : 'bg-white'}`}>
                            <Bluetooth size={30} className={isConnected ? 'text-success' : 'text-muted'} />
                            <div className="fw-bold my-1">Senzor vodivosti</div>
                            <Badge bg={isConnected ? "success" : "danger"} className="mb-2">
                                {isConnected ? "Připojeno" : "Odpojeno"}
                            </Badge>

                            {!isConnected ? (
                                <Button variant="primary" size="sm" className="d-block mx-auto" onClick={connect}>
                                    Vyhledat zařízení
                                </Button>
                            ) : (
                                <Button variant="outline-danger" size="sm" className="d-block mx-auto" onClick={disconnect}>
                                    Odpojit
                                </Button>
                            )}
                        </div>
                    </Col>
                </Row>
            </div>

            <Row className="mb-4">
                {/* Karta 1: Hodnota ze senzoru */}
                <Col lg={4} md={6} className="mb-3">
                    <Card className="h-100 shadow-sm text-center">
                        <Card.Header className="bg-dark text-white">
                            <Activity className="me-2" /> Aktuální hodnota (BLE)
                        </Card.Header>
                        <Card.Body className="d-flex flex-column justify-content-center">
                            {isConnected ? (
                                <>
                                    {batteryLevel !== null && (
                                        <Badge bg="secondary" className="p-2 mb-2 w-50 mx-auto">
                                            🔋 {batteryLevel}%
                                        </Badge>
                                    )}
                                    <div>
                                        <h2 className="display-4 fw-bold text-primary">{gsrValue ?? "--"}</h2>
                                        <span className="text-muted">μS (GSR)</span>
                                    </div>
                                </>
                            ) : (
                                <small className="text-muted">Připojte senzor pro zobrazení dat</small>
                            )}
                        </Card.Body>
                    </Card>
                </Col>

                {/* Karta 2: Diagnostika WebSocketu (Echo test) */}
                <Col lg={4} md={6} className="mb-3">
                    <Card className="h-100 shadow-sm border-warning">
                        <Card.Header className="bg-warning text-dark fw-bold">
                            📶 Diagnostika Serveru
                        </Card.Header>
                        <Card.Body>
                            {isConnected && testHub ? (
                                <ListGroup variant="flush" className="small">
                                    <ListGroup.Item className="d-flex justify-content-between align-items-center">
                                        Stav spojení: <Badge bg="success">Aktivní</Badge>
                                    </ListGroup.Item>
                                    <ListGroup.Item className="d-flex justify-content-between align-items-center">
                                        Server Echo: <strong>{lastEcho ?? "--"}</strong>
                                    </ListGroup.Item>
                                    <ListGroup.Item className="d-flex justify-content-between align-items-center">
                                        Latence (RTT): <span className="text-primary fw-bold">{latency !== null ? `${latency} ms` : "--"}</span>
                                    </ListGroup.Item>
                                </ListGroup>
                            ) : (
                                <div className="text-center text-muted py-4">
                                    {isConnected ? "Navazování spojení se serverem..." : "Čekání na BLE senzor..."}
                                </div>
                            )}
                        </Card.Body>
                        <Card.Footer className="text-muted small">
                            Validace datové cesty přes TestHub
                        </Card.Footer>
                    </Card>
                </Col>

                {/* Karta 3: Vysvětlivky */}
                <Col lg={4} md={12} className="mb-3">
                    <Card className="h-100 border-info shadow-sm">
                        <Card.Header className="bg-info text-white fw-bold">
                            <InfoCircle className="me-2" /> Interpretace dat
                        </Card.Header>
                        <Card.Body>
                             <p className="small text-muted">Měříme elektrickou vodivost kůže (GSR), která reaguje na aktivitu potních žláz ovládaných autonomním nervovým systémem.</p>
                             <ul className="small fw-bold">
                                <li className="text-danger">Vysoká / Rostoucí: Stres, vzrušení</li>
                                <li className="text-success">Nízká / Klesající: Relaxace, klid</li>
                             </ul>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
}