declare global {
  interface Window {
    __ENV__?: Record<string, string>
  }
}

/**
 * Read a config value at runtime.
 * In containers: sourced from window.__ENV__ (written by docker-entrypoint.sh).
 * In local dev:  sourced from import.meta.env (Vite reads .env.local).
 */
export function getEnv(key: string): string | undefined {
  return window.__ENV__?.[key] || import.meta.env[key]
}
