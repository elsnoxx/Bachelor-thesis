import { Col, Row } from "react-bootstrap"


export default function LudoGameList() {
    return (
        <>
            <Row>
                <Col>
                    <h1>Ludo Rooms</h1>
                </Col>
                <Col>
                    <a href="/games/ludo/create" className="btn btn-primary float-end">
                        Vytvořit novou místnost
                    </a>
                </Col>
            </Row>
        </>
    );
}