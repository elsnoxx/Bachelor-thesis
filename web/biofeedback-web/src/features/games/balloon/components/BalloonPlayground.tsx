import type { BalloonPlayer } from "../balloonsTypes";

interface Props {
    players: BalloonPlayer[];
}

export default function BalloonPlayground({ players }: Props) {
    return (
        <div style={{
            position: 'relative',
            height: '500px',
            background: 'linear-gradient(to top, #87CEEB, #E0F7FA)', // Obloha
            borderRadius: '15px',
            overflow: 'hidden',
            border: '2px solid #555'
        }}>
            {/* Cílová čára */}
            <div style={{
                position: 'absolute',
                right: '20px',
                top: 0,
                bottom: 0,
                width: '10px',
                borderLeft: '5px dashed white',
                zIndex: 1
            }}>
                <span style={{ writingMode: 'vertical-rl', color: 'white', marginTop: '10px' }}>FINISH</span>
            </div>

            {players.map((p, index) => (
                <div
                    key={p.email}
                    style={{
                        position: 'absolute',
                        // X osa: Progress 0-100%
                        left: `${p.progress}%`,
                        // Y osa: Mapujeme výšku senzoru (0-1000) na výšku plochy (0-400px)
                        bottom: `${(p.altitude / 1000) * 400}px`,
                        transition: 'all 0.5s ease-out',
                        fontSize: '40px',
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                        zIndex: 2
                    }}
                >
                    <div style={{ fontSize: '12px', background: 'rgba(0,0,0,0.5)', padding: '2px 5px', borderRadius: '5px' }}>
                        {p.email.split('@')[0]}
                    </div>
                    {/* Emoji balónu, barva podle indexu */}
                    <span>{index === 0 ? '🎈' : '🏮'}</span>
                </div>
            ))}

            {/* Mraky jako dekorace */}
            <div style={{ position: 'absolute', top: '20%', left: '10%', opacity: 0.5, fontSize: '30px' }}>☁️</div>
            <div style={{ position: 'absolute', top: '50%', left: '40%', opacity: 0.3, fontSize: '40px' }}>☁️</div>
            <div style={{ position: 'absolute', top: '15%', left: '70%', opacity: 0.4, fontSize: '30px' }}>☁️</div>
        </div>
    );
}