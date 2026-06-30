import { useAccount } from '@azure/msal-react'
import { useIsAdmin } from '../hooks/useIsAdmin'
import PageHeader from '../components/PageHeader'

export default function Dashboard() {
  const account = useAccount()
  const isAdmin = useIsAdmin()

  return (
    <div>
      <PageHeader
        title={`Welcome, ${account?.name ?? 'User'}`}
        subtitle="SVDSystem — Security Vulnerability Detection"
      />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <Card title="Repositories" description="View and configure repository analysis settings." href="/repositories" />
        <Card title="Prompts" description="System and custom prompt templates for vulnerability analysis." href="/prompts" />
        <Card title="Filter Groups" description="Manage ignore path and file type filter presets." href="/filter-groups" />
        <Card title="Category Groups" description="Manage vulnerability category presets." href="/category-groups" />
        {isAdmin && <Card title="User Management" description="Grant and revoke repository access for users." href="/admin/users" />}

      </div>
    </div>
  )
}

function Card({ title, description, href }: { title: string; description: string; href: string }) {
  return (
    <a
      href={href}
      className="block p-5 rounded-xl transition-colors"
      style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9', boxShadow: '0 1px 4px rgba(37,88,118,0.06)' }}
      onMouseEnter={e => ((e.currentTarget as HTMLAnchorElement).style.borderColor = '#255876')}
      onMouseLeave={e => ((e.currentTarget as HTMLAnchorElement).style.borderColor = '#d5d8d9')}
    >
      <h3 className="font-semibold mb-1" style={{ color: '#1a2e3b' }}>{title}</h3>
      <p className="text-sm" style={{ color: '#6b7f88' }}>{description}</p>
    </a>
  )
}
