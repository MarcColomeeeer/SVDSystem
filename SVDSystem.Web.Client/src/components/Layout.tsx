import { NavLink, Outlet } from 'react-router-dom'
import { useIsAdmin } from '../hooks/useIsAdmin'
import { useMsal } from '@azure/msal-react'

export default function Layout() {
  const isAdmin = useIsAdmin()
  const { instance, accounts } = useMsal()
  const user = accounts[0]

  const navClass = ({ isActive }: { isActive: boolean }) =>
    isActive
      ? 'block px-4 py-2 rounded-md text-sm font-medium transition-colors text-white'
        + ' ' + 'bg-white/20'
      : 'block px-4 py-2 rounded-md text-sm font-medium transition-colors text-white/70 hover:bg-white/10 hover:text-white'

  return (
    <div className="flex h-screen" style={{ backgroundColor: '#f9fafa', color: '#1a2e3b' }}>
      {/* Sidebar */}
      <aside className="w-56 flex-shrink-0 flex flex-col" style={{ backgroundColor: '#255876' }}>
        <div className="px-4 py-5" style={{ borderBottom: '1px solid rgba(255,255,255,0.1)' }}>
          <h1 className="text-lg font-bold text-white">SVDSystem</h1>
          <p className="text-xs mt-0.5 truncate" style={{ color: 'rgba(255,255,255,0.6)' }}>{user?.username}</p>
        </div>

        <nav className="flex-1 px-2 py-4 space-y-1">
          <NavLink to="/" end className={navClass}>Dashboard</NavLink>
          <NavLink to="/repositories" className={navClass}>Repositories</NavLink>
          <NavLink to="/prompts" className={navClass}>Prompts</NavLink>
          <NavLink to="/filter-groups" className={navClass}>Filter Groups</NavLink>
          <NavLink to="/category-groups" className={navClass}>Category Groups</NavLink>
          {isAdmin && <NavLink to="/admin/users" className={navClass}>Users</NavLink>}
        </nav>

        <div className="px-3 py-4" style={{ borderTop: '1px solid rgba(255,255,255,0.1)' }}>
          <button
            onClick={() => instance.logoutRedirect()}
            className="w-full text-left px-4 py-2 rounded-md text-sm transition-colors"
            style={{ color: 'rgba(255,255,255,0.7)' }}
            onMouseEnter={e => { (e.currentTarget as HTMLButtonElement).style.backgroundColor = 'rgba(255,255,255,0.1)'; (e.currentTarget as HTMLButtonElement).style.color = '#fff' }}
            onMouseLeave={e => { (e.currentTarget as HTMLButtonElement).style.backgroundColor = 'transparent'; (e.currentTarget as HTMLButtonElement).style.color = 'rgba(255,255,255,0.7)' }}
          >
            Sign out
          </button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto p-8">
        <Outlet />
      </main>
    </div>
  )
}
