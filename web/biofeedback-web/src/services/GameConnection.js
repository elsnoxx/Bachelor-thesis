import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export async function startConnection(playerId, onBioDataReceived) {
  const connection = new HubConnectionBuilder()
    .withUrl('/gamehub')
    .configureLogging(LogLevel.Information)
    .build();

  // Error handling
  connection.onclose(error => {
    console.error('Connection closed:', error);
  });

  connection.onreconnecting(error => {
    console.warn('Connection reconnecting:', error);
  });

  // Když přijde nová data
  connection.on('ReceiveBioData', (playerId, value) => {
    console.log(`Received from ${playerId}: ${value}`);
    if (onBioDataReceived) onBioDataReceived(playerId, value);
  });

  try {
    await connection.start();
    console.log('Connected to SignalR hub.');
    
    // Simulace odeslání dat ze senzoru
    setInterval(() => {
      const randomValue = Math.random() * 100;
      connection.invoke('SendBioData', playerId, "relax", randomValue)
        .catch(err => console.error('Error sending data:', err));
    }, 100);

    return connection;
  } catch (error) {
    console.error('Failed to start connection:', error);
    throw error;
  }
}