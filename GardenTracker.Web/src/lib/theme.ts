type Theme = 'light' | 'dark'

function applyTheme(dark: boolean) {
  document.documentElement.classList.toggle('dark', dark)
}

function getSystemDark() {
  return window.matchMedia('(prefers-color-scheme: dark)').matches
}

export function initTheme() {
  const stored = localStorage.getItem('theme') as Theme | null
  applyTheme(stored ? stored === 'dark' : getSystemDark())

  // Track system changes (only applies when no stored preference)
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
    if (!localStorage.getItem('theme')) applyTheme(e.matches)
  })
}

export function toggleTheme(): Theme {
  const isDark = document.documentElement.classList.contains('dark')
  const next: Theme = isDark ? 'light' : 'dark'
  localStorage.setItem('theme', next)
  applyTheme(next === 'dark')
  return next
}

export function getCurrentTheme(): Theme {
  return document.documentElement.classList.contains('dark') ? 'dark' : 'light'
}
