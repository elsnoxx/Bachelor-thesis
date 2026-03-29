export async function connectToBLE(onDataReceived?: (data: { gsr: number; timestamp: Date }) => void) {
  if (!(navigator as any).bluetooth) {
    throw new Error("Bluetooth není v tomto prohlížeči podporováno.");
  }

  try {
    // nejprve zkusíme filtrované volání (bezpečnější), potom fallback
    let device;
    try {
      device = await (navigator as any).bluetooth.requestDevice({
        filters: [{ services: ['0000b100-0000-1000-8000-00805f9b34fb'] }],
        optionalServices: ['0000b100-0000-1000-8000-00805f9b34fb']
      });
    } catch (err) {
      console.warn('Filtrované requestDevice selhalo, zkouším fallback acceptAllDevices:', err);
      device = await (navigator as any).bluetooth.requestDevice({
        acceptAllDevices: true,
        optionalServices: ['0000b100-0000-1000-8000-00805f9b34fb']
      });
    }

    const server = await device.gatt.connect();
    const service = await server.getPrimaryService('0000b100-0000-1000-8000-00805f9b34fb');
    const characteristic = await service.getCharacteristic('0000b101-0000-1000-8000-00805f9b34fb');

    await characteristic.startNotifications();

    characteristic.addEventListener('characteristicvaluechanged', (event: any) => {
      const dv = (event.target as any).value as DataView;
      const gsr = dv.getUint8(0) * 256 + dv.getUint8(1);
      const data = { gsr, timestamp: new Date() };
      console.log('GSR:', gsr);
      if (onDataReceived) onDataReceived(data);
    });

    return device;
  } catch (error) {
    console.error('BLE connect failed:', error);
    throw error;
  }
}