import api from '@/lib/axios'
import type { PlantType, PlantVariety, GrowthHabit, SunPreference } from '@/types/plant'
import type { CsvImportResult } from './inventory'

export interface PlantTypePayload {
  name: string
  category: string
  growthHabit?: GrowthHabit | null
  daysToMaturity?: number | null
  spacingInches?: number | null
  sunPreference?: SunPreference | null
  isPerennial?: boolean | null
}

export interface PlantVarietyPayload {
  name: string
  notes?: string | null
  growthHabit?: GrowthHabit | null
  daysToMaturity?: number | null
  spacingInches?: number | null
  sunPreference?: SunPreference | null
  isPerennial?: boolean | null
  imageUrl?: string | null
}

export async function getPlantTypes(): Promise<PlantType[]> {
  const res = await api.get<PlantType[]>('/api/v1/plant-types')
  return res.data
}

export async function createPlantType(data: PlantTypePayload): Promise<PlantType> {
  const res = await api.post<PlantType>('/api/v1/plant-types', data)
  return res.data
}

export async function updatePlantType(id: number, data: PlantTypePayload): Promise<void> {
  await api.put(`/api/v1/plant-types/${id}`, data)
}

export async function getVarieties(plantTypeId: number): Promise<PlantVariety[]> {
  const res = await api.get<PlantVariety[]>(`/api/v1/plant-types/${plantTypeId}/varieties`)
  return res.data
}

export async function getAllVarieties(): Promise<PlantVariety[]> {
  const res = await api.get<PlantVariety[]>('/api/v1/plant-varieties')
  return res.data
}

export async function createVariety(plantTypeId: number, data: PlantVarietyPayload): Promise<PlantVariety> {
  const res = await api.post<PlantVariety>(`/api/v1/plant-types/${plantTypeId}/varieties`, data)
  return res.data
}

export async function updateVariety(id: number, data: PlantVarietyPayload): Promise<void> {
  await api.put(`/api/v1/plant-varieties/${id}`, data)
}

export async function importPlantCatalogCsv(file: File): Promise<CsvImportResult> {
  const form = new FormData()
  form.append('file', file)
  const res = await api.post<CsvImportResult>('/api/v1/plant-types/import', form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return res.data
}

export async function uploadVarietyImage(file: File): Promise<{ url: string }> {
  const form = new FormData()
  form.append('file', file)
  const res = await api.post<{ url: string }>('/api/v1/plant-varieties/upload-image', form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return res.data
}
