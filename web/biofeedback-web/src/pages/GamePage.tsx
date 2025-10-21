import React from "react";
import { Card, Container, Row, Col } from "react-bootstrap";
// import './GamePage.css';

const games = [
    { name: 'Člověče nezlob se', href: '/ludo', description: 'Klasická desková hra pro celou rodinu.' },
    { name: 'Balancuj', href: '/balance', description: 'Zábavná hra na udržení rovnováhy.' },
    { name: 'Bitva', href: '/battle', description: 'Strategická hra pro dva hráče.' }
];

export default function HryPage() {
    return (
        <Container className="mt-4">
            <h2 className="mb-4">Výběr hry</h2>
            <Row>
                {games.map((game) => (
                    <Col md={4} key={game.name} className="mb-3">
                        <Card className="h-100">
                            <Card.Img 
                                variant="top" 
                                src={`/${game.name.replace(/\s+/g, '').toLowerCase()}.png`}
                                style={{ height: '200px', objectFit: 'cover' }}
                            />
                            <Card.Body className="d-flex flex-column">
                                <Card.Title>{game.name}</Card.Title>
                                <Card.Text className="flex-grow-1">
                                    {game.description}
                                </Card.Text>
                                <a href={game.href} className="btn btn-primary mt-auto">
                                    Hrát {game.name}
                                </a>
                            </Card.Body>
                        </Card>
                    </Col>
                ))}
            </Row>
        </Container>
    )
}