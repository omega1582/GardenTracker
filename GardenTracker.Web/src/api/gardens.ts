import api from '@/lib/axios'
import type { Garden, CreateGardenRequest, UpdateGardenRequest } from '@/types/garden'

export async function getGardens(): Promise<Garden[]> {
  const res = await api.get<Garden[]>('/api/v1/gardens')
  return res.data
}

export async function getGarden(id: number): Promise<Garden> {
  const res = await api.get<Garden>(`/api/v1/gardens/${id}`)
  return res.data
}

export async function createGarden(data: CreateGardenRequest): Promise<Garden> {
  const res = await api.post<Garden>('/api/v1/gardens', data)
  return res.data
}

export async function updateGarden(id: number, data: UpdateGardenRequest): Promise<void> {
  await api.put(`/api/v1/gardens/${id}`, data)
}

export async function deleteGarden(id: number): Promise<void> {
  await api.delete(`/api/v1/gardens/${id}`)
}
