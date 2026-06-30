import api from './client'
import type { CategoryGroup, UpsertCategoryGroupDto } from '../types'

export const categoryGroupsApi = {
  getAll: () => api.get<CategoryGroup[]>('/category-groups').then(r => r.data),
  getById: (id: string) => api.get<CategoryGroup>(`/category-groups/${id}`).then(r => r.data),
  create: (dto: UpsertCategoryGroupDto) => api.post<CategoryGroup>('/category-groups', dto).then(r => r.data),
  update: (id: string, dto: UpsertCategoryGroupDto) =>
    api.put<CategoryGroup>(`/category-groups/${id}`, dto).then(r => r.data),
  delete: (id: string) => api.delete(`/category-groups/${id}`),
}
