interface Player {
    email: string;
    altitude: number;
    progress: number;
}

export default function BalloonPlayground({ players }: { players: Player[] }) {
    return (
        <div style={{
            width: '100%',
            height: '600px',
            backgroundColor: '#87CEEB', // Nebeská modř
            position: 'relative',
            overflow: 'hidden',
            borderRadius: '15px',
            border: '4px solid #f0f0f0'
        }}>
            {/* Cílová čára */}
            <div style={{
                position: 'absolute',
                right: '50px',
                height: '100%',
                borderLeft: '5px dashed white',
                zIndex: 1
            }}>
                <span style={{ color: 'white', fontWeight: 'bold', padding: '10px' }}>CÍL</span>
            </div>

            {players.map((player, index) => (
                <div 
                    key={player.email}
                    style={{
                        position: 'absolute',
                        bottom: `${player.altitude}px`, // Výška dle GSR
                        left: `${player.progress}%`,    // Postup dle DistanceTraveled
                        transition: 'all 0.1s linear',  // Plynulý pohyb
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                        zIndex: 2
                    }}
                >
                    {/* Jednoduchý balón pomocí CSS */}
                    <div style={{
                        width: '40px',
                        height: '50px',
                        backgroundColor: index === 0 ? 'red' : 'blue',
                        borderRadius: '50% 50% 50% 50% / 40% 40% 60% 60%',
                        position: 'relative'
                    }}>
                        <div style={{
                            position: 'absolute',
                            bottom: '-15px',
                            left: '18px',
                            width: '2px',
                            height: '15px',
                            backgroundColor: '#666'
                        }} />
                        <div style={{
                            position: 'absolute',
                            bottom: '-25px',
                            left: '12px',
                            width: '15px',
                            height: '10px',
                            backgroundColor: '#8B4513'
                        }} />
                    </div>
                    
                    <span style={{ 
                        fontSize: '10px', 
                        background: 'rgba(255,255,255,0.7)',
                        padding: '2px 5px',
                        borderRadius: '5px',
                        marginTop: '30px'
                    }}>
                        {player.email.split('@')[0]}
                    </span>
                </div>
            ))}

            {/* Dekorace: Mraky */}
            <div className="cloud" style={{ top: '50px', left: '10%' }}>☁️</div>
            <div className="cloud" style={{ top: '150px', left: '60%' }}>☁️</div>
        </div>
    );
}