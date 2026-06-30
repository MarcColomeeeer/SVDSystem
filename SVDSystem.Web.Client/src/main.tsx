import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { MsalProvider } from '@azure/msal-react'
import { msalInstance } from './auth/msalConfig'
import App from './App'
import './index.css'

await msalInstance.initialize()

// Process the redirect response after returning from Microsoft login
const redirectResult = await msalInstance.handleRedirectPromise()
if (redirectResult?.account) {
  msalInstance.setActiveAccount(redirectResult.account)
} else {
  // Already signed in from a previous session
  const accounts = msalInstance.getAllAccounts()
  if (accounts.length > 0) msalInstance.setActiveAccount(accounts[0])
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MsalProvider instance={msalInstance}>
      <App />
    </MsalProvider>
  </StrictMode>,
)
