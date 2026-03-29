import React from "react";
import { Card, Container, Row, Col, Button, Badge } from "react-bootstrap";
import { Activity, Bluetooth, InfoCircle } from "react-bootstrap-icons";
// Importujeme náš nový hook
import { useBle } from "../services/BleProvider"; 

export default function MainPage() {
    // Vytáhneme vše potřebné z kontextu
    const { isConnected, gsrValue, error, connect, disconnect } = useBle();

    return (
        <Container className="mt-4 pb-5">
            {/* Zobrazení případné chyby (např. nepodporovaný prohlížeč) */}
            {error && <div className="alert alert-danger">{error}</div>}

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
                <Col lg={6}>
                    <Card className="h-100 shadow-sm text-center">
                        <Card.Header className="bg-dark text-white">
                            <Activity className="me-2" /> Aktuální hodnota
                        </Card.Header>
                        <Card.Body className="d-flex flex-column justify-content-center">
                            {isConnected ? (
                                <div>
                                    <h2 className="display-4 fw-bold text-primary">{gsrValue ?? "--"}</h2>
                                    <span className="text-muted">μS (GSR)</span>
                                </div>
                            ) : (
                                <small className="text-muted">Připojte senzor pro zobrazení dat</small>
                            )}
                        </Card.Body>
                    </Card>
                </Col>
                
                <Col lg={6}>
                    <Card className="h-100 border-info shadow-sm">
                        <Card.Header className="bg-info text-white">
                            <InfoCircle className="me-2" /> Jak to funguje?
                        </Card.Header>
                        <Card.Body>
                             <p>Měříme elektrickou vodivost kůže. </p>
                             <ul className="small">
                                <li><strong>Vysoká hodnota:</strong> Stres / Aktivace</li>
                                <li><strong>Nízká hodnota:</strong> Klid / Relaxace</li>
                             </ul>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
}