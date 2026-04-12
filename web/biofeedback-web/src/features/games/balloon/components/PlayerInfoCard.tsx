import React from 'react';
import { Col } from 'react-bootstrap';
import type { BalloonPlayer } from '../balloonsTypes';

interface PlayerCardProps {
    player: BalloonPlayer | undefined;
    playerNumber: number;
    isMe: boolean;
    align: 'start' | 'end';
}

export const PlayerInfoCard: React.FC<PlayerCardProps> = ({ player, playerNumber, isMe, align }) => {
    const textAlign = align === 'start' ? 'text-start' : 'text-end';
    const borderClass = align === 'start' ? 'border-end' : 'border-start';

    return (
        <Col md={2} className={`${borderClass} border-secondary ${textAlign}`}>
            <h5 style={{ color: isMe ? '#ffc107' : 'white' }}>
                {isMe ? "Ty" : `Hráč ${playerNumber}`}
            </h5>
            <div className="small text-truncate" title={player?.email}>
                {player?.email || 'Čekání na hráče...'}
            </div>
            <hr className="my-2" />
            <div className="text-info font-monospace" style={{ fontSize: '0.9rem' }}>
                Výška: {player?.altitude?.toFixed(0) || 0}m
            </div>
            <div className="text-white-50 small">
                Pokrok: {player?.progress?.toFixed(0) || 0}%
            </div>
        </Col>
    );
};