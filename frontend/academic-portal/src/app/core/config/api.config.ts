type RuntimeConfig = {
  __env?: {
    API_BASE_URL?: string;
  };
};

const runtimeConfig = globalThis as RuntimeConfig;

export const API_BASE_URL = runtimeConfig.__env?.API_BASE_URL ?? 'http://localhost:5262';
