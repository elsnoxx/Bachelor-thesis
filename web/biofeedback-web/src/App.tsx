import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import ConnectButton from './commpoment/ConnectButton'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <ConnectButton />
    </>
  )
}

export default App
