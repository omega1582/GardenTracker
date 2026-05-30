import { useState } from 'react'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import { toggleTheme, getCurrentTheme } from '@/lib/theme'

const NAV_LINKS = [
  { to: '/gardens',    label: 'Gardens'    },
  { to: '/plants',     label: 'Plants'     },
  { to: '/inventory',  label: 'Inventory'  },
  { to: '/water-bills',label: 'Water'      },
  { to: '/reports',    label: 'Reports'    },
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
      {/* ── Header ──────────────────────────────────────────────────── */}
      <header
        style={{ backgroundColor: 'var(--nav-bg)', borderColor: 'var(--nav-accent)' }}
        className="sticky top-0 z-40 border-b"
      >
        <div
          className="mx-auto flex h-14 max-w-5xl items-center justify-between px-4 gap-8"
        >
          {/* Brand */}
          <Link
            to="/"
            className="flex items-center gap-2.5 shrink-0 group"
          >
            <LeafMark />
            <span
              className="heading-serif text-xl font-semibold italic tracking-tight"
              style={{ color: 'var(--nav-fg)' }}
            >
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
                  className="relative px-3 py-1.5 text-sm transition-colors"
                  style={{
                    color: active ? 'var(--nav-fg)' : 'var(--nav-fg-muted)',
                    fontWeight: active ? 500 : 400,
                  }}
                >
                  {label}
                  {active && (
                    <span
                      className="absolute bottom-0 left-3 right-3 h-px"
                      style={{ backgroundColor: 'var(--nav-accent)' }}
                    />
                  )}
                </Link>
              )
            })}
          </nav>

          {/* Theme toggle */}
          <button
            onClick={() => setTheme(toggleTheme())}
            className="text-sm transition-colors shrink-0 p-1.5 rounded"
            style={{ color: 'var(--nav-fg-muted)' }}
            title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            aria-label={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
          >
            {theme === 'dark' ? <SunIcon /> : <MoonIcon />}
          </button>

          {/* Sign out */}
          <button
            onClick={handleSignOut}
            className="text-sm transition-colors shrink-0"
            style={{ color: 'var(--nav-fg-subtle)' }}
            onMouseEnter={(e) =>
              ((e.target as HTMLElement).style.color = 'var(--nav-fg)')
            }
            onMouseLeave={(e) =>
              ((e.target as HTMLElement).style.color = 'var(--nav-fg-subtle)')
            }
          >
            Sign out
          </button>
        </div>
      </header>

      {/* ── Content ─────────────────────────────────────────────────── */}
      <main className="mx-auto max-w-5xl px-4 py-8">
        <Outlet />
      </main>
    </div>
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

function LeafMark() {
  return (
    <svg
      width="20"
      height="20"
      viewBox="0 0 20 20"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden
    >
      {/* Simple botanical leaf — two strokes, minimal */}
      <path
        d="M10 18 C10 18 3 14 3 7 C3 4 6 2 10 2 C14 2 17 4 17 7 C17 14 10 18 10 18Z"
        fill="oklch(0.62 0.140 43)"
        opacity="0.9"
      />
      <path
        d="M10 18 L10 8"
        stroke="oklch(0.22 0.07 155)"
        strokeWidth="1"
        strokeLinecap="round"
      />
      <path
        d="M10 11 C10 11 7 9.5 6 7"
        stroke="oklch(0.22 0.07 155)"
        strokeWidth="0.75"
        strokeLinecap="round"
        opacity="0.6"
      />
      <path
        d="M10 13 C10 13 13 11.5 14 9"
        stroke="oklch(0.22 0.07 155)"
        strokeWidth="0.75"
        strokeLinecap="round"
        opacity="0.6"
      />
    </svg>
  )
}
