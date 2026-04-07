export async function connectToBLE(onDataReceived?: (data: { gsr: number; timestamp: Date }) => void) {
  if (!navigator.bluetooth) {
    throw new Error("Bluetooth není v tomto prohlížeči podporováno.");
  }

  // Definice UUID malými písmeny (Web Bluetooth je na to citlivý)
  const SERVICE_UUID = '0000b100-0000-1000-8000-00805f9b34fb';
  const CHARACTERISTIC_UUID = '0000b101-0000-1000-8000-00805f9b34fb';

  try {
    console.log('Vyhledávám zařízení...');
    
    const device = await navigator.bluetooth.requestDevice({
      // Zkusíme najít zařízení podle názvu nebo povolit vše, 
      // protože "filters" u necertifikovaných čínských modulů často zlobí
      acceptAllDevices: true, 
      optionalServices: [SERVICE_UUID, 'battery_service'] // Musí zde být vše, co chceš volat
    });

    console.log('Připojuji se k serveru GATT...');
    const server = await device.gatt!.connect();

    console.log('Získávám službu...');
    const service = await server.getPrimaryService(SERVICE_UUID);

    console.log('Získávám charakteristiku...');
    const characteristic = await service.getCharacteristic(CHARACTERISTIC_UUID);

    await characteristic.startNotifications();
    console.log('Notifikace zapnuty');

    characteristic.addEventListener('characteristicvaluechanged', (event: any) => {
      const value = event.target.value as DataView;
      
      // Předpokládáme 16-bit unsigned integer (Big Endian - to co jsi dělal ty)
      // Pokud jsou data v Little Endian, přidej druhý parametr true: getUint16(0, true)
      const gsr = value.getUint16(0, false); 
      
      const data = { gsr, timestamp: new Date() };
      if (onDataReceived) onDataReceived(data);
    });

    return device;
  } catch (error) {
    console.error('BLE chyba:', error);
    throw error;
  }
}