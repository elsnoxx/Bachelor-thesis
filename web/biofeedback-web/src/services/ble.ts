export async function connectToBLE(onDataReceived?: (data: { gsr: number; timestamp: Date }) => void) {
  if (!navigator.bluetooth) {
    throw new Error("Bluetooth is not supported by this browser.");
  }
  
  try {
    const device = await navigator.bluetooth.requestDevice({
      filters: [
        { services: ['0000b100-0000-1000-8000-00805f9b34fb'] }
      ],
    });

    const server = await device.gatt!.connect();
    
    const SERVICE_UUID = '0000b100-0000-1000-8000-00805f9b34fb';
    const service = await server.getPrimaryService(SERVICE_UUID);

    const CHARACTERISTIC_UUID = '0000b101-0000-1000-8000-00805f9b34fb';
    const characteristic = await service.getCharacteristic(CHARACTERISTIC_UUID);

    await characteristic.startNotifications();
    
    characteristic.addEventListener('characteristicvaluechanged', (event) => {
      const value = (event.target as BluetoothRemoteGATTCharacteristic).value!;
      const gsr = value.getUint8(0) * 256 + value.getUint8(1);
      
      const data = { gsr, timestamp: new Date() };
      
      // Vypsání do konzole
      console.log('📈 GSR:', gsr, 'Čas:', data.timestamp.toLocaleTimeString());
      
      // Volání callback funkce pro předání dat do komponenty
      if (onDataReceived) {
        onDataReceived(data);
      }
      
      // Uložení do databáze
      insertDataIntoDatabase(data);
    });

    console.log("✅ Připojeno k GATT serveru");
    return server;
    
  } catch (error) {
    console.error("❌ Bluetooth connection failed:", error);
    throw error;
  }
} 

function insertDataIntoDatabase(data: { gsr: number; timestamp: Date }) {
  console.log("💾 Ukládám data do databáze:", data);
  // Zde implementujte logiku pro uložení do databáze
}