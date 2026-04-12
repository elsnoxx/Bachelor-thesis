import { Card, Container, Row, Col } from "react-bootstrap";

const games = [
    { 
        name: 'Ovládání balónu', 
        href: '/games/balloon', 
        description: 'Udržuj klid a ovládej výšku svého balónu pomocí stresu. Doletíš do cíle jako první?', 
        image: "/img/balloon.avif"
    },
    { 
        name: 'Balancuj', 
        href: '/games/balance', 
        description: 'Zábavná hra na udržení rovnováhy.', 
        image: "/img/ballance.webp" 
    },
    { 
        name: 'Bitva', 
        href: '/games/energybattle', 
        description: 'Strategická hra pro dva hráče.', 
        image: "/img/energybattle.jfif" 
    }
];
    
export default function HryPage() {   
    return (
        <Container className="mt-4">
            <h2 className="mb-4">Výběr hry</h2>
            <Row>
                {games.map((game) => (
                    <Col md={4} key={game.name} className="mb-3">
                        <Card className="h-100 shadow-sm">
                            <Card.Img variant="top" src={game.image} className="gameCardImg" />
                            <Card.Body className="d-flex flex-column">
                                <Card.Title>{game.name}</Card.Title>
                                <Card.Text className="flex-grow-1">
                                    {game.description}
                                </Card.Text>
                                {/* Doporučuji použít Link z react-router-dom místo <a>, pokud chceš plynulý přechod bez reloadu */}
                                <a href={game.href} className="btn btn-primary mt-auto">
                                    Hrát
                                </a>
                            </Card.Body>
                        </Card>
                    </Col>
                ))}
            </Row>
        </Container>
    )
}