import { connectToBLE } from '../services/ble';

function ConnectButton() {
  return (
    <button onClick={connectToBLE}>
      Připojit GSR zařízení
    </button>
  );
}

export default ConnectButton;
