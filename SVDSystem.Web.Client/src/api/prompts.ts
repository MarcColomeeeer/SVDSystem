import api from './client'
import type { PromptTemplate, UpsertPromptTemplateDto } from '../types'

export const promptsApi = {
  getAll: () => api.get<PromptTemplate[]>('/prompts').then(r => r.data),
  getById: (id: string) => api.get<PromptTemplate>(`/prompts/${id}`).then(r => r.data),
  create: (dto: UpsertPromptTemplateDto) => api.post<PromptTemplate>('/prompts', dto).then(r => r.data),
  update: (id: string, dto: UpsertPromptTemplateDto) =>
    api.put<PromptTemplate>(`/prompts/${id}`, dto).then(r => r.data),
  delete: (id: string) => api.delete(`/prompts/${id}`),
}
