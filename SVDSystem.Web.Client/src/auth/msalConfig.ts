import { PublicClientApplication } from '@azure/msal-browser'
import type { Configuration, RedirectRequest } from '@azure/msal-browser'

export const msalConfig: Configuration = {
  auth: {
    clientId: '6ef84b08-247a-451a-9156-2a8bbb82137c',
    authority: 'https://login.microsoftonline.com/b9e785d9-6cc2-4dfc-8a52-95dcf6162f47',
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'sessionStorage',
  },
}

export const loginRequest: RedirectRequest = {
  scopes: ['api://7dca6d58-f0b2-457e-9c65-1d1c45b0b3ef/access_as_user'],
}

export const apiScopes = ['api://7dca6d58-f0b2-457e-9c65-1d1c45b0b3ef/access_as_user']

export const msalInstance = new PublicClientApplication(msalConfig)
