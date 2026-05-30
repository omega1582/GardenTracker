import api from '@/lib/axios'
import type { Harvest, CreateHarvestRequest, UpdateHarvestRequest } from '@/types/harvest'

export async function getHarvests(gardenId: number, year: number): Promise<Harvest[]> {
  const res = await api.get<Harvest[]>(`/api/v1/gardens/${gardenId}/seasons/${year}/harvests`)
  return res.data
}

export async function createHarvest(
  gardenId: number,
  year: number,
  data: CreateHarvestRequest
): Promise<Harvest> {
  const res = await api.post<Harvest>(`/api/v1/gardens/${gardenId}/seasons/${year}/harvests`, data)
  return res.data
}

export async function updateHarvest(
  gardenId: number,
  year: number,
  id: number,
  data: UpdateHarvestRequest
): Promise<void> {
  await api.put(`/api/v1/gardens/${gardenId}/seasons/${year}/harvests/${id}`, data)
}

export async function deleteHarvest(gardenId: number, year: number, id: number): Promise<void> {
  await api.delete(`/api/v1/gardens/${gardenId}/seasons/${year}/harvests/${id}`)
}
