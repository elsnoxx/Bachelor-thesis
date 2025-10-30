import LudoBoard from "./components/LudoBoard";
import { Container, Row, Col } from "react-bootstrap";
import LudoTurn from "./components/LudoTurn";
import LudoChat from "./components/LudoChat";

const playerTurn = true;

export default function LudoGame() {
  return (
    <Container fluid className="py-4">
      <Row className="align-items-start">
        {/* Herní deska */}
        <Col lg={8} md={12}>
          <div className="d-flex justify-content-center">
            <div className="border p-2 p-md-4" style={{
              maxWidth: '100%',
              overflow: 'auto',
              display: 'inline-block'
            }}>
              <LudoBoard />
            </div>
          </div>
        </Col>
        
        {/* Boční panel */}
        <Col lg={4} md={12}>
          <div className="d-flex flex-column gap-3">
            <LudoTurn playerOnTurn={playerTurn} />
            <LudoChat />
          </div>
        </Col>
      </Row>
    </Container>
  );
}