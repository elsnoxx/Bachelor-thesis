import { Card, Button, Form, InputGroup } from "react-bootstrap";

export default function LudoChat() {
  const messages = [
    { id: 1, player: "Červený", text: "Ahoj všichni!", time: "14:32", color: "#ff4d4d" },
    { id: 2, player: "Modrý", text: "Zdravím! 😊", time: "14:33", color: "#4d94ff" },
    { id: 3, player: "Vy", text: "Dobrá hra!", time: "14:34", isMe: true }
  ];

  return (
    <div className="d-flex flex-column h-100">
      <div className="flex-grow-1 p-3" style={{ overflowY: 'auto', maxHeight: '400px', background: '#f8f9fa' }}>
        {messages.map((msg) => (
          <div key={msg.id} className={`d-flex flex-column mb-3 ${msg.isMe ? 'align-items-end' : 'align-items-start'}`}>
            {!msg.isMe && <small className="fw-bold mb-1 px-2" style={{ color: msg.color }}>{msg.player}</small>}
            <div 
              className={`px-3 py-2 shadow-sm ${msg.isMe ? 'bg-primary text-white' : 'bg-white text-dark'}`}
              style={{ 
                borderRadius: msg.isMe ? '15px 15px 2px 15px' : '15px 15px 15px 2px',
                maxWidth: '85%',
                fontSize: '0.95rem'
              }}
            >
              {msg.text}
            </div>
            <small className="text-muted mt-1" style={{ fontSize: '0.7rem' }}>{msg.time}</small>
          </div>
        ))}
      </div>

      <div className="p-2 bg-white border-top">
        <InputGroup>
          <Form.Control
            placeholder="Napište zprávu..."
            className="border-0 bg-light"
            style={{ borderRadius: '20px 0 0 20px', paddingLeft: '15px' }}
          />
          <Button variant="primary" style={{ borderRadius: '0 20px 20px 0' }}>
            ➤
          </Button>
        </InputGroup>
      </div>
    </div>
  );
}