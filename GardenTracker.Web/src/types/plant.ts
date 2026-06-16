export type GrowthHabit = 'Upright' | 'Vining' | 'Bushy' | 'Spreading' | 'Rosette'
export type SunPreference = 'FullSun' | 'PartialSun' | 'Shade'

export interface PlantType {
  id: number
  name: string
  growthHabit?: GrowthHabit | null
  daysToMaturity?: number | null
  spacingInches?: number | null
  sunPreference?: SunPreference | null
  isPerennial?: boolean | null
}

export interface PlantVariety {
  id: number
  plantTypeId: number
  plantTypeName: string
  name: string
  notes?: string | null
  growthHabit?: GrowthHabit | null
  daysToMaturity?: number | null
  spacingInches?: number | null
  sunPreference?: SunPreference | null
  isPerennial?: boolean | null
}
