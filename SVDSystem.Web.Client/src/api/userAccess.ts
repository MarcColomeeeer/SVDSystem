import api from './client'
import type { UserAccessDto } from '../dtos'

export const userAccessApi = {
  getUserAccesses: (userObjectId: string) =>
    api.get<UserAccessDto[]>(`/admin/users/${userObjectId}/repositories`).then(r => r.data),
  getRepositoryAccesses: (repositoryConfigurationId: string) =>
    api.get<UserAccessDto[]>(`/admin/users/repository/${repositoryConfigurationId}`).then(r => r.data),
  grant: (userObjectId: string, dto: { repositoryConfigurationId: string }) =>
    api.post<UserAccessDto>(`/admin/users/${userObjectId}/repositories`, dto).then(r => r.data),
  revoke: (id: string) => api.delete(`/admin/users/access/${id}`),
}
