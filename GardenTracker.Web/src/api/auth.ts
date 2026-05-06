import api, { clearTokens } from '@/lib/axios'
import type { AuthResponse, LoginRequest, RegisterRequest } from '@/types/auth'

export async function login(data: LoginRequest): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/api/v1/auth/login', data)
  return response.data
}

export async function register(data: RegisterRequest): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/api/v1/auth/register', data)
  return response.data
}

export function logout() {
  clearTokens()
  window.location.href = '/login'
}
