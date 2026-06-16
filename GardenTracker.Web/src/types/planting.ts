export type StartMethod = 'Seed' | 'Transplant' | 'SeedSaved' | 'Cutting'

export interface Planting {
  id: number
  bedId: number
  bedName: string
  seasonId: number
  plantVarietyId: number
  plantVarietyName: string
  plantTypeName: string
  supplierId?: number | null
  supplierName?: string | null
  startMethod: StartMethod
  quantity: number
  totalCost: number
  sourceHarvestId?: number | null
  notes?: string | null
  inventoryItemId?: number | null
  quantityUsedFromInventory?: number | null
  positionX?: number | null
  positionY?: number | null
  layoutWidth?: number | null
  layoutHeight?: number | null
}

export interface CreatePlantingRequest {
  bedId: number
  plantVarietyId: number
  startMethod: StartMethod
  quantity: number
  totalCost: number
  notes?: string | null
  inventoryItemId?: number | null
  quantityUsedFromInventory?: number | null
}

export interface UpdatePlantingRequest {
  supplierId?: number | null
  startMethod: StartMethod
  quantity: number
  totalCost: number
  sourceHarvestId?: number | null
  notes?: string | null
  inventoryItemId?: number | null
  quantityUsedFromInventory?: number | null
}
