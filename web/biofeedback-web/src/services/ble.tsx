export async function connectToBLE(
  onDataReceived?: (data: { gsr: number; timestamp: Date }) => void,
  onBatteryReceived?: (level: number) => void
) {
  if (!navigator.bluetooth) {
    throw new Error("Bluetooth není v tomto prohlížeči podporováno.");
  }

  let batteryPollTimer: number | undefined;
  let batteryReadInProgress = false;

  const SERVICE_UUID = '0000b100-0000-1000-8000-00805f9b34fb';
  const CHARACTERISTIC_UUID = '0000b101-0000-1000-8000-00805f9b34fb';
  const BATTERY_SERVICE = 'battery_service';
  const BATTERY_LEVEL_CHAR = 'battery_level';

  try {
    console.log('Vyhledávám zařízení...');
    const device = await navigator.bluetooth.requestDevice({
      acceptAllDevices: true,
      optionalServices: [SERVICE_UUID, BATTERY_SERVICE]
    });

    console.log('Připojuji se k serveru GATT...');
    const server = device.gatt!;

    // Pomocná funkce, která zajistí aktivní spojení (reconnect pokud potřebné)
    const ensureConnected = async () => {
      if (!server.connected) {
        try {
          await server.connect();
        } catch (e) {
          console.warn('Nepodařilo se připojit k GATT serveru:', e);
          throw e;
        }
      }
    };

    await ensureConnected();

    console.log('Získávám službu GSR...');
    // Teď už bezpečně získáme službu
    const service = await server.getPrimaryService(SERVICE_UUID);
    const characteristic = await service.getCharacteristic(CHARACTERISTIC_UUID);

    await characteristic.startNotifications();
    console.log('GSR notifikace zapnuty');

    // --- Battery setup (notify nebo bezpečný polling) ---
    let batteryChar: BluetoothRemoteGATTCharacteristic | null = null;
    try {
      const batteryService = await server.getPrimaryService(BATTERY_SERVICE);
      batteryChar = await batteryService.getCharacteristic(BATTERY_LEVEL_CHAR);
      console.log('Battery properties:', batteryChar.properties);
    } catch (err) {
      console.warn('Battery service/char není dostupná:', err);
      batteryChar = null;
    }

    if (batteryChar) {
      if (batteryChar.properties.notify) {
        await batteryChar.startNotifications();
        batteryChar.addEventListener('characteristicvaluechanged', (ev: any) => {
          const level = (ev.target.value as DataView).getUint8(0);
          if (onBatteryReceived) onBatteryReceived(level);
        });
      } else {
        const readAndEmit = async () => {
          if (batteryReadInProgress) return;
          batteryReadInProgress = true;
          try {
            if (!device.gatt?.connected) await device.gatt!.connect();
            const val = await batteryChar!.readValue();
            const level = val.getUint8(0);
            if (onBatteryReceived) onBatteryReceived(level);
          } catch (e) {
            console.warn('Chyba při čtení battery:', e);
          } finally {
            batteryReadInProgress = false;
          }
        };
        await readAndEmit();
        batteryPollTimer = window.setInterval(readAndEmit, 60_000);
      }
    }

    // --- GSR listener: pouze parsování GSR dat (ne čtení battery zde) ---
    characteristic.addEventListener('characteristicvaluechanged', (event: any) => {
      const value = event.target.value as DataView;
      const gsr = value.getUint16(0, false); // nebo true podle endianness
      const data = { gsr, timestamp: new Date() };
      if (onDataReceived) onDataReceived(data);
    });

    // cleanup při odpojení
    device.addEventListener('gattserverdisconnected', () => {
      if (batteryPollTimer !== undefined) {
        clearInterval(batteryPollTimer);
        batteryPollTimer = undefined;
      }
    });

    return device;
  } catch (error) {
    console.error('BLE chyba:', error);
    throw error;
  }
}