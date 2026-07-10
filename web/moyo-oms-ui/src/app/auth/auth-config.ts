import {
  BrowserCacheLocation,
  IPublicClientApplication,
  InteractionType,
  PublicClientApplication,
} from '@azure/msal-browser';
import {
  MsalGuardConfiguration,
  MsalInterceptorConfiguration,
} from '@azure/msal-angular';

import { getRuntimeConfig } from '../core/api.config';

function apiScope(clientId: string): string {
  return `api://${clientId}/access_as_user`;
}

export function msalInstanceFactory(): IPublicClientApplication {
  const { clientId, tenantId } = getRuntimeConfig();

  return new PublicClientApplication({
    auth: {
      clientId,
      authority: `https://login.microsoftonline.com/${tenantId}`,
      // Origin rather than a literal: the same build serves localhost and the deployed site.
      redirectUri: window.location.origin,
      postLogoutRedirectUri: window.location.origin,
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage,
    },
  });
}

export function msalGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: {
      scopes: [apiScope(getRuntimeConfig().clientId)],
    },
  };
}

export function msalInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const { apiBaseUrl, clientId } = getRuntimeConfig();

  const protectedResourceMap = new Map<string, Array<string>>();
  protectedResourceMap.set(`${apiBaseUrl}/api/*`, [apiScope(clientId)]);

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap,
  };
}
