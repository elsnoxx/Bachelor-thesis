import React from "react";
import { Container, Row, Col } from "react-bootstrap";


function getCurrentYear() {
    return new Date().getFullYear();
}


export default function AppsFooter() {
    return (
        <footer className="bg-light py-3 mt-auto border-top">
            <Container>
                <Row>
                    <Col className="text-center">
                        <p className="text-muted mb-0">
                            © {getCurrentYear()} Biofeedback App - Bakalářská práce Rihard Ficek
                        </p>
                    </Col>
                </Row>
            </Container>
        </footer>
    );
}
