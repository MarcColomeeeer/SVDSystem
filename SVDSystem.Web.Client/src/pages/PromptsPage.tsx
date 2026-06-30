import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useAccount } from '@azure/msal-react'
import { promptsApi } from '../api/prompts'
import { useIsAdmin } from '../hooks/useIsAdmin'
import type { PromptTemplate, UpsertPromptTemplateDto } from '../types'
import Modal from '../components/Modal'
import PageHeader from '../components/PageHeader'
import SearchUserFilter from '../components/SearchUserFilter'
import Spinner from '../components/Spinner'

export default function PromptsPage() {
  const queryClient = useQueryClient()
  const isAdmin = useIsAdmin()
  const account = useAccount()
  const myObjectId = (account?.idTokenClaims as Record<string, unknown>)?.oid as string ?? ''

  const { data, isLoading } = useQuery({ queryKey: ['prompts'], queryFn: promptsApi.getAll })

  const [editing, setEditing] = useState<PromptTemplate | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<UpsertPromptTemplateDto>({ name: '', content: '' })
  const [preview, setPreview] = useState<PromptTemplate | null>(null)
  const [search, setSearch] = useState('')
  const [filterUser, setFilterUser] = useState('')

  const saveMutation = useMutation({
    mutationFn: () => editing ? promptsApi.update(editing.id, form) : promptsApi.create(form),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['prompts'] })
      setEditing(null)
      setCreating(false)
    },
  })

  const deleteMutation = useMutation({
    mutationFn: promptsApi.delete,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['prompts'] }),
  })

  const canEdit = (p: PromptTemplate) =>
    isAdmin || (!p.isSystem && p.createdByObjectId === myObjectId)

  const startEdit = (p: PromptTemplate) => {
    setEditing(p)
    setForm({ name: p.name, content: p.content })
    setCreating(false)
    setPreview(null)
  }

  const startCreate = () => {
    setCreating(true)
    setEditing(null)
    setForm({ name: '', content: '' })
    setPreview(null)
  }

  const closeForm = () => { setEditing(null); setCreating(false) }

  if (isLoading) return <Spinner />

  const systemPrompts = data?.filter(p => p.isSystem) ?? []
  const userPrompts = data?.filter(p => !p.isSystem) ?? []
  const users = Array.from(new Set(userPrompts.map(p => p.createdByDisplayName).filter(Boolean)))
  const filteredUserPrompts = userPrompts.filter(p => {
    const matchesSearch = !search || p.name.toLowerCase().includes(search.toLowerCase()) || p.content.toLowerCase().includes(search.toLowerCase())
    const matchesUser = !filterUser || p.createdByDisplayName === filterUser
    return matchesSearch && matchesUser
  })

  return (
    <div>
      <PageHeader
        title="Prompts"
        subtitle="System prompts and reusable custom prompt templates."
      />

      {/* Create / Edit modal */}
      {(creating || editing) && (
        <Modal title={creating ? 'New Prompt' : `Edit: ${editing!.name}`} onClose={closeForm} primary>
          <div className="space-y-4">
            <div>
              <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Name</label>
              <input
                className="input"
                value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
              />
            </div>
            <div>
              <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Additional instructions</label>
              <textarea
                className="input min-h-36 resize-y font-mono text-xs"
                value={form.content}
                onChange={e => setForm(f => ({ ...f, content: e.target.value }))}
              />
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => saveMutation.mutate()}
                disabled={saveMutation.isPending || !form.name.trim() || !form.content.trim()}
                className="btn-primary"
              >
                {saveMutation.isPending ? 'Saving…' : 'Save'}
              </button>
              <button onClick={closeForm} className="btn-ghost">Cancel</button>
            </div>
          </div>
        </Modal>
      )}

      {/* Preview modal */}
      {preview && (
        <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
          <div className="rounded-xl p-6 max-w-2xl w-full max-h-[80vh] overflow-y-auto" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
            <div className="flex justify-between items-center mb-4">
              <h3 className="font-semibold" style={{ color: '#1a2e3b' }}>{preview.name}</h3>
              <button onClick={() => setPreview(null)} className="btn-ghost text-xs">✕ Close</button>
            </div>
            <pre className="text-xs font-mono whitespace-pre-wrap" style={{ color: '#1a2e3b' }}>{preview.content}</pre>
          </div>
        </div>
      )}

      {/* System prompts */}
      <div className="rounded-xl p-5 space-y-3 mb-6" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
        <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>System Prompts</h3>
        {systemPrompts.map(p => (
          <PromptCard
            key={p.id}
            prompt={p}
            canEdit={isAdmin}
            onEdit={() => startEdit(p)}
            onDelete={() => deleteMutation.mutate(p.id)}
            onPreview={() => setPreview(p)}
          />
        ))}
      </div>

      {/* Custom templates */}
      <div className="rounded-xl p-5 space-y-3" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>Custom Templates</h3>
          <button onClick={startCreate} className="btn-primary text-xs">+ New prompt</button>
        </div>

        <SearchUserFilter
          search={search}
          onSearchChange={setSearch}
          filterUser={filterUser}
          onFilterUserChange={setFilterUser}
          users={users}
          searchPlaceholder="Search by name or content…"
        />

        {filteredUserPrompts.map(p => (
          <PromptCard
            key={p.id}
            prompt={p}
            canEdit={canEdit(p)}
            onEdit={() => startEdit(p)}
            onDelete={() => deleteMutation.mutate(p.id)}
            onPreview={() => setPreview(p)}
          />
        ))}

        {filteredUserPrompts.length === 0 && (
          <p className="text-sm" style={{ color: '#6b7f88' }}>
            {userPrompts.length === 0
              ? 'No custom templates yet. Create one above or save one from a repository configuration.'
              : 'No templates match your search.'}
          </p>
        )}
      </div>
    </div>
  )
}

function PromptCard({
  prompt,
  canEdit,
  onEdit,
  onDelete,
  onPreview,
}: {
  prompt: PromptTemplate
  canEdit: boolean
  onEdit: () => void
  onDelete: () => void
  onPreview: () => void
}) {
  return (
    <div className="p-4 rounded-xl" style={{ backgroundColor: '#f4f7f9', border: '1px solid #d5d8d9' }}>
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1">
            <p className="font-medium" style={{ color: '#1a2e3b' }}>{prompt.name}</p>
            {prompt.isSystem && (
              <span className="text-xs px-2 py-0.5 rounded-full font-medium" style={{ backgroundColor: '#e8f0f5', color: '#255876' }}>
                System
              </span>
            )}
          </div>
          <p className="text-xs" style={{ color: '#9aa5ab' }}>
            By <span style={{ color: '#6b7f88' }}>{prompt.isSystem ? 'System' : (prompt.createdByDisplayName || 'Unknown')}</span>
          </p>
          <p className="text-xs font-mono whitespace-pre-wrap line-clamp-2 mt-1" style={{ color: '#6b7f88' }}>
            {prompt.content}
          </p>
        </div>
        <div className="flex gap-2 flex-shrink-0">
          <button onClick={onPreview} className="btn-ghost text-xs">View</button>
          {canEdit && (
            <>
              <button onClick={onEdit} className="btn-ghost text-xs">Edit</button>
              {!prompt.isSystem && (
                <button
                  onClick={onDelete}
                  className="text-xs px-3 py-1.5 rounded-md transition-colors"
                  style={{ color: '#c0392b' }}
                  onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                  onMouseLeave={e => (e.currentTarget.style.backgroundColor = 'transparent')}
                >
                  Delete
                </button>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}
