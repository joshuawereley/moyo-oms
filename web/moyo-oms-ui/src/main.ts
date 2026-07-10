import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { RuntimeConfig, setRuntimeConfig } from './app/core/api.config';

// MSAL and the API clients read their settings while the application bootstraps,
// so config.json has to be resolved before bootstrap rather than injected into it.
fetch('config.json', { cache: 'no-store' })
  .then((response) => {
    if (!response.ok) {
      throw new Error(`Could not load config.json (HTTP ${response.status}).`);
    }

    return response.json() as Promise<RuntimeConfig>;
  })
  .then((config) => {
    setRuntimeConfig(config);
    return bootstrapApplication(App, appConfig);
  })
  .catch((err) => console.error(err));
