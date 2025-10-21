import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export async function startConnection(playerId, onBioDataReceived) {
  const connection = new HubConnectionBuilder()
    .withUrl('http://localhost:5000/gamehub')
    .configureLogging(LogLevel.Information)
    .build();

  // Když přijde nová data
  connection.on('ReceiveBioData', (playerId, value) => {
    console.log(`Received from ${playerId}: ${value}`);
    if (onBioDataReceived) onBioDataReceived(playerId, value);
  });

  await connection.start();
  console.log('Connected to SignalR hub.');

  // Simulace odeslání dat ze senzoru
  setInterval(() => {
    const randomValue = Math.random() * 100;
    connection.invoke('SendBioData', playerId, "relax", randomValue);
  }, 100);

  return connection;
}
