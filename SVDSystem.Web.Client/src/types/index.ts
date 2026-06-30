// ── Enums ─────────────────────────────────────────────────────────────────────

export type VulnerabilityLevel = 'Low' | 'Medium' | 'High'

// ── Read models (API responses) ───────────────────────────────────────────────

export interface Repository {
  id: string
  repositoryId: string
  repositoryName: string
  projectName: string
  remoteUrl: string
  enabled: boolean
  customPrompt: string | null
  useCategories: boolean
  severityThreshold: VulnerabilityLevel
  vulnerabilityCategories: string
  ignorePaths: string
  fileTypeFilters: string
  includeAddedFiles: boolean
  includeDeletedFiles: boolean
  includeModifiedFiles: boolean
}

export interface PromptTemplate {
  id: string
  name: string
  content: string
  createdByObjectId: string
  createdByDisplayName: string
  isSystem: boolean
}

export interface FilterGroup {
  id: string
  name: string
  ignorePaths: string
  fileTypeExtensions: string
  createdByObjectId: string
  createdByDisplayName: string
}

export interface FileTypeFilter {
  id: string
  name: string
  extension: string
  createdByObjectId: string
  createdByDisplayName: string
}

export interface CategoryGroup {
  id: string
  name: string
  categories: string
  createdByObjectId: string
  createdByDisplayName: string
}

export interface VulnerabilityCategory {
  id: string
  name: string
  createdByObjectId: string
  createdByDisplayName: string
}

export interface UserRepositoryAccess {
  id: string
  userObjectId: string
  userDisplayName: string
  userEmail: string
  repositoryConfigurationId: string
}

// ── Write models (API request bodies) ────────────────────────────────────────

export interface UpdateRepositoryDto {
  enabled: boolean
  customPrompt: string | null
  useCategories: boolean
  severityThreshold: VulnerabilityLevel
  vulnerabilityCategories: string
  ignorePaths: string
  fileTypeFilters: string
  includeAddedFiles: boolean
  includeDeletedFiles: boolean
  includeModifiedFiles: boolean
}

export interface UpsertPromptTemplateDto {
  name: string
  content: string
}

export interface UpsertFilterGroupDto {
  name: string
  ignorePaths: string
  fileTypeExtensions: string
}

export interface UpsertFileTypeFilterDto {
  name: string
  extension: string
}

export interface UpsertCategoryGroupDto {
  name: string
  categories: string
}

export interface UpsertVulnerabilityCategoryDto {
  name: string
}

export interface GrantAccessDto {
  repositoryConfigurationId: string
  userDisplayName: string
  userEmail: string
}
