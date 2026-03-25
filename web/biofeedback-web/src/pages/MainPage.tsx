import { Card, Container, Row, Col, Button, Badge } from "react-bootstrap";
import { Activity, Bluetooth, PlayCircle, InfoCircle, Controller } from "react-bootstrap-icons";

export default function MainPage() {
    // Logika pro stav připojení
    const isConnected = false;

    return (
        <Container className="mt-4 pb-5">
            {/* Hero sekce s připojením */}
            <div className="p-4 mb-4 bg-light rounded-3 border shadow-sm">
                <Row className="align-items-center">
                    <Col md={8}>
                        <h1 className="display-5 fw-bold text-primary">Biofeedback Gaming</h1>
                        <p className="lead">
                            Ovládejte hry pomocí svého těla. Systém využívá <strong>metodu biofeedbacku</strong> 
                            k detekci vaší relaxace nebo aktivace v reálném čase.
                        </p>
                    </Col>
                    <Col md={4} className="text-center">
                        <div className={`p-3 rounded border ${isConnected ? 'bg-success-subtle' : 'bg-white'}`}>
                            <Bluetooth size={30} className={isConnected ? 'text-success' : 'text-muted'} />
                            <div className="fw-bold my-1">Senzor vodivosti</div>
                            <Badge bg={isConnected ? "success" : "danger"} className="mb-2">
                                {isConnected ? "Připojeno" : "Odpojeno"}
                            </Badge>
                            {!isConnected && (
                                <Button variant="primary" size="sm" className="d-block mx-auto">
                                    Vyhledat zařízení
                                </Button>
                            )}
                        </div>
                    </Col>
                </Row>
            </div>

            {/* Monitoring a Vysvětlení vedle sebe */}
            <Row className="mb-4">
                <Col lg={6} className="mb-3 mb-lg-0">
                    <Card className="h-100 shadow-sm">
                        <Card.Header className="bg-dark text-white d-flex align-items-center justify-content-between">
                            <span><Activity className="me-2" /> Aktuální biofeedback</span>
                            {isConnected && <Badge bg="primary">Live</Badge>}
                        </Card.Header>
                        <Card.Body className="text-center d-flex flex-column justify-content-center">
                            {/* Prostor pro tvůj graf (Recharts/Chart.js) */}
                            <div className="placeholder-glow mb-2">
                                <div className="placeholder col-12 rounded" style={{ height: "120px", background: "#f8f9fa" }}>
                                    <small className="text-muted d-block pt-5">Zde se zobrazí křivka biofeedbacku</small>
                                </div>
                            </div>
                        </Card.Body>
                    </Card>
                </Col>

                <Col lg={6}>
                    <Card className="h-100 border-info shadow-sm">
                        <Card.Header className="bg-info text-white">
                            <InfoCircle className="me-2" /> Jak to funguje?
                        </Card.Header>
                        <Card.Body>
                            <p>
                                <strong>Biofeedback</strong> je metoda, která vám umožní sledovat vaše tělesné funkce 
                                v reálném čase. Tato aplikace měří drobné změny ve vlhkosti vaší pokožky.
                            </p>
                            <ul className="small text-muted">
                                <li><strong>Vysoká hodnota:</strong> Značí stres, soustředění nebo vzrušení.</li>
                                <li><strong>Nízká hodnota:</strong> Značí uvolnění, klid a relaxaci.</li>
                            </ul>
                            <p className="small mb-0 text-primary fw-bold">
                                Cílem her je naučit se tyto stavy vědomě ovládat.
                            </p>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
}