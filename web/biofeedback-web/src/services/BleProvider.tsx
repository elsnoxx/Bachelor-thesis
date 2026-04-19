import React, { createContext, useContext, useState, useRef, useEffect, ReactNode } from "react";
import { connectToBLE } from "./ble";

interface BleContextType {
  isConnected: boolean;
  gsrValue: number | null;
  batteryLevel: number | null;
  error: string | null;
  connect: () => Promise<void>;
  disconnect: () => void;
}

const BleContext = createContext<BleContextType | undefined>(undefined);

export const BleProvider = ({ children }: { children: ReactNode }) => {
  const [isConnected, setIsConnected] = useState(false);
  const [gsrValue, setGsrValue] = useState<number | null>(null);
  const [batteryLevel, setBatteryLevel] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const deviceRef = useRef<BluetoothDevice | null>(null);
  const disconnectedHandlerRef = useRef<(() => void) | null>(null);

  useEffect(() => {
    if (!(navigator as any).bluetooth) return;
    const handleAvailabilityChange = (e: any) => {
      if (e.value) setError(null);
      else setError("Bluetooth adaptér byl vypnut v systému.");
    };
    (navigator as any).bluetooth.addEventListener('availabilitychanged', handleAvailabilityChange);
    return () => {
      (navigator as any).bluetooth.removeEventListener('availabilitychanged', handleAvailabilityChange);
    };
  }, []);

  async function isBluetoothAvailable(): Promise<boolean> {
    if (!(navigator as any).bluetooth) return false;
    try { return Boolean(await (navigator as any).bluetooth.getAvailability()); } catch { return false; }
  }

  const connect = async () => {
    try {
      setError(null);
      const available = await isBluetoothAvailable();
      if (!available) {
        setError("Bluetooth adaptér není dostupný. Zkontrolujte nastavení prohlížeče a systém.");
        return;
      }

      const device = await connectToBLE(
        (data) => setGsrValue(data.gsr),
        (lvl) => setBatteryLevel(lvl)
      );

      deviceRef.current = device;
      setIsConnected(true);

      const onDisconnected = () => {
        setIsConnected(false);
        setGsrValue(null);
        setBatteryLevel(null);
      };
      device.addEventListener('gattserverdisconnected', onDisconnected);
      disconnectedHandlerRef.current = onDisconnected;
    } catch (err: any) {
      if (err?.name === "NotFoundError") {
        setError("Zařízení se nenašlo (uživatel zrušil výběr nebo filtr nesedí).");
      } else if (err?.name === "NotAllowedError" || /permission/i.test(err?.message)) {
        setError("Přístup k Bluetooth byl zablokován. Povolit v nastavení stránek.");
      } else if (err?.name === "NotSupportedError") {
        setError("Tento prohlížeč nepodporuje Web Bluetooth.");
      } else {
        setError("Chyba při připojování: " + (err?.message ?? String(err)));
      }
    }
  };

  const disconnect = () => {
    const dev = deviceRef.current;
    if (dev) {
      if (disconnectedHandlerRef.current) {
        dev.removeEventListener('gattserverdisconnected', disconnectedHandlerRef.current);
        disconnectedHandlerRef.current = null;
      }
      if (dev.gatt?.connected) dev.gatt.disconnect();
      deviceRef.current = null;
    }
    setIsConnected(false);
    setGsrValue(null);
    setBatteryLevel(null);
  };

  useEffect(() => () => disconnect(), []);

  return (
    <BleContext.Provider value={{ isConnected, gsrValue, batteryLevel, error, connect, disconnect }}>
      {children}
    </BleContext.Provider>
  );
};

export const useBle = () => {
  const context = useContext(BleContext);
  if (!context) throw new Error("useBle musí být použit uvnitř BleProvideru");
  return context;
};