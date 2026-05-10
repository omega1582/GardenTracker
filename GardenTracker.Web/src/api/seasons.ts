import api from '@/lib/axios'
import type { Season } from '@/types/season'

export async function getSeasons(gardenId: number): Promise<Season[]> {
  const res = await api.get<Season[]>(`/api/v1/gardens/${gardenId}/seasons`)
  return res.data
}

export async function createSeason(gardenId: number, year: number): Promise<Season> {
  const res = await api.post<Season>(`/api/v1/gardens/${gardenId}/seasons`, { year })
  return res.data
}
