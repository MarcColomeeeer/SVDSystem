import { useMsal } from '@azure/msal-react'
import { loginRequest } from '../auth/msalConfig'

export default function LoginPage() {
  const { instance } = useMsal()

  const handleLogin = () => {
    instance.loginRedirect(loginRequest)
  }

  return (
    <div className="min-h-screen flex items-center justify-center" style={{ backgroundColor: '#f9fafa' }}>
      <div className="rounded-2xl p-10 text-center max-w-sm w-full" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9', boxShadow: '0 4px 24px rgba(37,88,118,0.08)' }}>
        <div className="w-12 h-12 rounded-xl flex items-center justify-center mx-auto mb-4" style={{ backgroundColor: '#255876' }}>
          <span className="text-white font-bold text-lg">S</span>
        </div>
        <h1 className="text-2xl font-bold mb-2" style={{ color: '#1a2e3b' }}>SVDSystem</h1>
        <p className="text-sm mb-8" style={{ color: '#6b7f88' }}>Security Vulnerability Detection</p>
        <button
          onClick={handleLogin}
          className="w-full py-2.5 text-white font-medium rounded-lg transition-colors"
          style={{ backgroundColor: '#255876' }}
          onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#1d4460')}
          onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#255876')}
        >
          Sign in with Microsoft
        </button>
      </div>
    </div>
  )
}
