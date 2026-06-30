import api from './client'
import type { FilterGroup, UpsertFilterGroupDto } from '../types'

export const filterGroupsApi = {
  getAll: () => api.get<FilterGroup[]>('/filter-groups').then(r => r.data),
  getById: (id: string) => api.get<FilterGroup>(`/filter-groups/${id}`).then(r => r.data),
  create: (dto: UpsertFilterGroupDto) => api.post<FilterGroup>('/filter-groups', dto).then(r => r.data),
  update: (id: string, dto: UpsertFilterGroupDto) =>
    api.put<FilterGroup>(`/filter-groups/${id}`, dto).then(r => r.data),
  delete: (id: string) => api.delete(`/filter-groups/${id}`),
}
