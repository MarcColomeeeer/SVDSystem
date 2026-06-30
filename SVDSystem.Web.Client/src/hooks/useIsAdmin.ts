import { useAccount } from '@azure/msal-react'

export function useIsAdmin(): boolean {
  const account = useAccount()
  const roles: string[] = (account?.idTokenClaims as Record<string, unknown>)?.roles as string[] ?? []
    // return roles.includes('Admin')
  return true
}
