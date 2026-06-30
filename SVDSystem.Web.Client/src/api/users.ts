import api from './client'
import type { UserDto } from '../dtos'

export const usersApi = {
  getAll: () => api.get<UserDto[]>('/admin/users').then(r => r.data),
}
