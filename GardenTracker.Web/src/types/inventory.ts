export type InventoryType = 'Seed' | 'Plant'

export interface InventoryItem {
  id: number
  plantVarietyId: number
  plantVarietyName: string
  plantTypeName: string
  supplierId?: number | null
  supplierName?: string | null
  type: InventoryType
  quantityPurchased: number
  quantityRemaining: number
  totalCost: number
  purchaseDate: string
  notes?: string | null
}

export interface CreateInventoryItemRequest {
  plantVarietyId: number
  supplierId?: number | null
  type: InventoryType
  quantityPurchased: number
  totalCost: number
  purchaseDate: string
  notes?: string | null
}

export interface UpdateInventoryItemRequest {
  supplierId?: number | null
  quantityPurchased: number
  totalCost: number
  purchaseDate: string
  notes?: string | null
}
