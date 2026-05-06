export interface PlantType {
  id: number
  name: string
}

export interface PlantVariety {
  id: number
  plantTypeId: number
  plantTypeName: string
  name: string
  notes?: string | null
}
