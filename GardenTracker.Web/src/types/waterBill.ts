export interface WaterBill {
  id: number
  year: number
  month: number
  usageCcf: number
  usageGallons: number
  totalCost: number
  isGardenActive: boolean
  notes?: string | null
}

export interface CreateWaterBillRequest {
  year: number
  month: number
  usageCcf: number
  usageGallons: number
  totalCost: number
  isGardenActive: boolean
  notes?: string | null
}

export interface UpdateWaterBillRequest {
  usageCcf: number
  usageGallons: number
  totalCost: number
  isGardenActive: boolean
  notes?: string | null
}
