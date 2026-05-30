import api from '@/lib/axios'
import type { WaterBill, CreateWaterBillRequest, UpdateWaterBillRequest } from '@/types/waterBill'

export async function getWaterBills(year?: number): Promise<WaterBill[]> {
  const res = await api.get<WaterBill[]>('/api/v1/water-bills', {
    params: year ? { year } : undefined,
  })
  return res.data
}

export async function createWaterBill(data: CreateWaterBillRequest): Promise<WaterBill> {
  const res = await api.post<WaterBill>('/api/v1/water-bills', data)
  return res.data
}

export async function updateWaterBill(id: number, data: UpdateWaterBillRequest): Promise<void> {
  await api.put(`/api/v1/water-bills/${id}`, data)
}

export async function deleteWaterBill(id: number): Promise<void> {
  await api.delete(`/api/v1/water-bills/${id}`)
}
