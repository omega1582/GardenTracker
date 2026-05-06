import api from '@/lib/axios'
import type { PlantType, PlantVariety } from '@/types/plant'

export async function getPlantTypes(): Promise<PlantType[]> {
  const res = await api.get<PlantType[]>('/api/v1/plant-types')
  return res.data
}

export async function createPlantType(name: string): Promise<PlantType> {
  const res = await api.post<PlantType>('/api/v1/plant-types', { name })
  return res.data
}

export async function updatePlantType(id: number, name: string): Promise<void> {
  await api.put(`/api/v1/plant-types/${id}`, { name })
}

export async function getVarieties(plantTypeId: number): Promise<PlantVariety[]> {
  const res = await api.get<PlantVariety[]>(`/api/v1/plant-types/${plantTypeId}/varieties`)
  return res.data
}

export async function createVariety(
  plantTypeId: number,
  data: { name: string; notes?: string }
): Promise<PlantVariety> {
  const res = await api.post<PlantVariety>(`/api/v1/plant-types/${plantTypeId}/varieties`, data)
  return res.data
}

export async function updateVariety(
  id: number,
  data: { name: string; notes?: string }
): Promise<void> {
  await api.put(`/api/v1/plant-varieties/${id}`, data)
}
