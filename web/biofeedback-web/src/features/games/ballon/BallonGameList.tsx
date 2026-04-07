import { Col, Row, Button } from "react-bootstrap"
import { useState } from "react"
import CreateGameModal from "../general/CreateGameModal";
import GameRoomsTable from "../general/GameRoomsTable"

export default function BallonGameList() {
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

            <GameRoomsTable gameType="ballon" redirectPath="/ballon/game" />
            
            <CreateGameModal 
                show={showCreateModal} 
                gameType="ballon"
                onHide={() => {
                    setShowCreateModal(false);
                    document.location.reload();
                }} 
            />
        </>
    );
}