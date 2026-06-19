import { useState } from 'react'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import { toggleTheme, getCurrentTheme } from '@/lib/theme'

const NAV_LINKS = [
  { to: '/gardens',     label: 'Gardens'   },
  { to: '/plants',      label: 'Plants'    },
  { to: '/inventory',   label: 'Inventory' },
  { to: '/water-bills', label: 'Water'     },
  { to: '/reports',     label: 'Reports'   },
]

export default function AppShell() {
  const { signOut } = useAuth()
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const [theme, setTheme] = useState(getCurrentTheme)

  function handleSignOut() {
    signOut()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-40 bg-white dark:bg-slate-900 border-b border-border">
        <div className="mx-auto flex h-14 max-w-5xl items-center gap-8 px-4">

          {/* Brand */}
          <Link to="/" className="flex items-center gap-2 shrink-0">
            <SproutIcon />
            <span className="text-base font-semibold tracking-tight text-foreground">
              Garden Tracker
            </span>
          </Link>

          {/* Nav links */}
          <nav className="flex items-center gap-1 flex-1">
            {NAV_LINKS.map(({ to, label }) => {
              const active = pathname === to || pathname.startsWith(to + '/')
              return (
                <Link
                  key={to}
                  to={to}
                  className={`px-3 py-1.5 rounded-md text-sm transition-colors ${
                    active
                      ? 'bg-slate-100 dark:bg-slate-800 text-foreground font-medium'
                      : 'text-muted-foreground hover:text-foreground hover:bg-slate-50 dark:hover:bg-slate-800'
                  }`}
                >
                  {label}
                </Link>
              )
            })}
          </nav>

          {/* Theme toggle */}
          <button
            onClick={() => setTheme(toggleTheme())}
            className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
            title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            aria-label={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
          >
            {theme === 'dark' ? <SunIcon /> : <MoonIcon />}
          </button>

          {/* Sign out */}
          <button
            onClick={handleSignOut}
            className="text-sm text-muted-foreground hover:text-foreground transition-colors shrink-0"
          >
            Sign out
          </button>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}

function SproutIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-emerald-600" aria-hidden>
      <path d="M7 20h10"/>
      <path d="M10 20c5.5-2.5 4-6 3-8"/>
      <path d="M9 6.8A6 6 0 0 0 3 12c0 0 0 0 0 0h10c0-5.5-3-8.5-4-5.2z"/>
      <path d="M21 12h-2a6 6 0 0 0-5 7"/>
    </svg>
  )
}

function SunIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <circle cx="12" cy="12" r="4"/>
      <path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41"/>
    </svg>
  )
}

function MoonIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z"/>
    </svg>
  )
}
