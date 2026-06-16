import api from '@/lib/axios'
import type { InventoryItem, CreateInventoryItemRequest, UpdateInventoryItemRequest } from '@/types/inventory'

export async function getInventory(): Promise<InventoryItem[]> {
  const res = await api.get<InventoryItem[]>('/api/v1/inventory')
  return res.data
}

export async function getInventoryByVariety(plantVarietyId: number): Promise<InventoryItem[]> {
  const res = await api.get<InventoryItem[]>(`/api/v1/inventory/variety/${plantVarietyId}`)
  return res.data
}

export async function createInventoryItem(data: CreateInventoryItemRequest): Promise<InventoryItem> {
  const res = await api.post<InventoryItem>('/api/v1/inventory', data)
  return res.data
}

export async function updateInventoryItem(id: number, data: UpdateInventoryItemRequest): Promise<void> {
  await api.put(`/api/v1/inventory/${id}`, data)
}

export async function adjustInventoryRemaining(id: number, newRemaining: number): Promise<void> {
  await api.patch(`/api/v1/inventory/${id}/adjust`, { newRemaining })
}

export async function deleteInventoryItem(id: number): Promise<void> {
  await api.delete(`/api/v1/inventory/${id}`)
}

export async function exportInventoryCsv(): Promise<Blob> {
  const res = await api.get('/api/v1/inventory/export', { responseType: 'blob' })
  return res.data as Blob
}

export interface CsvImportResult {
  created: number
  updated: number
  errors: string[]
}

export async function importInventoryCsv(file: File): Promise<CsvImportResult> {
  const form = new FormData()
  form.append('file', file)
  const res = await api.post<CsvImportResult>('/api/v1/inventory/import', form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return res.data
}
