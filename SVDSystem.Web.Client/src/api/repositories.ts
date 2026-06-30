import api from './client'
import type { Repository, UpdateRepositoryDto } from '../types'

export const repositoriesApi = {
  getAll: () => api.get<Repository[]>('/repositories').then(r => r.data),
  getById: (id: string) => api.get<Repository>(`/repositories/${id}`).then(r => r.data),
  update: (id: string, dto: UpdateRepositoryDto) =>
    api.put<Repository>(`/repositories/${id}`, dto).then(r => r.data),
  savePromptAsTemplate: (id: string, name: string) =>
    api.post<{ id: string; name: string }>(`/repositories/${id}/save-prompt-as-template`, { name }).then(r => r.data),
  saveCategoriesAsGroup: (id: string, name: string) =>
    api.post<{ id: string; name: string }>(`/repositories/${id}/save-categories-as-group`, { name }).then(r => r.data),
}
