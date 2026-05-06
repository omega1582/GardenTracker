import api from '@/lib/axios'
import type { Bed, CreateBedRequest, UpdateBedRequest } from '@/types/bed'

export async function getBeds(gardenId: number): Promise<Bed[]> {
  const res = await api.get<Bed[]>(`/api/v1/gardens/${gardenId}/beds`)
  return res.data
}

export async function createBed(gardenId: number, data: CreateBedRequest): Promise<Bed> {
  const res = await api.post<Bed>(`/api/v1/gardens/${gardenId}/beds`, data)
  return res.data
}

export async function updateBed(gardenId: number, id: number, data: UpdateBedRequest): Promise<void> {
  await api.put(`/api/v1/gardens/${gardenId}/beds/${id}`, data)
}

export async function deleteBed(gardenId: number, id: number): Promise<void> {
  await api.delete(`/api/v1/gardens/${gardenId}/beds/${id}`)
}
