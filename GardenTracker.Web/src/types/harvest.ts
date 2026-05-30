export type HarvestUnit = 'Pounds' | 'Ounces' | 'Count' | 'Bunch'

export interface Harvest {
  id: number
  bedId: number
  bedName: string
  seasonId: number
  plantVarietyId: number
  plantVarietyName: string
  plantTypeName: string
  quantity: number
  unit: HarvestUnit
  harvestDate: string
  notes?: string | null
}

export interface CreateHarvestRequest {
  bedId: number
  plantVarietyId: number
  quantity: number
  unit: HarvestUnit
  harvestDate: string
  notes?: string | null
}

export interface UpdateHarvestRequest {
  quantity: number
  unit: HarvestUnit
  harvestDate: string
  notes?: string | null
}
