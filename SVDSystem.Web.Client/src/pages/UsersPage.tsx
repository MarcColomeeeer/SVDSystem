import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { userAccessApi } from '../api/userAccess'
import { repositoriesApi } from '../api/repositories'
import { usersApi } from '../api/users'
import PageHeader from '../components/PageHeader'
import Spinner from '../components/Spinner'

export default function UsersPage() {
  const queryClient = useQueryClient()

  const { data: repos, isLoading: reposLoading } = useQuery({ queryKey: ['repositories'], queryFn: repositoriesApi.getAll })
  const { data: users, isLoading: usersLoading } = useQuery({ queryKey: ['users'], queryFn: usersApi.getAll })

  const [selectedUserId, setSelectedUserId] = useState<string | null>(null)
  const [viewMode, setViewMode] = useState<'view' | 'edit'>('view')
  const [newAccessRepoId, setNewAccessRepoId] = useState<string>('')

  const { data: userAccesses, isLoading: accessesLoading } = useQuery({
    queryKey: ['user-accesses', selectedUserId],
    queryFn: () => selectedUserId ? userAccessApi.getUserAccesses(selectedUserId) : Promise.resolve([]),
    enabled: !!selectedUserId,
  })

  const grantMutation = useMutation({
    mutationFn: (payload: { userId: string; repoId: string }) => userAccessApi.grant(payload.userId, { repositoryConfigurationId: payload.repoId }),
    onSuccess: (_data, vars) => {
      queryClient.invalidateQueries({ queryKey: ['user-accesses', vars.userId] })
      setNewAccessRepoId('')
      setViewMode('view')
    }
  })

  const revokeMutation = useMutation({
    mutationFn: (id: string) => userAccessApi.revoke(id),
    onSuccess: (_data, id) => queryClient.invalidateQueries({ queryKey: ['user-accesses', selectedUserId] }),
  })

  if (reposLoading || usersLoading) return <Spinner />

  return (
    <div>
      <PageHeader title="User Management" subtitle="View and manage repository access per user." />

      <div className="grid grid-cols-3 gap-6">
        <div className="col-span-1">
          <div className="rounded-xl p-5 space-y-3" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
            <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>Users</h3>

            <div className="mt-3">
              <input className="input w-full" placeholder="Search by name or email…" onChange={e => { /* TODO: implement search state */ }} />
            </div>

            <div className="mt-3 space-y-2">
              {users?.map(u => (
                <div key={u.id} className="p-3 rounded-xl" style={{ backgroundColor: '#f4f7f9', border: '1px solid #d5d8d9' }}>
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <p className="font-medium" style={{ color: '#1a2e3b' }}>{u.displayName}</p>
                      <p className="text-xs" style={{ color: '#9aa5ab' }}>{u.email}</p>
                    </div>
                    <div className="flex gap-2 flex-shrink-0">
                      <button className="btn-ghost text-xs" onClick={() => { setSelectedUserId(u.id); setViewMode('view') }}>View</button>
                      <button className="btn-ghost text-xs" onClick={() => { setSelectedUserId(u.id); setViewMode('edit') }}>Edit</button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="col-span-2">
          {!selectedUserId ? (
            <p className="text-sm" style={{ color: '#6b7f88' }}>Select a user to view or edit their repository access.</p>
          ) : (
            <div>
              <h3 className="text-sm font-semibold mb-3">{viewMode === 'view' ? 'View user' : 'Edit user'}</h3>

              <div className="mb-4 p-4 rounded-xl" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
                <p className="text-sm font-medium" style={{ color: '#1a2e3b' }}>{users?.find(x => x.id === selectedUserId)?.displayName}</p>
                <p className="text-xs mb-3" style={{ color: '#6b7f88' }}>{users?.find(x => x.id === selectedUserId)?.email}</p>

                <h4 className="text-xs font-semibold mb-2">Repositories with access</h4>

                {accessesLoading ? <Spinner /> : (
                  <div className="space-y-2 mb-3">
                    {userAccesses?.map(a => (
                      <div key={a.id} className="flex items-center justify-between p-3 rounded-md" style={{ backgroundColor: '#f9fafb', border: '1px solid #e5e7eb' }}>
                        <div>
                          <p className="text-sm">{a.repositoryProjectName} / {a.repositoryName}</p>
                        </div>
                        <div className="flex gap-2">
                          <button onClick={() => revokeMutation.mutate(a.id)} className="text-xs px-3 py-1.5 rounded-md" style={{ color: '#c0392b' }}>Revoke</button>
                        </div>
                      </div>
                    ))}
                    {userAccesses?.length === 0 && <p className="text-sm" style={{ color: '#6b7f88' }}>No repository access granted.</p>}
                  </div>
                )}

                {viewMode === 'edit' && (
                  <div className="mt-4">
                    <h4 className="text-xs font-semibold mb-2">Grant new access</h4>
                    <div className="flex gap-2 items-center">
                      <select className="input" value={newAccessRepoId} onChange={e => setNewAccessRepoId(e.target.value)}>
                        <option value="">— Choose a repository —</option>
                        {repos?.filter(r => !userAccesses?.some(a => a.repositoryConfigurationId === r.id)).map(r => (
                          <option key={r.id} value={r.id}>{r.projectName} / {r.repositoryName}</option>
                        ))}
                      </select>
                      <button className="btn-primary" onClick={() => selectedUserId && newAccessRepoId && grantMutation.mutate({ userId: selectedUserId, repoId: newAccessRepoId })} disabled={grantMutation.isLoading || !newAccessRepoId}>
                        {grantMutation.isLoading ? 'Granting…' : 'Grant access'}
                      </button>
                    </div>
                    {grantMutation.isError && <p className="text-red-400 text-xs mt-2">Failed to grant access.</p>}
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
