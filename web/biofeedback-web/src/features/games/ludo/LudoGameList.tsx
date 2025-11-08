import { Col, Row, Button } from "react-bootstrap"
import { useState } from "react"
import CreateGameModal from "./components/CreateGameModal"
import GameRoomsList from "./components/GameRoomsList"

export default function LudoGameList() {
    const [showCreateModal, setShowCreateModal] = useState(false);

    return (
        <>
            <Row>
                <Col>
                    <h2>Seznam herních místností</h2>
                </Col>
                <Col>
                    <Button 
                        variant="primary" 
                        className="float-end"
                        onClick={() => setShowCreateModal(true)}
                    >
                        Vytvořit novou místnost
                    </Button>
                </Col>
            </Row>

            <GameRoomsList />
            
            <CreateGameModal 
                show={showCreateModal} 
                onHide={() => {
                    setShowCreateModal(false);
                    document.location.reload();
                }} 
            />
        </>
    );
}