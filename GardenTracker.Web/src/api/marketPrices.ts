import api from '@/lib/axios'
import type { MarketPrice, CreateMarketPriceRequest } from '@/types/marketPrice'

export async function getMarketPrices(gardenId: number, year: number): Promise<MarketPrice[]> {
  const res = await api.get<MarketPrice[]>(`/api/v1/gardens/${gardenId}/seasons/${year}/market-prices`)
  return res.data
}

export async function createMarketPrice(
  gardenId: number,
  year: number,
  data: CreateMarketPriceRequest
): Promise<MarketPrice> {
  const res = await api.post<MarketPrice>(`/api/v1/gardens/${gardenId}/seasons/${year}/market-prices`, data)
  return res.data
}

export async function updateMarketPrice(
  gardenId: number,
  year: number,
  id: number,
  data: CreateMarketPriceRequest
): Promise<void> {
  await api.put(`/api/v1/gardens/${gardenId}/seasons/${year}/market-prices/${id}`, data)
}

export async function deleteMarketPrice(gardenId: number, year: number, id: number): Promise<void> {
  await api.delete(`/api/v1/gardens/${gardenId}/seasons/${year}/market-prices/${id}`)
}
