import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useAccount } from '@azure/msal-react'
import { categoryGroupsApi } from '../api/categoryGroups'
import { vulnerabilityCategoriesApi } from '../api/vulnerabilityCategories'
import { useIsAdmin } from '../hooks/useIsAdmin'
import type { CategoryGroup, VulnerabilityCategory } from '../types'
import Modal from '../components/Modal'
import PageHeader from '../components/PageHeader'
import SearchUserFilter from '../components/SearchUserFilter'
import Spinner from '../components/Spinner'


// ── Master list helpers ───────────────────────────────────────────────────────

function CategoryMasterList() {
  const queryClient = useQueryClient()
  const isAdmin = useIsAdmin()
  const account = useAccount()
  const myOid = (account?.idTokenClaims as Record<string, unknown>)?.oid as string ?? ''

  const { data: categories = [], isLoading } = useQuery({
    queryKey: ['vulnerability-categories'],
    queryFn: vulnerabilityCategoriesApi.getAll,
  })

  const [editingId, setEditingId] = useState<string | null>(null)
  const [editName, setEditName] = useState('')
  const [newName, setNewName] = useState('')
  const [error, setError] = useState<string | null>(null)

  const canEdit = (c: VulnerabilityCategory) => isAdmin || c.createdByObjectId === myOid

  const createMutation = useMutation({
    mutationFn: () => vulnerabilityCategoriesApi.create({ name: newName.trim() }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['vulnerability-categories'] }); setNewName(''); setError(null) },
    onError: (e: unknown) => setError((e as { response?: { data?: string } }).response?.data ?? 'Already exists or invalid name.'),
  })

  const updateMutation = useMutation({
    mutationFn: (id: string) => vulnerabilityCategoriesApi.update(id, { name: editName.trim() }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['vulnerability-categories'] }); setEditingId(null); setError(null) },
    onError: (e: unknown) => setError((e as { response?: { data?: string } }).response?.data ?? 'Already exists.'),
  })

  const deleteMutation = useMutation({
    mutationFn: vulnerabilityCategoriesApi.delete,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['vulnerability-categories'] }),
    onError: (e: unknown) => setError((e as { response?: { data?: string } }).response?.data ?? 'Cannot delete: this category may be in use.'),
  })

  if (isLoading) return <Spinner />

  return (
    <div className="rounded-xl p-5 space-y-4" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
      <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>
        Vulnerability Category List
      </h3>
      <p className="text-xs" style={{ color: '#9aa5ab' }}>
        Master list of vulnerability categories. Users can only edit or delete categories they created.
      </p>

      {/* Add new */}
      <div className="flex gap-2">
        <input
          className="input flex-1 text-xs"
          placeholder="New category name (e.g. Injection Attacks)…"
          value={newName}
          onChange={e => { setNewName(e.target.value); setError(null) }}
          onKeyDown={e => e.key === 'Enter' && newName.trim() && createMutation.mutate()}
        />
        <button
          onClick={() => createMutation.mutate()}
          disabled={!newName.trim() || createMutation.isPending}
          className="btn-primary px-4 text-xs"
        >
          Add
        </button>
      </div>
      {error && <p className="text-xs px-3 py-2 rounded" style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}>{error}</p>}

      {/* 4-column grid */}
      {categories.length === 0 ? (
        <p className="text-sm text-center py-4" style={{ color: '#9aa5ab' }}>No categories yet. Add one above.</p>
      ) : (
        <div className="grid grid-cols-4 gap-2">
          {categories.map(c => (
            <div
              key={c.id}
              className="flex items-center justify-between gap-2 px-3 py-2 rounded-lg"
              style={{ backgroundColor: '#f4f7f9', border: '1px solid #d5d8d9' }}
            >
              {editingId === c.id ? (
                <input
                  className="input text-xs flex-1"
                  value={editName}
                  onChange={e => setEditName(e.target.value)}
                  onKeyDown={e => { if (e.key === 'Enter') updateMutation.mutate(c.id); if (e.key === 'Escape') setEditingId(null) }}
                  autoFocus
                />
              ) : (
                <span className="text-xs truncate flex-1" style={{ color: '#1a2e3b' }}>{c.name}</span>
              )}

              {canEdit(c) && (
                <div className="flex gap-1 flex-shrink-0">
                  {editingId === c.id ? (
                    <>
                      <button
                        onClick={() => updateMutation.mutate(c.id)}
                        className="text-xs px-2 py-0.5 rounded font-medium text-white"
                        style={{ backgroundColor: '#255876' }}
                      >✓</button>
                      <button
                        onClick={() => setEditingId(null)}
                        className="text-xs px-2 py-0.5 rounded"
                        style={{ color: '#6b7f88' }}
                      >✕</button>
                    </>
                  ) : (
                    <>
                      <button
                        onClick={() => { setEditingId(c.id); setEditName(c.name); setError(null) }}
                        className="text-xs"
                        style={{ color: '#255876' }}
                      >✎</button>
                      <button
                        onClick={() => deleteMutation.mutate(c.id)}
                        className="text-xs"
                        style={{ color: '#c0392b' }}
                      >✕</button>
                    </>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

// ── Group creator ─────────────────────────────────────────────────────────────

type GroupForm = { name: string; selected: Set<string> }

function CategoryGroupManager() {
  const queryClient = useQueryClient()
  const isAdmin = useIsAdmin()
  const account = useAccount()
  const myOid = (account?.idTokenClaims as Record<string, unknown>)?.oid as string ?? ''

  const { data: allCategories = [] } = useQuery({
    queryKey: ['vulnerability-categories'],
    queryFn: vulnerabilityCategoriesApi.getAll,
  })
  const { data: groups = [], isLoading } = useQuery({
    queryKey: ['category-groups'],
    queryFn: categoryGroupsApi.getAll,
  })

  const [editing, setEditing] = useState<CategoryGroup | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<GroupForm>({ name: '', selected: new Set() })
  const [filterUser, setFilterUser] = useState('')
  const [search, setSearch] = useState('')
  const [groupError, setGroupError] = useState<string | null>(null)

  const canEdit = (g: CategoryGroup) => isAdmin || g.createdByObjectId === myOid

  const toggle = (name: string) =>
    setForm(f => {
      const next = new Set(f.selected)
      next.has(name) ? next.delete(name) : next.add(name)
      return { ...f, selected: next }
    })

  const startCreate = () => {
    setCreating(true); setEditing(null)
    setForm({ name: '', selected: new Set() })
  }

  const startEdit = (g: CategoryGroup) => {
    setEditing(g); setCreating(false)
    setForm({ name: g.name, selected: new Set(g.categories.split(',').map(s => s.trim()).filter(Boolean)) })
  }

  const categoriesToString = () => Array.from(form.selected).sort().join(', ')

  const saveMutation = useMutation({
    mutationFn: () => {
      const dto = { name: form.name, categories: categoriesToString() }
      return editing ? categoryGroupsApi.update(editing.id, dto) : categoryGroupsApi.create(dto)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['category-groups'] })
      setEditing(null); setCreating(false); setGroupError(null)
    },
    onError: (e: unknown) => setGroupError((e as { response?: { data?: string } }).response?.data ?? 'Could not save. The name may already be in use.'),
  })

  const deleteMutation = useMutation({
    mutationFn: categoryGroupsApi.delete,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['category-groups'] }),
    onError: (e: unknown) => setGroupError((e as { response?: { data?: string } }).response?.data ?? 'Cannot delete: this group may be in use.'),
  })

  const users = Array.from(new Set(groups.map(g => g.createdByDisplayName).filter(Boolean)))

  const filtered = groups.filter(g => {
    const matchesUser = !filterUser || g.createdByDisplayName === filterUser
    const matchesSearch = !search || g.name.toLowerCase().includes(search.toLowerCase())
    return matchesUser && matchesSearch
  })

  if (isLoading) return <Spinner />

  return (
    <div className="rounded-xl p-5 space-y-4" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>Category Groups</h3>
        <button onClick={startCreate} className="btn-primary text-xs px-3 py-1.5">+ New group</button>
      </div>

      {/* Create / Edit modal */}
      {(creating || editing) && (
        <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
          <div className="rounded-xl p-6 w-full max-w-5xl max-h-[85vh] flex flex-col space-y-4" style={{ backgroundColor: '#ffffff', border: '2px solid #255876' }}>
            <h4 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>
              {creating ? 'New Category Group' : `Edit: ${editing!.name}`}
            </h4>

            <div>
              <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Group name</label>
              <input
                className="input"
                value={form.name}
                onChange={e => { setForm(f => ({ ...f, name: e.target.value })); setGroupError(null) }}
                placeholder="e.g. OWASP Top 10"
              />
              {groupError && (
                <p className="text-xs mt-2 px-3 py-2 rounded" style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}>{groupError}</p>
              )}
            </div>

            {/* Checkbox grid */}
            <div className="flex-1 overflow-y-auto">
              <label className="block text-xs font-medium mb-2" style={{ color: '#6b7f88' }}>
                Select categories ({form.selected.size} selected)
              </label>
              {allCategories.length === 0 ? (
                <p className="text-xs" style={{ color: '#9aa5ab' }}>No categories in the master list yet. Add some first.</p>
              ) : (
                <div className="grid grid-cols-4 gap-2">
                  {[...allCategories].sort((a, b) => a.name.localeCompare(b.name)).map(c => (
                    <label key={c.id} className="flex items-center gap-2 cursor-pointer px-2 py-1.5 rounded-lg" style={{ backgroundColor: form.selected.has(c.name) ? '#e8f0f5' : '#f4f7f9', border: `1px solid ${form.selected.has(c.name) ? '#255876' : '#d5d8d9'}` }}>
                      <input
                        type="checkbox"
                        checked={form.selected.has(c.name)}
                        onChange={() => toggle(c.name)}
                        style={{ accentColor: '#255876' }}
                      />
                      <span className="text-sm" style={{ color: '#1a2e3b' }}>{c.name}</span>
                    </label>
                  ))}
                </div>
              )}
            </div>

            {form.selected.size > 0 && (
              <div className="text-xs px-3 py-2 rounded" style={{ backgroundColor: '#e8f0f5', color: '#255876' }}>
                <span className="font-medium">Preview: </span>{categoriesToString()}
              </div>
            )}

            <div className="flex gap-2 pt-1">
              <button
                onClick={() => saveMutation.mutate()}
                disabled={!form.name.trim() || form.selected.size === 0 || saveMutation.isPending}
                className="btn-primary"
              >
                {saveMutation.isPending ? 'Saving…' : 'Save group'}
              </button>
              {form.selected.size > 0 ? (
                <button
                  onClick={() => setForm(f => ({ ...f, selected: new Set() }))}
                  className="text-xs px-3 py-1.5 rounded-md font-medium"
                  style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}
                  onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#fbe4e4')}
                  onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                >
                  Clear all
                </button>
              ) : (
                <button
                  disabled
                  className="text-xs px-3 py-1.5 rounded-md font-medium"
                  style={{ backgroundColor: '#fdf2f2', color: '#c0392b', opacity: 0.4, cursor: 'not-allowed' }}
                >
                  Clear all
                </button>
              )}
              <button onClick={() => { setEditing(null); setCreating(false); setGroupError(null) }} className="btn-ghost">Cancel</button>
            </div>
          </div>
        </div>
      )}

      {/* Search and user filter */}
      <div className="flex gap-2">
        <input
          className="input flex-1 text-xs"
          placeholder="Search by group name…"
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
        <select className="input w-44 text-xs" value={filterUser} onChange={e => setFilterUser(e.target.value)}>
          <option value="">All users</option>
          {users.map(u => <option key={u} value={u}>{u}</option>)}
        </select>
      </div>

      {/* Groups list */}
      {groupError && !creating && !editing && (
        <p className="text-xs px-3 py-2 rounded" style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}>{groupError}</p>
      )}
      <div className="space-y-3">
        {filtered.length === 0 && (
          <p className="text-sm text-center py-4" style={{ color: '#9aa5ab' }}>No groups yet.</p>
        )}
        {filtered.map(g => (
          <div key={g.id} className="p-4 rounded-xl" style={{ backgroundColor: '#f4f7f9', border: '1px solid #d5d8d9' }}>
            <div className="flex items-center justify-between mb-1">
              <div>
                <p className="font-medium text-sm" style={{ color: '#1a2e3b' }}>{g.name}</p>
                <p className="text-xs" style={{ color: '#9aa5ab' }}>By {g.createdByDisplayName || 'Unknown'}</p>
              </div>
              {canEdit(g) && (
                <div className="flex gap-2">
                  <button onClick={() => startEdit(g)} className="btn-ghost text-xs">Edit</button>
                  <button
                    onClick={() => deleteMutation.mutate(g.id)}
                    className="text-xs px-3 py-1.5 rounded-md transition-colors"
                    style={{ color: '#c0392b' }}
                  >Delete</button>
                </div>
              )}
            </div>
            <div className="flex flex-wrap gap-1.5 mt-2">
              {g.categories.split(',').filter(Boolean).map(c => (
                <span key={c} className="text-xs px-2 py-0.5 rounded-full" style={{ backgroundColor: '#e8f0f5', color: '#255876' }}>
                  {c.trim()}
                </span>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function CategoryGroupsPage() {
  return (
    <div>
      <PageHeader
        title="Category Groups"
        subtitle="Manage vulnerability categories and reusable category groups."
      />
      <div className="space-y-6">
        <CategoryMasterList />
        <CategoryGroupManager />
      </div>
    </div>
  )
}
