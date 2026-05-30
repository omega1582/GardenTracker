import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from './AuthContext'
import { login } from '@/api/auth'
import { Button } from '@/components/ui/button'

export default function LoginPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const data = await login({ email, password })
      signIn(data.accessToken, data.refreshToken)
      navigate('/')
    } catch (err) {
      if (axios.isAxiosError(err)) {
        if (err.response?.status === 401) {
          setError('Invalid email or password.')
        } else if (!err.response) {
          setError('Cannot reach the server. Is the API running?')
        } else {
          setError(`Login failed (${err.response.status}).`)
        }
      } else {
        setError('Something went wrong. Please try again.')
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <div className="w-full max-w-sm space-y-6 border border-border bg-card p-8 shadow-sm" style={{ borderRadius: '0.2rem' }}>
        <div className="text-center space-y-1 pb-2 border-b border-border">
          <div className="flex justify-center mb-3">
            <svg width="32" height="32" viewBox="0 0 20 20" fill="none" aria-hidden>
              <path d="M10 18 C10 18 3 14 3 7 C3 4 6 2 10 2 C14 2 17 4 17 7 C17 14 10 18 10 18Z" fill="oklch(0.62 0.140 43)" opacity="0.9"/>
              <path d="M10 18 L10 8" stroke="oklch(0.30 0.090 155)" strokeWidth="1" strokeLinecap="round"/>
            </svg>
          </div>
          <h1 className="heading-serif text-3xl font-semibold italic" style={{ color: 'oklch(0.30 0.090 155)' }}>Garden Tracker</h1>
          <p className="text-sm text-muted-foreground">Sign in to your account</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <label htmlFor="email" className="text-sm font-medium">
              Email
            </label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            />
          </div>

          <div className="space-y-1">
            <label htmlFor="password" className="text-sm font-medium">
              Password
            </label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-ring"
            />
          </div>

          {error && <p className="text-sm text-destructive">{error}</p>}

          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? 'Signing in…' : 'Sign in'}
          </Button>
        </form>

        <p className="text-center text-sm text-muted-foreground">
          Don't have an account?{' '}
          <Link to="/register" className="font-medium text-primary hover:underline">
            Register
          </Link>
        </p>
      </div>
    </div>
  )
}
