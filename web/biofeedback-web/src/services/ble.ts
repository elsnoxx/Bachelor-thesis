/**
 * Check if Web Bluetooth is available and ready to use
 */
export function checkBluetoothAvailability() {
  // First check if Bluetooth is supported by the browser
  if (!navigator.bluetooth) {
    console.error('❌ Web Bluetooth API není podporováno v tomto prohlížeči.');
    alert('Váš prohlížeč nepodporuje Bluetooth připojení.\nPoužijte prosím Chrome nebo Edge.');
    return false;
  }
  
  return true;
}

export async function connectToBLE() {
  // Check if Web Bluetooth API is available
  if (!checkBluetoothAvailability()) return;
  
  try {
    // Show instructions before requesting the device
    console.log('Povolte Bluetooth v dialogovém okně, které se zobrazí...');
    
    const device = await navigator.bluetooth.requestDevice({
      filters: [{ services: ['battery_service'] }], // nahraď UUID služby tvého zařízení
      optionalServices: ['device_information']       // přidej, pokud potřebuješ další služby
    });

    const server = await device.gatt?.connect();
    console.log('✅ Připojeno:', device.name);

    const service = await server?.getPrimaryService('battery_service');
    const characteristic = await service?.getCharacteristic('battery_level');

    const value = await characteristic?.readValue();
    console.log('🔋 Hodnota:', value?.getUint8(0));

    // Nebo naslouchej změnám
    characteristic?.addEventListener('characteristicvaluechanged', (event) => {
      const val = (event.target as BluetoothRemoteGATTCharacteristic).value;
      console.log('📈 Nová hodnota:', val?.getUint8(0));
    });
    await characteristic?.startNotifications();

  } catch (error) {
    console.error('❌ Chyba při připojení k BLE:', error);
    
    // Provide more helpful error messages based on error type
    if (error.name === 'NotFoundError') {
      const message = 
        'Bluetooth je v prohlížeči zakázáno.\n\n' +
        'Jak povolit Bluetooth:\n' +
        '1. Zadejte do adresního řádku: chrome://flags\n' +
        '2. Vyhledejte "Bluetooth"\n' +
        '3. Povolte "Experimental Web Platform features"\n' +
        '4. Restartujte prohlížeč';
      
      alert(message);
    } else if (error.name === 'SecurityError') {
      alert('Bluetooth vyžaduje zabezpečené připojení (HTTPS nebo localhost).');
    } else if (error.name === 'NotAllowedError') {
      alert('Přístup k Bluetooth byl zamítnut. Zkuste to znovu a povolte přístup.');
    } else {
      alert(`Chyba při připojení k BLE: ${error.message}`);
    }
  }
}