import React, { useEffect, useState } from 'react'
import { useParams, useNavigate, useBlocker } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { repositoriesApi } from '../api/repositories'
import { promptsApi } from '../api/prompts'
import { filterGroupsApi } from '../api/filterGroups'
import { fileTypeFiltersApi } from '../api/fileTypeFilters'
import { categoryGroupsApi } from '../api/categoryGroups'
import { vulnerabilityCategoriesApi } from '../api/vulnerabilityCategories'
import type { CategoryGroup, FileTypeFilter, FilterGroup, PromptTemplate, UpdateRepositoryDto, VulnerabilityCategory, VulnerabilityLevel } from '../types'
import PageHeader from '../components/PageHeader'
import ReadonlyField from '../components/ReadonlyField'
import Spinner from '../components/Spinner'

const SEVERITY_LEVELS: VulnerabilityLevel[] = ['Low', 'Medium', 'High']

export default function RepositoryDetailPage() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()
    const queryClient = useQueryClient()

    const { data: repo, isLoading } = useQuery({ queryKey: ['repositories', id], queryFn: () => repositoriesApi.getById(id!) })
    const { data: prompts = [] } = useQuery({ queryKey: ['prompts'], queryFn: promptsApi.getAll })
    const { data: filterGroups = [] } = useQuery({ queryKey: ['filter-groups'], queryFn: filterGroupsApi.getAll })
    const { data: allFileTypes = [] } = useQuery({ queryKey: ['file-type-filters'], queryFn: fileTypeFiltersApi.getAll })
    const { data: categoryGroups = [] } = useQuery({ queryKey: ['category-groups'], queryFn: categoryGroupsApi.getAll })
    const { data: allVulnCategories = [] } = useQuery({ queryKey: ['vulnerability-categories'], queryFn: vulnerabilityCategoriesApi.getAll })

    const [form, setForm] = useState<UpdateRepositoryDto | null>(null)
    const [showPromptPicker, setShowPromptPicker] = useState(false)
    const [showSaveTemplate, setShowSaveTemplate] = useState(false)
    const [templateName, setTemplateName] = useState('')
    const [savedTemplateName, setSavedTemplateName] = useState<string | null>(null)
    const [showCategoryPicker, setShowCategoryPicker] = useState(false)
    const [showSaveCategoryGroup, setShowSaveCategoryGroup] = useState(false)
    const [categoryGroupName, setCategoryGroupName] = useState('')
    const [savedCategoryGroupName, setSavedCategoryGroupName] = useState<string | null>(null)
    const [showFileTypePicker, setShowFileTypePicker] = useState(false)
    const [showSaveFilterGroup, setShowSaveFilterGroup] = useState(false)
    const [filterGroupName, setFilterGroupName] = useState('')
    const [savedFilterGroupName, setSavedFilterGroupName] = useState<string | null>(null)
    const [isDirty, setIsDirty] = useState(false)
    const [formErrors, setFormErrors] = useState<{ fileToggles?: string; categories?: string }>({})

    const blocker = useBlocker(({ currentLocation, nextLocation }) =>
        isDirty && currentLocation.pathname !== nextLocation.pathname
    )

    useEffect(() => {
        if (repo) {
            setForm({
                enabled: repo.enabled,
                customPrompt: repo.customPrompt,
                useCategories: repo.useCategories,
                severityThreshold: repo.severityThreshold,
                vulnerabilityCategories: repo.vulnerabilityCategories,
                ignorePaths: repo.ignorePaths,
                fileTypeFilters: repo.fileTypeFilters,
                includeAddedFiles: repo.includeAddedFiles,
                includeDeletedFiles: repo.includeDeletedFiles,
                includeModifiedFiles: repo.includeModifiedFiles,
            })
            setIsDirty(false)
            setFormErrors({})
        }
    }, [repo])

    const mutation = useMutation({
        mutationFn: (dto: UpdateRepositoryDto) => repositoriesApi.update(id!, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['repositories'] })
            setIsDirty(false)
            // If we got here via the blocker modal, proceed to the intended destination;
            // otherwise just go back to repositories.
            if (blocker.state === 'blocked') {
                blocker.proceed()
            } else {
                navigate('/repositories')
            }
        },
    })
    const saveTemplateMutation = useMutation({
        mutationFn: () => repositoriesApi.savePromptAsTemplate(id!, templateName),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ['prompts'] })
            setSavedTemplateName(data.name)
            setShowSaveTemplate(false)
            setTemplateName('')
        },
    })

    const saveCategoryGroupMutation = useMutation({
        mutationFn: () => repositoriesApi.saveCategoriesAsGroup(id!, categoryGroupName),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ['category-groups'] })
            setSavedCategoryGroupName(data.name)
            setShowSaveCategoryGroup(false)
            setCategoryGroupName('')
        },
    })

    const saveFilterGroupMutation = useMutation({
        mutationFn: () => filterGroupsApi.create({
            name: filterGroupName,
            ignorePaths: form!.ignorePaths,
            fileTypeExtensions: form!.fileTypeFilters,
        }),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ['filter-groups'] })
            setSavedFilterGroupName(data.name)
            setShowSaveFilterGroup(false)
            setFilterGroupName('')
        },
    })

    if (isLoading || !form) return <Spinner />

    const set = <K extends keyof UpdateRepositoryDto>(key: K, value: UpdateRepositoryDto[K]) => {
        setForm(f => f ? { ...f, [key]: value } : f)
        setIsDirty(true)
        setFormErrors(e => ({ ...e, [key === 'includeAddedFiles' || key === 'includeDeletedFiles' || key === 'includeModifiedFiles' ? 'fileToggles' : key === 'vulnerabilityCategories' ? 'categories' : '']: undefined }))
    }

    const validate = (): boolean => {
        const errors: { fileToggles?: string; categories?: string } = {}
        if (!form.includeAddedFiles && !form.includeDeletedFiles && !form.includeModifiedFiles)
            errors.fileToggles = 'At least one file change type must be included.'
        if (form.useCategories && !form.vulnerabilityCategories.trim())
            errors.categories = 'At least one category must be selected when using Specific categories only mode.'
        setFormErrors(errors)
        return Object.keys(errors).length === 0
    }

    const handleSave = () => {
        if (!validate()) return
        mutation.mutate(form)
    }

    // Custom prompts only (not system prompts) are available in the picker
    const customPrompts = prompts.filter(p => !p.isSystem)

    return (
        <div>
            <PageHeader
                title={repo!.repositoryName}
                subtitle={repo!.projectName}
                actions={
                        <button onClick={() => navigate('/repositories')} className="btn-ghost px-3 py-1.5 text-sm">
                                ← Back
                            </button>
                    }
            />

            {/* Unsaved changes modal */}
            {blocker.state === 'blocked' && (
                <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
                    <div className="rounded-xl p-6 max-w-sm w-full" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
                        <h3 className="font-semibold mb-2" style={{ color: '#1a2e3b' }}>Unsaved changes</h3>
                        <p className="text-sm mb-4" style={{ color: '#6b7f88' }}>You have unsaved changes. Do you want to save before leaving?</p>
                        <div className="flex gap-2">
                            <button
                                onClick={() => {
                                    if (!validate()) {
                                        blocker.reset()
                                        return
                                    }
                                    mutation.mutate(form)
                                }}
                                disabled={mutation.isPending}
                                className="btn-primary"
                            >
                                {mutation.isPending ? 'Saving…' : 'Save'}
                            </button>
                            <button
                                onClick={() => { setIsDirty(false); blocker.proceed() }}
                                className="text-xs px-3 py-1.5 rounded-md font-medium"
                                style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}
                                onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#fbe4e4')}
                                onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                            >Discard</button>
                            <button onClick={() => blocker.reset()} className="btn-ghost">Cancel</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Prompt picker modal */}
            {showPromptPicker && (
                <PromptPickerModal
                    prompts={customPrompts}
                    onSelect={content => { set('customPrompt', content); setShowPromptPicker(false) }}
                    onClose={() => setShowPromptPicker(false)}
                />
            )}

            {/* Save as template modal */}
            {showSaveTemplate && (
                <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
                    <div className="rounded-xl p-6 max-w-sm w-full" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
                        <h3 className="font-semibold mb-3" style={{ color: '#1a2e3b' }}>Save as template</h3>
                        <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Template name</label>
                        <input className="input mb-4" value={templateName} onChange={e => setTemplateName(e.target.value)} placeholder="e.g. My API security prompt" />
                        <div className="flex gap-2">
                            <button onClick={() => saveTemplateMutation.mutate()} disabled={!templateName.trim() || saveTemplateMutation.isPending} className="btn-primary">
                                {saveTemplateMutation.isPending ? 'Saving…' : 'Save'}
                            </button>
                            <button onClick={() => setShowSaveTemplate(false)} className="btn-ghost">Cancel</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Save as category group modal */}
            {showSaveCategoryGroup && (
                <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
                    <div className="rounded-xl p-6 max-w-sm w-full" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
                        <h3 className="font-semibold mb-3" style={{ color: '#1a2e3b' }}>Save as category group</h3>
                        <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Group name</label>
                        <input className="input mb-4" value={categoryGroupName} onChange={e => setCategoryGroupName(e.target.value)} placeholder="e.g. OWASP Top 10" />
                        <div className="flex gap-2">
                            <button onClick={() => saveCategoryGroupMutation.mutate()} disabled={!categoryGroupName.trim() || saveCategoryGroupMutation.isPending} className="btn-primary">
                                {saveCategoryGroupMutation.isPending ? 'Saving…' : 'Save'}
                            </button>
                            <button onClick={() => setShowSaveCategoryGroup(false)} className="btn-ghost">Cancel</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Category picker modal */}
            {showCategoryPicker && form && (
                <CategoryPickerModal
                    allCategories={allVulnCategories}
                    groups={categoryGroups}
                    current={form.vulnerabilityCategories}
                    onApply={cats => { set('vulnerabilityCategories', cats); setShowCategoryPicker(false) }}
                    onClose={() => setShowCategoryPicker(false)}
                />
            )}

            {/* File type picker modal */}
            {showFileTypePicker && form && (
                <FileTypePickerModal
                    allFileTypes={allFileTypes}
                    filterGroups={filterGroups}
                    current={form.fileTypeFilters}
                    onApply={exts => { set('fileTypeFilters', exts); setShowFileTypePicker(false) }}
                    onClose={() => setShowFileTypePicker(false)}
                />
            )}

            {/* Save as filter group modal */}
            {showSaveFilterGroup && (
                <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
                    <div className="rounded-xl p-6 max-w-sm w-full" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
                        <h3 className="font-semibold mb-3" style={{ color: '#1a2e3b' }}>Save as filter group</h3>
                        <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>Group name</label>
                        <input className="input mb-4" value={filterGroupName} onChange={e => setFilterGroupName(e.target.value)} placeholder="e.g. Backend files" />
                        <div className="flex gap-2">
                            <button onClick={() => saveFilterGroupMutation.mutate()} disabled={!filterGroupName.trim() || saveFilterGroupMutation.isPending} className="btn-primary">
                                {saveFilterGroupMutation.isPending ? 'Saving…' : 'Save'}
                            </button>
                            <button onClick={() => setShowSaveFilterGroup(false)} className="btn-ghost">Cancel</button>
                        </div>
                    </div>
                </div>
            )}

            <div className="space-y-6">
                {/* Read-only info */}
                <Section title="Repository Info">
                    <ReadonlyField label="Repository ID" value={repo!.repositoryId} />
                    <ReadonlyField label="Repository Name" value={repo!.repositoryName} />
                    <ReadonlyField label="Project Name" value={repo!.projectName} />
                    <ReadonlyField label="Remote URL" value={repo!.remoteUrl} />
                </Section>

                {/* Editable settings */}
                <Section title="Analysis Settings">
                    <Toggle label="Enabled" checked={form.enabled} onChange={v => set('enabled', v)} />

                    <Field label="Severity Threshold">
                        <select className="input" value={form.severityThreshold} onChange={e => set('severityThreshold', e.target.value as VulnerabilityLevel)}>
                            {SEVERITY_LEVELS.map(l => <option key={l}>{l}</option>)}
                        </select>
                    </Field>

                    {/* Base prompt selection */}
                    <Field label="Analysis Mode">
                        <div className="flex gap-3">
                            <label className="flex items-center gap-2 cursor-pointer">
                                <input type="radio" name="useCategories" checked={!form.useCategories} onChange={() => set('useCategories', false)} style={{ accentColor: '#255876' }} />
                                <span className="text-sm" style={{ color: '#1a2e3b' }}>Any vulnerability</span>
                            </label>
                            <label className="flex items-center gap-2 cursor-pointer">
                                <input type="radio" name="useCategories" checked={form.useCategories} onChange={() => set('useCategories', true)} style={{ accentColor: '#255876' }} />
                                <span className="text-sm" style={{ color: '#1a2e3b' }}>Specific categories only</span>
                            </label>
                        </div>
                        <p className="text-xs mt-1" style={{ color: '#9aa5ab' }}>
                            {form.useCategories ? 'Uses the Category-Focused system prompt.' : 'Uses the General Vulnerability Analysis system prompt.'}
                        </p>
                    </Field>

                    {/* Custom prompt */}
                    <Field label="Additional Instructions (Custom Prompt)">
                        <textarea
                            className="input min-h-24 resize-y"
                            value={form.customPrompt ?? ''}
                            placeholder="Optional extra instructions appended to the system prompt…"
                            onChange={e => set('customPrompt', e.target.value || null)}
                        />
                        <div className="flex gap-2 mt-2">
                            <button
                                type="button"
                                onClick={() => setShowPromptPicker(true)}
                                className="text-xs px-3 py-1.5 rounded-md font-medium transition-colors text-white"
                                style={{ backgroundColor: '#255876' }}
                                onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#1d4460')}
                                onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#255876')}
                            >
                                Load from template
                            </button>
                            {form.customPrompt?.trim() ? (
                                <button
                                    type="button"
                                    onClick={() => setShowSaveTemplate(true)}
                                    className="text-xs px-3 py-1.5 rounded-md font-medium transition-colors text-white"
                                    style={{ backgroundColor: '#255876' }}
                                    onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#1d4460')}
                                    onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#255876')}
                                >
                                    Save as template
                                </button>
                            ) : (
                                <button
                                    type="button"
                                    disabled
                                    className="text-xs px-3 py-1.5 rounded-md font-medium text-white"
                                    style={{ backgroundColor: '#255876', opacity: 0.4, cursor: 'not-allowed' }}
                                >
                                    Save as template
                                </button>
                            )}
                            {form.customPrompt?.trim() ? (
                                <button
                                    type="button"
                                    onClick={() => set('customPrompt', '')}
                                    className="text-xs px-3 py-1.5 rounded-md font-medium"
                                    style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}
                                    onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#fbe4e4')}
                                    onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                                >
                                    Clear
                                </button>
                            ) : (
                                <button
                                    type="button"
                                    disabled
                                    className="text-xs px-3 py-1.5 rounded-md font-medium"
                                    style={{ backgroundColor: '#fdf2f2', color: '#c0392b', opacity: 0.4, cursor: 'not-allowed' }}
                                >
                                    Clear
                                </button>
                            )}
                        </div>
                        {savedTemplateName && (
                                            <p className="text-xs mt-1" style={{ color: '#255876' }}>✓ Saved as "{savedTemplateName}"</p>
                                        )}
                    </Field>
                </Section>

                <Section title="Vulnerability Categories">
                    {!form.useCategories ? (
                        <p className="text-sm" style={{ color: '#9aa5ab' }}>
                            Not used — switch to <strong>Specific categories only</strong> mode to configure this.
                        </p>
                    ) : (
                        <>
                            {/* Selected categories as pills */}
                            {form.vulnerabilityCategories.trim() && (
                                <div className="flex flex-wrap gap-1.5">
                                    {form.vulnerabilityCategories.split(',').filter(Boolean).map(c => (
                                        <span key={c} className="text-xs px-2 py-0.5 rounded-full" style={{ backgroundColor: '#e8f0f5', color: '#255876' }}>
                                            {c.trim()}
                                        </span>
                                    ))}
                                </div>
                            )}
                            {!form.vulnerabilityCategories.trim() && (
                                <p className="text-xs" style={{ color: '#9aa5ab' }}>No categories selected.</p>
                            )}
                            {formErrors.categories && (
                                <p className="text-xs px-3 py-2 rounded" style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}>{formErrors.categories}</p>
                            )}

                            <div className="flex gap-2">
                                <button
                                    type="button"
                                    onClick={() => setShowCategoryPicker(true)}
                                    className="text-xs px-3 py-1.5 rounded-md font-medium text-white"
                                    style={{ backgroundColor: '#255876' }}
                                    onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#1d4460')}
                                    onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#255876')}
                                >
                                    Select / load categories
                                </button>
                                {form.vulnerabilityCategories.trim() ? (
                                    <button
                                        type="button"
                                        onClick={() => setShowSaveCategoryGroup(true)}
                                        className="text-xs px-3 py-1.5 rounded-md font-medium text-white"
                                        style={{ backgroundColor: '#255876' }}
                                        onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#1d4460')}
                                        onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#255876')}
                                    >
                                        Save as group
                                    </button>
                                ) : (
                                    <button
                                        type="button"
                                        disabled
                                        className="text-xs px-3 py-1.5 rounded-md font-medium text-white"
                                        style={{ backgroundColor: '#255876', opacity: 0.4, cursor: 'not-allowed' }}
                                    >
                                        Save as group
                                    </button>
                                )}
                                {form.vulnerabilityCategories.trim() ? (
                                    <button
                                        type="button"
                                        onClick={() => set('vulnerabilityCategories', '')}
                                        className="text-xs px-3 py-1.5 rounded-md font-medium"
                                        style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}
                                        onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#fbe4e4')}
                                        onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                                    >
                                        Clear all
                                    </button>
                                ) : (
                                    <button
                                        type="button"
                                        disabled
                                        className="text-xs px-3 py-1.5 rounded-md font-medium"
                                        style={{ backgroundColor: '#fdf2f2', color: '#c0392b', opacity: 0.4, cursor: 'not-allowed' }}
                                    >
                                        Clear all
                                    </button>
                                )}
                            </div>
                            {savedCategoryGroupName && (
                                <p className="text-xs" style={{ color: '#255876' }}>? Saved as "{savedCategoryGroupName}"</p>
                            )}
                        </>
                    )}
                </Section>

                <Section title="File Filters">
                    {/* Ignore paths — free text */}
                    <Field label="Ignore Paths (comma-separated)">
                        <input className="input" value={form.ignorePaths} onChange={e => set('ignorePaths', e.target.value)} placeholder="e.g. tests/,docs/,migrations/" />
                    </Field>

                    {/* File type selection */}
                    <div>
                        <label className="block text-xs font-medium mb-2" style={{ color: '#6b7f88' }}>File Types</label>
                        {/* Selected pills */}
                        {form.fileTypeFilters.trim() && (
                            <div className="flex flex-wrap gap-1.5 mb-2">
                                {form.fileTypeFilters.split(',').map(s => s.trim()).filter(Boolean).map(ext => {
                                    const ft = allFileTypes.find(f => f.extension === ext)
                                    return (
                                        <span key={ext} className="text-xs px-2 py-1 rounded-md font-medium"
                                            style={{ backgroundColor: '#e8f0f5', color: '#255876' }}>
                                            {ft ? `${ft.name} (${ft.extension})` : ext}
                                        </span>
                                    )
                                })}
                            </div>
                        )}
                        <div className="flex gap-2 flex-wrap">
                            <button
                                type="button"
                                onClick={() => setShowFileTypePicker(true)}
                                className="text-xs px-3 py-1.5 rounded-md font-medium text-white"
                                style={{ backgroundColor: '#255876' }}
                                onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#1d4460')}
                                onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#255876')}
                            >
                                Select / load file types
                            </button>
                            {form.fileTypeFilters.trim() ? (
                                <button
                                    type="button"
                                    onClick={() => setShowSaveFilterGroup(true)}
                                    className="text-xs px-3 py-1.5 rounded-md font-medium text-white"
                                    style={{ backgroundColor: '#255876' }}
                                    onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#1d4460')}
                                    onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#255876')}
                                >
                                    Save as group
                                </button>
                            ) : (
                                <button
                                    type="button"
                                    disabled
                                    className="text-xs px-3 py-1.5 rounded-md font-medium text-white"
                                    style={{ backgroundColor: '#255876', opacity: 0.4, cursor: 'not-allowed' }}
                                >
                                    Save as group
                                </button>
                            )}
                            {form.fileTypeFilters.trim() ? (
                                <button
                                    type="button"
                                    onClick={() => set('fileTypeFilters', '')}
                                    className="text-xs px-3 py-1.5 rounded-md font-medium"
                                    style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}
                                    onMouseEnter={e => (e.currentTarget.style.backgroundColor = '#fbe4e4')}
                                    onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                                >
                                    Clear all
                                </button>
                            ) : (
                                <button
                                    type="button"
                                    disabled
                                    className="text-xs px-3 py-1.5 rounded-md font-medium"
                                    style={{ backgroundColor: '#fdf2f2', color: '#c0392b', opacity: 0.4, cursor: 'not-allowed' }}
                                >
                                    Clear all
                                </button>
                            )}
                        </div>
                        {savedFilterGroupName && (
                            <p className="text-xs mt-1" style={{ color: '#255876' }}>✓ Saved as "{savedFilterGroupName}"</p>
                        )}
                    </div>

                    <div className="flex gap-6">
                        <Toggle label="Include Added Files" checked={form.includeAddedFiles} onChange={v => set('includeAddedFiles', v)} />
                        <Toggle label="Include Deleted Files" checked={form.includeDeletedFiles} onChange={v => set('includeDeletedFiles', v)} />
                        <Toggle label="Include Modified Files" checked={form.includeModifiedFiles} onChange={v => set('includeModifiedFiles', v)} />
                    </div>
                    {formErrors.fileToggles && (
                        <p className="text-xs px-3 py-2 rounded" style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}>{formErrors.fileToggles}</p>
                    )}
                </Section>

                <div className="flex gap-3 pt-2">
                    <button
                        onClick={handleSave}
                        disabled={mutation.isPending}
                        className="btn-primary px-5 py-2"
                    >
                        {mutation.isPending ? 'Saving…' : 'Save changes'}
                    </button>
                    <button onClick={() => navigate('/repositories')} className="btn-ghost px-5 py-2 text-sm">
                        Cancel
                    </button>
                </div>

                {mutation.isError && <p className="text-sm" style={{ color: '#c0392b' }}>Failed to save changes.</p>}
            </div>
        </div>
    )
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
    return (
        <div className="rounded-xl p-5 space-y-4" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
            <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>{title}</h3>
            {children}
        </div>
    )
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <div>
            <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>{label}</label>
            {children}
        </div>
    )
}

function Toggle({ label, checked, onChange }: { label: string; checked: boolean; onChange: (v: boolean) => void }) {
    return (
        <label className="flex items-center gap-2 cursor-pointer select-none">
            <div
                onClick={() => onChange(!checked)}
                className="w-9 h-5 rounded-full relative transition-colors"
                style={{ backgroundColor: checked ? '#255876' : '#d5d8d9' }}
            >
                <span className={`absolute top-0.5 left-0.5 w-4 h-4 bg-white rounded-full shadow transition-transform ${checked ? 'translate-x-4' : ''}`} />
            </div>
            <span className="text-sm" style={{ color: '#1a2e3b' }}>{label}</span>
        </label>
    )
}

function PromptPickerModal({ prompts, onSelect, onClose }: {
    prompts: PromptTemplate[]
    onSelect: (content: string) => void
    onClose: () => void
}) {
    const [search, setSearch] = useState('')
    const [filterUser, setFilterUser] = useState('')

    const users = Array.from(new Set(prompts.map(p => p.createdByDisplayName).filter(Boolean)))

    const filtered = prompts.filter(p => {
        const matchesSearch = !search || p.name.toLowerCase().includes(search.toLowerCase()) || p.content.toLowerCase().includes(search.toLowerCase())
        const matchesUser = !filterUser || p.createdByDisplayName === filterUser
        return matchesSearch && matchesUser
    })

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
            <div className="rounded-xl p-6 max-w-lg w-full max-h-[80vh] flex flex-col" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
                <div className="flex justify-between items-center mb-4">
                    <h3 className="font-semibold" style={{ color: '#1a2e3b' }}>Select a prompt template</h3>
                    <button onClick={onClose} className="btn-ghost text-xs">✕ Close</button>
                </div>

                {/* Filters */}
                <div className="flex gap-2 mb-4">
                    <input
                        className="input flex-1"
                        placeholder="Search by name or content…"
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                    />
                    <select className="input w-40" value={filterUser} onChange={e => setFilterUser(e.target.value)}>
                        <option value="">All users</option>
                        {users.map(u => <option key={u} value={u}>{u}</option>)}
                    </select>
                </div>

                {/* List */}
                <div className="overflow-y-auto space-y-2 flex-1">
                    {filtered.length === 0 && (
                        <p className="text-sm text-center py-8" style={{ color: '#9aa5ab' }}>No templates match your search.</p>
                    )}
                    {filtered.map(p => (
                        <button
                            key={p.id}
                            onClick={() => onSelect(p.content)}
                            className="w-full text-left p-3 rounded-lg transition-colors"
                            style={{ border: '1px solid #d5d8d9' }}
                            onMouseEnter={e => (e.currentTarget.style.borderColor = '#255876')}
                            onMouseLeave={e => (e.currentTarget.style.borderColor = '#d5d8d9')}
                        >
                            <p className="font-medium text-sm" style={{ color: '#1a2e3b' }}>{p.name}</p>
                            <p className="text-xs mt-0.5" style={{ color: '#9aa5ab' }}>By {p.createdByDisplayName || 'Unknown'}</p>
                            <p className="text-xs font-mono line-clamp-2 mt-1" style={{ color: '#6b7f88' }}>{p.content}</p>
                        </button>
                    ))}
                </div>
            </div>
        </div>
    )
}

function CategoryPickerModal({ allCategories, groups, current, onApply, onClose }: {
    allCategories: VulnerabilityCategory[]
    groups: CategoryGroup[]
    current: string
    onApply: (categories: string) => void
    onClose: () => void
}) {
    const initialSelected = new Set(current.split(',').map(s => s.trim()).filter(Boolean))
    const [selected, setSelected] = useState<Set<string>>(initialSelected)
    const [filterUser, setFilterUser] = useState('')

    const toggle = (name: string) => setSelected(prev => {
        const next = new Set(prev)
        next.has(name) ? next.delete(name) : next.add(name)
        return next
    })

    const loadGroup = (g: CategoryGroup) => {
        const names = g.categories.split(',').map(s => s.trim()).filter(Boolean)
        setSelected(new Set(names))
    }

    const users = Array.from(new Set(groups.map(g => g.createdByDisplayName).filter(Boolean)))
    const filteredGroups = filterUser ? groups.filter(g => g.createdByDisplayName === filterUser) : groups
    const preview = Array.from(selected).sort().join(', ')

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
            <div className="rounded-xl p-6 w-full max-w-4xl max-h-[85vh] flex flex-col" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
                <div className="flex justify-between items-center mb-4">
                    <h3 className="font-semibold" style={{ color: '#1a2e3b' }}>Select vulnerability categories</h3>
                    <button onClick={onClose} className="btn-ghost text-xs">✕ Close</button>
                </div>

                {/* Load from group */}
                {groups.length > 0 && (
                    <div className="mb-4 p-3 rounded-lg" style={{ backgroundColor: '#f4f7f9', border: '1px solid #d5d8d9' }}>
                        <div className="flex items-center gap-2 mb-2">
                            <p className="text-xs font-medium" style={{ color: '#6b7f88' }}>Load from a saved group:</p>
                            <select className="input text-xs w-36" value={filterUser} onChange={e => setFilterUser(e.target.value)}>
                                <option value="">All users</option>
                                {users.map(u => <option key={u} value={u}>{u}</option>)}
                            </select>
                        </div>
                        <div className="flex flex-wrap gap-2">
                            {filteredGroups.map(g => (
                                <button
                                    key={g.id}
                                    onClick={() => loadGroup(g)}
                                    className="text-xs px-3 py-1 rounded-md font-medium"
                                    style={{ backgroundColor: '#e8f0f5', color: '#255876' }}
                                >
                                    {g.name}
                                </button>
                            ))}
                        </div>
                    </div>
                )}

                {/* Category checkboxes — 4 columns */}
                <div className="flex-1 overflow-y-auto">
                    {allCategories.length === 0 ? (
                        <p className="text-sm text-center py-8" style={{ color: '#9aa5ab' }}>
                            No categories in the master list. Go to Category Groups to add some.
                        </p>
                    ) : (
                        <div className="grid grid-cols-4 gap-2">
                            {[...allCategories].sort((a, b) => a.name.localeCompare(b.name)).map(c => (
                                <label
                                    key={c.id}
                                    className="flex items-center gap-2 cursor-pointer px-3 py-2 rounded-lg"
                                    style={{
                                        backgroundColor: selected.has(c.name) ? '#e8f0f5' : '#f4f7f9',
                                        border: `1px solid ${selected.has(c.name) ? '#255876' : '#d5d8d9'}`,
                                    }}
                                >
                                    <input
                                        type="checkbox"
                                        checked={selected.has(c.name)}
                                        onChange={() => toggle(c.name)}
                                        style={{ accentColor: '#255876' }}
                                    />
                                    <span className="text-sm" style={{ color: '#1a2e3b' }}>{c.name}</span>
                                </label>
                            ))}
                        </div>
                    )}
                </div>

                {/* Preview & actions */}
                {selected.size > 0 && (
                    <div className="mt-3 text-xs px-3 py-2 rounded" style={{ backgroundColor: '#e8f0f5', color: '#255876' }}>
                        <span className="font-medium">Selected ({selected.size}): </span>{preview}
                    </div>
                )}
                <div className="flex gap-2 mt-4">
                    <button onClick={() => onApply(preview)} disabled={selected.size === 0} className="btn-primary">
                        Apply selection
                    </button>
                    <button
                        onClick={() => setSelected(new Set())}
                        disabled={selected.size === 0}
                        className="text-xs px-3 py-1.5 rounded-md font-medium disabled:opacity-40"
                        style={{ backgroundColor: '#fdf2f2', color: '#c0392b' }}
                        onMouseEnter={e => { if (selected.size > 0) e.currentTarget.style.backgroundColor = '#fbe4e4' }}
                        onMouseLeave={e => (e.currentTarget.style.backgroundColor = '#fdf2f2')}
                    >
                        Clear all
                    </button>
                </div>
            </div>
        </div>
    )
}

function FileTypePickerModal({ allFileTypes, filterGroups, current, onApply, onClose }: {
    allFileTypes: FileTypeFilter[]
    filterGroups: FilterGroup[]
    current: string
    onApply: (extensions: string) => void
    onClose: () => void
}) {
    const initialSelected = new Set(current.split(',').map(s => s.trim()).filter(Boolean))
    const [selected, setSelected] = useState<Set<string>>(initialSelected)
    const [filterUser, setFilterUser] = useState('')

    const toggle = (ext: string) => setSelected(prev => {
        const next = new Set(prev)
        next.has(ext) ? next.delete(ext) : next.add(ext)
        return next
    })

    const loadGroup = (g: FilterGroup) => {
        const exts = g.fileTypeExtensions.split(',').map(s => s.trim()).filter(Boolean)
        setSelected(new Set(exts))
    }

    const users = Array.from(new Set(filterGroups.map(g => g.createdByDisplayName).filter(Boolean)))
    const filteredGroups = filterUser ? filterGroups.filter(g => g.createdByDisplayName === filterUser) : filterGroups

    const preview = Array.from(selected).sort().join(',')

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
            <div className="rounded-xl p-6 w-full max-w-5xl max-h-[85vh] flex flex-col" style={{ backgroundColor: '#ffffff', border: '1px solid #d5d8d9' }}>
                <div className="flex justify-between items-center mb-4">
                    <h3 className="font-semibold" style={{ color: '#1a2e3b' }}>Select file types</h3>
                    <button onClick={onClose} className="btn-ghost text-xs">✕ Close</button>
                </div>

                {/* Load from group */}
                {filterGroups.length > 0 && (
                    <div className="mb-4 p-3 rounded-lg" style={{ backgroundColor: '#f4f7f9', border: '1px solid #d5d8d9' }}>
                        <div className="flex items-center gap-2 mb-2">
                            <p className="text-xs font-medium" style={{ color: '#6b7f88' }}>Load from a saved group:</p>
                            <select className="input text-xs w-36" value={filterUser} onChange={e => setFilterUser(e.target.value)}>
                                <option value="">All users</option>
                                {users.map(u => <option key={u} value={u}>{u}</option>)}
                            </select>
                        </div>
                        <div className="flex flex-wrap gap-2">
                            {filteredGroups.map(g => (
                                <button
                                    key={g.id}
                                    onClick={() => loadGroup(g)}
                                    className="text-xs px-3 py-1 rounded-md font-medium"
                                    style={{ backgroundColor: '#e8f0f5', color: '#255876' }}
                                >
                                    {g.name}
                                </button>
                            ))}
                        </div>
                    </div>
                )}

                {/* File type checkboxes — 6 columns */}
                <div className="flex-1 overflow-y-auto">
                    {allFileTypes.length === 0 ? (
                        <p className="text-sm text-center py-8" style={{ color: '#9aa5ab' }}>
                            No file types in the master list. Go to Filter Groups to add some.
                        </p>
                    ) : (
                        <div className="grid grid-cols-6 gap-2">
                            {[...allFileTypes].sort((a, b) => a.name.localeCompare(b.name)).map(f => (
                                <label
                                    key={f.id}
                                    className="flex items-center gap-2 cursor-pointer px-3 py-2 rounded-lg"
                                    style={{
                                        backgroundColor: selected.has(f.extension) ? '#e8f0f5' : '#f4f7f9',
                                        border: `1px solid ${selected.has(f.extension) ? '#255876' : '#d5d8d9'}`,
                                    }}
                                >
                                    <input
                                        type="checkbox"
                                        checked={selected.has(f.extension)}
                                        onChange={() => toggle(f.extension)}
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

                {/* Preview */}
                {selected.size > 0 && (
                    <div className="mt-3 text-xs px-3 py-2 rounded" style={{ backgroundColor: '#e8f0f5', color: '#255876' }}>
                        <span className="font-medium">Selected ({selected.size}): </span>
                        {Array.from(selected).sort().map(ext => {
                            const ft = allFileTypes.find(f => f.extension === ext)
                            return ft ? ft.name : ext
                        }).join(', ')}
                    </div>
                )}

                <div className="flex gap-2 mt-4">
                    <button
                        onClick={() => onApply(preview)}
                        className="btn-primary"
                    >
                        Apply selection
                    </button>
                    {selected.size > 0 ? (
                        <button
                            onClick={() => setSelected(new Set())}
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
                </div>
            </div>
        </div>
    )
}
