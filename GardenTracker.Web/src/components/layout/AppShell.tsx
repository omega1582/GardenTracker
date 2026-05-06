import { Link, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import { Button } from '@/components/ui/button'

export default function AppShell() {
  const { signOut } = useAuth()
  const navigate = useNavigate()

  function handleSignOut() {
    signOut()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-background">
      <header className="border-b border-border">
        <div className="mx-auto flex h-14 max-w-5xl items-center justify-between px-4">
          <Link to="/" className="text-base font-semibold tracking-tight">
            🌱 Garden Tracker
          </Link>
          <nav className="flex items-center gap-4">
            <Link to="/gardens" className="text-sm text-muted-foreground hover:text-foreground">
              Gardens
            </Link>
            <Link to="/water-bills" className="text-sm text-muted-foreground hover:text-foreground">
              Water Bills
            </Link>
            <Link to="/reports" className="text-sm text-muted-foreground hover:text-foreground">
              Reports
            </Link>
            <Button variant="ghost" size="sm" onClick={handleSignOut}>
              Sign out
            </Button>
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}
