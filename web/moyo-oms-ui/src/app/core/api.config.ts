/**
 * Settings that differ per environment and are only known after the backend is
 * deployed, so they are fetched from /config.json at startup rather than compiled in.
 */
export interface RuntimeConfig {
  apiBaseUrl: string;
  clientId: string;
  tenantId: string;
}

let runtimeConfig: RuntimeConfig | undefined;

export function setRuntimeConfig(config: RuntimeConfig): void {
  runtimeConfig = config;
}

export function getRuntimeConfig(): RuntimeConfig {
  if (!runtimeConfig) {
    throw new Error('Runtime config was not loaded before the application bootstrapped.');
  }

  return runtimeConfig;
}

export function apiBaseUrl(): string {
  return getRuntimeConfig().apiBaseUrl;
}
