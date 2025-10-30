import { Card, Button, Form, InputGroup } from "react-bootstrap";

export default function LudoChat() {
    const messages = [
        { id: 1, player: "Červený hráč", text: "Ahoj všichni!", time: "14:32" },
        { id: 2, player: "Modrý hráč", text: "Zdravím! 😊", time: "14:33" },
        { id: 3, player: "Vy", text: "Dobrá hra!", time: "14:34" }
    ];

    return (
        <Card>
            <Card.Header>
                <h5 className="mb-0">💬 Chat</h5>
            </Card.Header>
            
            <Card.Body className="d-flex flex-column p-0">
                {/* Zprávy */}
                <div className="flex-grow-1 p-3" style={{ 
                    overflowY: 'auto',
                    maxHeight: '280px'
                }}>
                    {messages.map((msg) => (
                        <div key={msg.id} className={`mb-2 ${msg.player === 'Vy' ? 'text-end' : ''}`}>
                            <div className={`d-inline-block p-2 rounded ${
                                msg.player === 'Vy' 
                                    ? 'bg-primary text-white' 
                                    : 'bg-light'
                            }`} style={{ maxWidth: '80%' }}>
                                <small className="fw-bold">{msg.player}</small>
                                <div>{msg.text}</div>
                                <small className="opacity-75">{msg.time}</small>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Input */}
                <div className="p-3 border-top">
                    <InputGroup>
                        <Form.Control
                            placeholder="Napište zprávu..."
                        />
                        <Button variant="primary">
                            Odeslat
                        </Button>
                    </InputGroup>
                </div>
            </Card.Body>
        </Card>
    );
}