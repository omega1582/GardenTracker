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
