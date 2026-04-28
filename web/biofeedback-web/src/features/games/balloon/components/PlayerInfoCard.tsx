import React from 'react';
import { Col, Card, ProgressBar, Badge } from 'react-bootstrap';
import type { BalloonPlayer } from '../balloonsTypes';

interface PlayerCardProps {
    player: BalloonPlayer | undefined;
    playerNumber: number;
    isMe: boolean;
}

export const PlayerInfoCard: React.FC<PlayerCardProps> = ({ player, playerNumber, isMe }) => {
    const isWaiting = !player;

    return (
        <Col md={12} className="mb-3 px-1">
            <Card 
                bg={isMe ? 'dark' : 'secondary'} 
                text="white" 
                className={`shadow-sm ${isMe ? 'border-warning' : 'border-light'}`}
                style={{ borderWidth: isMe ? '2px' : '1px', opacity: isWaiting ? 0.7 : 1, minWidth: '160px' }}
            >
                <Card.Header className="py-2 d-flex justify-content-between align-items-center">
                    <span className="fw-bold small">
                        {isMe ? "🚀 Ty" : `👤 Hráč ${playerNumber}`}
                    </span>
                    {isMe && <Badge bg="warning" text="dark" style={{fontSize: '0.6rem'}}>MÉ STATY</Badge>}
                </Card.Header>

                <Card.Body className="p-2">
                    <div className="mb-2">
                        <div className="text-white-50" style={{fontSize: '0.7rem'}}>Aktuální výška:</div>
                        <div className="text-info fw-bold font-monospace">
                            {player?.altitude?.toFixed(0) || 0} m
                        </div>
                    </div>

                    <div>
                        <div className="d-flex justify-content-between text-white-50" style={{fontSize: '0.7rem'}}>
                            <span>Pokrok:</span>
                            <span>{player?.progress?.toFixed(0) || 0}%</span>
                        </div>
                        <ProgressBar 
                            variant={isMe ? "warning" : "info"} 
                            now={player?.progress || 0} 
                            style={{ height: '6px' }}
                        />
                    </div>
                </Card.Body>
            </Card>
        </Col>
    );
};