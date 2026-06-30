import api from './client'
import type { FileTypeFilter, UpsertFileTypeFilterDto } from '../types'

export const fileTypeFiltersApi = {
  getAll: () => api.get<FileTypeFilter[]>('/file-type-filters').then(r => r.data),
  create: (dto: UpsertFileTypeFilterDto) => api.post<FileTypeFilter>('/file-type-filters', dto).then(r => r.data),
  update: (id: string, dto: UpsertFileTypeFilterDto) =>
    api.put<FileTypeFilter>(`/file-type-filters/${id}`, dto).then(r => r.data),
  delete: (id: string) => api.delete(`/file-type-filters/${id}`),
}
