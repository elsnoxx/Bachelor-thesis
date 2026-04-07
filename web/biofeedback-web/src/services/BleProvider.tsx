import React, { createContext, useContext, useState, useRef, useEffect, ReactNode } from "react";
import { connectToBLE } from "./ble"; // Importujeme tvou funkci

interface BleContextType {
  isConnected: boolean;
  gsrValue: number | null;
  error: string | null;
  connect: () => Promise<void>;
  disconnect: () => void;
}

const BleContext = createContext<BleContextType | undefined>(undefined);

export const BleProvider = ({ children }: { children: ReactNode }) => {
  const [isConnected, setIsConnected] = useState(false);
  const [gsrValue, setGsrValue] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const deviceRef = useRef<BluetoothDevice | null>(null);

  useEffect(() => {
    if (!(navigator as any).bluetooth) return;

    const handleAvailabilityChange = (e: any) => {
      if (e.value) {
        setError(null); // Bluetooth byl zapnut, smažeme chybu
      } else {
        setError("Bluetooth adaptér byl vypnut v systému.");
      }
    };

    (navigator as any).bluetooth.addEventListener('availabilitychanged', handleAvailabilityChange);

    return () => {
      (navigator as any).bluetooth.removeEventListener('availabilitychanged', handleAvailabilityChange);
    };
  }, []);

  // helper: zjistí dostupnost adaptéru
  async function isBluetoothAvailable(): Promise<boolean> {
    if (!(navigator as any).bluetooth) return false;
    try {
      return Boolean(await (navigator as any).bluetooth.getAvailability());
    } catch {
      return false;
    }
  }

  const connect = async () => {
    try {
      setError(null);

      // rychlá kontrola dostupnosti adaptérů
      const available = await isBluetoothAvailable();
      if (!available) {
        setError(
          "Bluetooth adaptér není dostupný. Zkontrolujte, že máte Bluetooth zapnutý v systému a povolený prohlížeč. " +
          "Spusťte stránku přes HTTPS/localhost a povolte Bluetooth v nastavení prohlížeče."
        );
        return;
      }

      // dál voláme requestDevice (stejně jako máš) — chyby zachytíme níže
      const device = await connectToBLE((data) => setGsrValue(data.gsr));

      deviceRef.current = device;
      setIsConnected(true);

      // Handler pro nečekané odpojení (např. vypnutí senzoru)
      device.addEventListener('gattserverdisconnected', () => {
        setIsConnected(false);
        setGsrValue(null);
      });
    } catch (err: any) {
      // přátelské hlášky podle typu chyby
      if (err?.name === "NotFoundError") {
        setError("Zařízení se nenašlo (filtr neodpovídá) nebo Web Bluetooth je vypnutý v prohlížeči.");
      } else if (err?.name === "NotAllowedError" || /permission/i.test(err?.message)) {
        setError("Přístup k Bluetooth byl zablokován. Ověřte nastavení stránek v prohlížeči a povolte ho.");
      } else if (err?.name === "NotSupportedError") {
        setError("Tento prohlížeč nepodporuje Web Bluetooth. Použijte Chrome / Edge na desktopu nebo Android (Chromium).");
      } else {
        setError("Chyba při připojování: " + (err?.message ?? String(err)));
      }
    }
  };

  const disconnect = () => {
    if (deviceRef.current?.gatt?.connected) {
      deviceRef.current.gatt.disconnect();
      // Po manuálním odpojení resetujeme stavy
      setIsConnected(false);
      setGsrValue(null);
    }
  };

  useEffect(() => {
    return () => disconnect();
  }, []);

  return (
    <BleContext.Provider value={{ isConnected, gsrValue, error, connect, disconnect }}>
      {children}
    </BleContext.Provider>
  );
};

export const useBle = () => {
  const context = useContext(BleContext);
  if (!context) throw new Error("useBle musí být použit uvnitř BleProvideru");
  return context;
};