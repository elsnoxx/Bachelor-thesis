import { Row, Col, Card, Button } from "react-bootstrap";

export default function LudoTurn({ playerOnTurn }: { playerOnTurn: boolean }) {
    return (
        <Card className="mt-4">
            <Card.Body>
                {playerOnTurn ? (
                    <div className="text-center">
                        <h3 className="mb-3">Váš tah</h3>
                        <p className="text-muted mb-3">Klikněte na tlačítko pro hod kostkou</p>
                        <Button variant="primary" size="lg" className="px-4">
                            🎲 Hodit kostkou
                        </Button>
                    </div>
                ) : (
                    <div className="text-center">
                        <h3 className="mb-3">Čekáte na soupeře</h3>
                        <div className="alert alert-info">
                            <strong>Na tahu:</strong> Modrý hráč
                        </div>
                        <div className="spinner-border text-primary" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </div>
                    </div>
                )}
            </Card.Body>
        </Card>
    );
}