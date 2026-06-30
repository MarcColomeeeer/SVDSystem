import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useIsAdmin } from '../hooks/useIsAdmin'
import { useMsal } from '@azure/msal-react'
import { filterGroupsApi } from '../api/filterGroups'
import { fileTypeFiltersApi } from '../api/fileTypeFilters'
import type { FileTypeFilter, FilterGroup, UpsertFileTypeFilterDto } from '../types'
import Modal from '../components/Modal'
import PageHeader from '../components/PageHeader'
import SearchUserFilter from '../components/SearchUserFilter'
import Spinner from '../components/Spinner'

// ── File Type Master List ─────────────────────────────────────────────────────

function FileTypeMasterList() {
  const queryClient = useQueryClient()
  const isAdmin = useIsAdmin()
  const { accounts } = useMsal()
  const objectId = accounts[0]?.localAccountId ?? ''

  const { data: fileTypes = [], isLoading } = useQuery({
    queryKey: ['file-type-filters'],
    queryFn: fileTypeFiltersApi.getAll,
  })

  const [showModal, setShowModal] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<UpsertFileTypeFilterDto>({ name: '', extension: '' })
  const [error, setError] = useState<string | null>(null)
  const [deleteError, setDeleteError] = useState<string | null>(null)

  const canEdit = (f: FileTypeFilter) => isAdmin || f.createdByObjectId === objectId

  const EXTENSION_RE = /^\.[a-zA-Z]+$/
  const extensionValid = form.extension.trim() === '' || EXTENSION_RE.test(form.extension.trim())

  const validate = (): string | null => {
    const name = form.name.trim()
    const ext = form.extension.trim()
    if (!name) return 'Display name is required.'
    if (!ext) return 'Extension is required.'
    if (!EXTENSION_RE.test(ext)) return 'Extension must start with a dot followed by letters only (e.g. .sql).'
    if (fileTypes.some(f => f.name.toLowerCase() === name.toLowerCase() && f.id !== editingId))
      return `A file type named "${name}" already exists.`
    if (fileTypes.some(f => f.extension.toLowerCase() === ext.toLowerCase() && f.id !== editingId))
      return `Extension "${ext}" is already used by another file type.`
    return null
  }

  const createMutation = useMutation({
    queryClient,
    mutationFn: () => fileTypeFiltersApi.create({ name: form.name.trim(), extension: form.extension.trim() }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['file-type-filters'] }); closeModal() },
    onError: (e: unknown) => setError((e as { response?: { data?: string } }).response?.data ?? 'Could not save. The name or extension may already exist.'),
  })

  const updateMutation = useMutation({
    queryClient,
    mutationFn: () => fileTypeFiltersApi.update(editingId!, { name: form.name.trim(), extension: form.extension.trim() }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['file-type-filters'] }); closeModal() },
    onError: (e: unknown) => setError((e as { response?: { data?: string } }).response?.data ?? 'Could not save. The name or extension may already exist.'),
  })

  const deleteMutation = useMutation({
    queryClient,
    mutationFn: fileTypeFiltersApi.delete,
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['file-type-filters'] }); setDeleteError(null) },
    onError: (e: unknown) => setDeleteError((e as { response?: { data?: string } }).response?.data ?? 'Cannot delete: this file type may be in use.'),
  })

  const openCreate = () => {
    setEditingId(null)
    setForm({ name: '', extension: '' })
    setError(null)
    setShowModal(true)
  }

  const openEdit = (f: FileTypeFilter) => {
    setEditingId(f.id)
    setForm({ name: f.name, extension: f.extension })
    setError(null)
    setShowModal(true)
  }

  const closeModal = () => {
    setShowModal(false)
    setEditingId(null)
    setForm({ name: '', extension: '' })
    setError(null)
  }

  const handleSave = () => {
    const err = validate()
    if (err) { setError(err); return }
    setError(null)
    editingId ? updateMutation.mutate() : createMutation.mutate()
  }

  const canSave = form.name.trim() !== '' && form.extension.trim() !== '' && extensionValid

  if (isLoading) return <Spinner />

  return (
    <>
      <div className="rounded-xl p-5 space-y-4" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>File Types</h3>
          <button onClick={openCreate} className="btn-primary text-xs">+ New file type</button>
        </div>

        {deleteError && (
          <p className="text-xs px-3 py-2 rounded" style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}>{deleteError}</p>
        )}

        {fileTypes.length === 0 ? (
          <p className="text-sm text-center py-6" style={{ color: '#9aa5ab' }}>No file types yet.</p>
        ) : (
          <div className="grid grid-cols-6 gap-2">
            {fileTypes.map(f => (
              <div
                key={f.id}
                className="flex items-center justify-between px-3 py-2 rounded-lg"
                style={{ backgroundColor: '#f4f7f9', border: '1px solid #d5d8d9' }}
              >
                <div className="min-w-0 flex items-center gap-1.5">
                  <p className="text-xs font-medium truncate" style={{ color: '#1a2e3b' }}>{f.name.replace(/\s*\([^)]+\)\s*$/, '')}</p>
                  <p className="text-xs font-mono shrink-0" style={{ color: '#9aa5ab' }}>{f.extension}</p>
                </div>
                {canEdit(f) && (
                  <div className="flex flex-row gap-1 ml-1 shrink-0">
                    <button onClick={() => openEdit(f)} className="text-xs" style={{ color: '#255876' }}>✎</button>
                    <button onClick={() => deleteMutation.mutate(f.id)} className="text-xs" style={{ color: '#c0392b' }}>✕</button>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {showModal && (
        <Modal title={editingId ? 'Edit File Type' : 'New File Type'} onClose={closeModal} sizeClass="max-w-sm" primary>
          <div className="space-y-4">
            <div>
              <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Display name (e.g. SQL)</label>
              <input
                className="input"
                value={form.name}
                onChange={e => { setForm(f => ({ ...f, name: e.target.value })); setError(null) }}
                placeholder="e.g. SQL"
              />
            </div>
            <div>
              <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Extension (e.g. .sql)</label>
              <input
                className="input"
                value={form.extension}
                onChange={e => { setForm(f => ({ ...f, extension: e.target.value })); setError(null) }}
                placeholder="e.g. .sql"
              />
              {form.extension.trim() && !extensionValid && (
                <p className="text-xs mt-1" style={{ color: '#c0392b' }}>
                  Extension must start with a dot followed by letters only (e.g. .sql).
                </p>
              )}
            </div>
            {error && (
              <p className="text-xs px-3 py-2 rounded" style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}>{error}</p>
            )}
            <div className="flex gap-2">
              <button
                onClick={handleSave}
                disabled={!canSave || createMutation.isPending || updateMutation.isPending}
                className="btn-primary"
              >
                {createMutation.isPending || updateMutation.isPending ? 'Saving…' : 'Save'}
              </button>
              <button onClick={closeModal} className="btn-ghost">Cancel</button>
            </div>
          </div>
        </Modal>
      )}
    </>
  )
}

// ── Filter Group Manager ──────────────────────────────────────────────────────

function FilterGroupManager() {
  const queryClient = useQueryClient()
  const isAdmin = useIsAdmin()
  const { accounts } = useMsal()
  const objectId = accounts[0]?.localAccountId ?? ''

  const { data: groups = [], isLoading: groupsLoading } = useQuery({
    queryKey: ['filter-groups'],
    queryFn: filterGroupsApi.getAll,
  })
  const { data: fileTypes = [], isLoading: ftLoading } = useQuery({
    queryKey: ['file-type-filters'],
    queryFn: fileTypeFiltersApi.getAll,
  })

  const [showModal, setShowModal] = useState(false)
  const [editingGroup, setEditingGroup] = useState<FilterGroup | null>(null)
  const [form, setForm] = useState({ name: '', ignorePaths: '' })
  const [selectedExtensions, setSelectedExtensions] = useState<Set<string>>(new Set())
  const [groupError, setGroupError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [filterUser, setFilterUser] = useState('')

  const canEdit = (g: FilterGroup) => isAdmin || g.createdByObjectId === objectId

  const groupNameTaken = (name: string) =>
    groups.some(g =>
      g.name.toLowerCase() === name.trim().toLowerCase() &&
      g.createdByObjectId === objectId &&
      g.id !== editingGroup?.id
    )

  const openCreate = () => {
    setEditingGroup(null)
    setForm({ name: '', ignorePaths: '' })
    setSelectedExtensions(new Set())
    setGroupError(null)
    setShowModal(true)
  }

  const openEdit = (g: FilterGroup) => {
    setEditingGroup(g)
    setForm({ name: g.name, ignorePaths: g.ignorePaths })
    setSelectedExtensions(new Set(g.fileTypeExtensions.split(',').map(s => s.trim()).filter(Boolean)))
    setGroupError(null)
    setShowModal(true)
  }

  const closeModal = () => { setShowModal(false); setEditingGroup(null); setGroupError(null) }

  const toggleExt = (ext: string) => setSelectedExtensions(prev => {
    const next = new Set(prev)
    next.has(ext) ? next.delete(ext) : next.add(ext)
    return next
  })

  const buildDto = () => ({
    name: form.name.trim(),
    ignorePaths: form.ignorePaths,
    fileTypeExtensions: Array.from(selectedExtensions).sort().join(','),
  })

  const handleSave = () => {
    if (groupNameTaken(form.name)) {
      setGroupError(`You already have a filter group named "${form.name.trim()}".`)
      return
    }
    setGroupError(null)
    saveMutation.mutate()
  }

  const saveMutation = useMutation({
    queryClient,
    mutationFn: () => editingGroup
      ? filterGroupsApi.update(editingGroup.id, buildDto())
      : filterGroupsApi.create(buildDto()),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['filter-groups'] }); closeModal() },
    onError: (e: unknown) => setGroupError((e as { response?: { data?: string } }).response?.data ?? 'A filter group with this name already exists.'),
  })

  const deleteMutation = useMutation({
    queryClient,
    mutationFn: filterGroupsApi.delete,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['filter-groups'] }),
  })

  const users = Array.from(new Set(groups.map(g => g.createdByDisplayName).filter(Boolean)))
  const filtered = groups.filter(g => {
    const matchesUser = !filterUser || g.createdByDisplayName === filterUser
    const matchesSearch = !search || g.name.toLowerCase().includes(search.toLowerCase())
    return matchesUser && matchesSearch
  })

  if (groupsLoading || ftLoading) return <Spinner />

  return (
    <>
      <div className="rounded-xl p-5 space-y-4" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>Filter Groups</h3>
          <button onClick={openCreate} className="btn-primary text-xs">+ New group</button>
        </div>

        <SearchUserFilter
          search={search}
          onSearchChange={setSearch}
          filterUser={filterUser}
          onFilterUserChange={setFilterUser}
          users={users}
          searchPlaceholder="Search by group name…"
        />

        {groups.length === 0 ? (
          <p className="text-sm text-center py-6" style={{ color: '#9aa5ab' }}>No filter groups yet.</p>
        ) : filtered.length === 0 ? (
          <p className="text-sm text-center py-6" style={{ color: '#9aa5ab' }}>No groups match your search.</p>
        ) : (
          <div className="space-y-3">
            {filtered.map(g => (
              <div key={g.id} className="p-4 rounded-lg" style={{ backgroundColor: '#f4f7f9', border: '1px solid #d5d8d9' }}>
                <div className="flex items-center justify-between mb-2">
                  <div>
                    <p className="font-medium text-sm" style={{ color: '#1a2e3b' }}>{g.name}</p>
                    <p className="text-xs" style={{ color: '#9aa5ab' }}>By {g.createdByDisplayName || 'Unknown'}</p>
                  </div>
                  {canEdit(g) && (
                    <div className="flex gap-2">
                      <button onClick={() => openEdit(g)} className="btn-ghost text-xs">Edit</button>
                      <button
                        onClick={() => deleteMutation.mutate(g.id)}
                        className="text-xs px-3 py-1.5 rounded-md font-medium"
                        style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}
                        onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#fbe4e4')}
                        onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                      >
                        Delete
                      </button>
                    </div>
                  )}
                </div>
                <p className="text-xs" style={{ color: '#9aa5ab' }}>
                  Ignore paths: <span className="font-mono" style={{ color: '#1a2e3b' }}>{g.ignorePaths || '—'}</span>
                </p>
                <p className="text-xs mt-1" style={{ color: '#9aa5ab' }}>
                  File types: <span className="font-mono" style={{ color: '#1a2e3b' }}>
                    {g.fileTypeExtensions
                      ? g.fileTypeExtensions.split(',').map(ext => {
                        const ft = fileTypes.find(f => f.extension === ext.trim())
                        return ft ? ft.name : ext.trim()
                      }).join(', ')
                      : 'All'}
                  </span>
                </p>
              </div>
            ))}
          </div>
        )}
      </div>

      {showModal && (
        <Modal
          title={editingGroup ? `Edit: ${editingGroup.name}` : 'New Filter Group'}
          onClose={closeModal}
          sizeClass="max-w-5xl max-h-[85vh] flex flex-col"
          primary
        >
          <div className="mb-4">
            <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Group name</label>
            <input
              className="input"
              value={form.name}
              onChange={e => { setForm(f => ({ ...f, name: e.target.value })); setGroupError(null) }}
              placeholder="e.g. Backend only"
            />
            {groupError && (
              <p className="text-xs mt-1 px-3 py-2 rounded" style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}>{groupError}</p>
            )}
          </div>

          <div className="mb-4">
            <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Ignore paths (comma-separated)</label>
            <input
              className="input"
              value={form.ignorePaths}
              onChange={e => setForm(f => ({ ...f, ignorePaths: e.target.value }))}
              placeholder="e.g. tests/,docs/,migrations/"
            />
          </div>

          <div className="mb-4 flex-1 overflow-y-auto">
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs font-medium" style={{ color: '#6b7f88' }}>
                File types ({selectedExtensions.size} selected — empty = all files)
              </label>
              {selectedExtensions.size > 0 && (
                <button
                  onClick={() => setSelectedExtensions(new Set())}
                  className="text-xs px-3 py-1 rounded-md font-medium"
                  style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}
                  onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#fbe4e4')}
                  onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                >
                  Clear all
                </button>
              )}
            </div>
            {fileTypes.length === 0 ? (
              <p className="text-sm text-center py-6" style={{ color: '#9aa5ab' }}>No file types in the master list. Add some above.</p>
            ) : (
              <div className="grid grid-cols-6 gap-2">
                {[...fileTypes].sort((a, b) => a.name.localeCompare(b.name)).map(f => (
                  <label
                    key={f.id}
                    className="flex items-center gap-2 cursor-pointer px-3 py-2 rounded-lg"
                    style={{
                      backgroundColor: selectedExtensions.has(f.extension) ? '#e8f0f5' : '#f4f7f9',
                      border: `1px solid ${selectedExtensions.has(f.extension) ? '#255876' : '#d5d8d9'}`,
                    }}
                  >
                    <input
                      type="checkbox"
                      checked={selectedExtensions.has(f.extension)}
                      onChange={() => toggleExt(f.extension)}
                      style={{ accentColor: '#255876' }}
                    />
                    <div className="min-w-0 flex items-center gap-1">
                      <p className="text-xs font-medium truncate" style={{ color: '#1a2e3b' }}>{f.name.replace(/\s*\([^)]+\)\s*$/, '')}</p>
                      <p className="text-xs font-mono shrink-0" style={{ color: '#9aa5ab' }}>{f.extension}</p>
                    </div>
                  </label>
                ))}
              </div>
            )}
          </div>

          {selectedExtensions.size > 0 && (
            <div className="mb-4 px-3 py-2 rounded text-xs" style={{ backgroundColor: '#e8f0f5', color: '#255876' }}>
              <span className="font-medium">Selected extensions: </span>
              {Array.from(selectedExtensions).sort().join(', ')}
            </div>
          )}

          <div className="flex gap-2">
            <button onClick={handleSave} disabled={!form.name.trim() || saveMutation.isPending} className="btn-primary">
              {saveMutation.isPending ? 'Saving…' : 'Save group'}
            </button>
            <button onClick={closeModal} className="btn-ghost">Cancel</button>
          </div>
        </Modal>
      )}
    </>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function FilterGroupsPage() {
  return (
    <div>
      <PageHeader
        title="Filter Groups"
        subtitle="Manage the file type master list and create reusable filter groups."
      />
      <div className="space-y-6">
        <FileTypeMasterList />
        <FilterGroupManager />
      </div>
    </div>
  )
}
