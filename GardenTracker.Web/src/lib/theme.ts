// Syncs the .dark class on <html> with the OS color-scheme preference.
// Called once at startup; also listens for live changes.
export function initTheme() {
  const mql = window.matchMedia('(prefers-color-scheme: dark)')

  const apply = (dark: boolean) =>
    document.documentElement.classList.toggle('dark', dark)

  apply(mql.matches)
  mql.addEventListener('change', (e) => apply(e.matches))
}
