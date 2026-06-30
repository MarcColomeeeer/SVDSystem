import axios from 'axios'
import { msalInstance, apiScopes } from '../auth/msalConfig'

const api = axios.create({ baseURL: '/api' })

// Attach Bearer token to every request
api.interceptors.request.use(async (config) => {
  const account = msalInstance.getActiveAccount()
  if (account) {
    const result = await msalInstance.acquireTokenSilent({ scopes: apiScopes, account })
    config.headers.Authorization = `Bearer ${result.accessToken}`
  }
  return config
})

export default api
