import { useState } from 'react';
import { connectToBLE } from '../services/ble';

function ConnectButton() {
  const [gsrValue, setGsrValue] = useState<number | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleConnect = async () => {
    try {
      setError(null);
      const server = await connectToBLE((data) => {
        setGsrValue(data.gsr);
        console.log('📈 Nová GSR hodnota:', data.gsr);
      });
      
      if (server) {
        setIsConnected(true);
        console.log('✅ Úspěšně připojeno k BLE zařízení');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Chyba připojení');
      console.error('❌ Chyba připojení:', err);
    }
  };

  return (
    <div>
      <button onClick={handleConnect} disabled={isConnected}>
        {isConnected ? 'Připojeno' : 'Připojit GSR zařízení'}
      </button>
      
      {error && (
        <div style={{ color: 'red', marginTop: '10px' }}>
          Chyba: {error}
        </div>
      )}
      
      {isConnected && (
        <div style={{ marginTop: '10px' }}>
          <p><strong>Stav:</strong> Připojeno ✅</p>
          <p><strong>Aktuální GSR:</strong> {gsrValue !== null ? gsrValue : 'Čekám na data...'}</p>
        </div>
      )}
    </div>
  );
}

export default ConnectButton;