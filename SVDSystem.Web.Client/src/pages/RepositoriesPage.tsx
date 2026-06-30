import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { repositoriesApi } from '../api/repositories'
import PageHeader from '../components/PageHeader'
import Spinner from '../components/Spinner'

export default function RepositoriesPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['repositories'],
    queryFn: repositoriesApi.getAll,
  })

  if (isLoading) return <Spinner />
  if (error) return <p style={{ color: '#c0392b' }}>Failed to load repositories.</p>

  return (
    <div>
      <PageHeader title="Repositories" subtitle="Select a repository to view or edit its settings." />
      <div className="space-y-3">
        {data?.map(repo => (
          <Link
            key={repo.id}
            to={`/repositories/${repo.id}`}
            className="flex items-center justify-between p-4 rounded-xl transition-colors"
            style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}
            onMouseEnter={e => ((e.currentTarget as HTMLAnchorElement).style.borderColor = '#255876')}
            onMouseLeave={e => ((e.currentTarget as HTMLAnchorElement).style.borderColor = '#d5d8d9')}
          >
            <div>
              <p className="font-medium" style={{ color: '#1a2e3b' }}>{repo.repositoryName}</p>
              <p className="text-sm" style={{ color: '#6b7f88' }}>{repo.projectName}</p>
            </div>
            <span className="text-xs px-2 py-1 rounded-full font-medium" style={repo.enabled ? { backgroundColor: '#e6f4ea', color: '#1e7e34' } : { backgroundColor: '#f0f2f2', color: '#6b7f88' }}>
              {repo.enabled ? 'Enabled' : 'Disabled'}
            </span>
          </Link>
        ))}
        {data?.length === 0 && (
          <p className="text-sm" style={{ color: '#6b7f88' }}>No repositories found. They appear automatically when a webhook is received.</p>
        )}
      </div>
    </div>
  )
}
