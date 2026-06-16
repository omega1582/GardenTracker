import api from '@/lib/axios'
import type { Planting, CreatePlantingRequest, UpdatePlantingRequest } from '@/types/planting'

export async function getPlantings(gardenId: number, year: number, bedId?: number): Promise<Planting[]> {
  const res = await api.get<Planting[]>(`/api/v1/gardens/${gardenId}/seasons/${year}/plantings`, {
    params: bedId ? { bedId } : undefined,
  })
  return res.data
}

export async function createPlanting(
  gardenId: number,
  year: number,
  data: CreatePlantingRequest
): Promise<Planting> {
  const res = await api.post<Planting>(`/api/v1/gardens/${gardenId}/seasons/${year}/plantings`, data)
  return res.data
}

export async function updatePlanting(
  gardenId: number,
  year: number,
  id: number,
  data: UpdatePlantingRequest
): Promise<void> {
  await api.put(`/api/v1/gardens/${gardenId}/seasons/${year}/plantings/${id}`, data)
}

export async function deletePlanting(gardenId: number, year: number, id: number): Promise<void> {
  await api.delete(`/api/v1/gardens/${gardenId}/seasons/${year}/plantings/${id}`)
}

export async function updatePlantingLayout(
  gardenId: number,
  year: number,
  id: number,
  positionX: number | null,
  positionY: number | null,
  layoutWidth: number | null,
  layoutHeight: number | null
): Promise<void> {
  await api.patch(`/api/v1/gardens/${gardenId}/seasons/${year}/plantings/${id}/layout`, {
    positionX, positionY, layoutWidth, layoutHeight,
  })
}
