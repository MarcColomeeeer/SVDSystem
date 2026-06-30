export type UserDto = { id: string; displayName: string; email: string }
export type UserAccessDto = { id: string; userId: string; displayName: string; email: string; repositoryConfigurationId?: string }
export type RepositoryDto = { id: string; projectName: string; repositoryName: string }
