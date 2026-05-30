import type { HarvestUnit } from './harvest'

export interface MarketPrice {
  id: number
  seasonId: number
  plantTypeId: number
  plantTypeName: string
  plantVarietyId?: number | null
  plantVarietyName?: string | null
  pricePerUnit: number
  unit: HarvestUnit
  recordedDate: string
}

export interface CreateMarketPriceRequest {
  plantTypeId: number
  plantVarietyId?: number | null
  pricePerUnit: number
  unit: HarvestUnit
  recordedDate: string
}
