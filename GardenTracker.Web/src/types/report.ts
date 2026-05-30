export interface HarvestLine {
  varietyName: string
  plantTypeName: string
  quantity: number
  unit: string
  pricePerUnit?: number | null
  value?: number | null
}

export interface SeasonSummary {
  gardenId: number
  year: number
  expensesByCategory: Record<string, number>
  totalExpenses: number
  harvestLines: HarvestLine[]
  totalHarvestValue: number
  waterAttribution: number
  netCost: number
}

export interface MonthSummary {
  month: number
  totalExpenses: number
  totalHarvestValue: number
  waterAttribution: number
  netCost: number
}

export interface YearSummary {
  year: number
  months: MonthSummary[]
  totalExpenses: number
  totalHarvestValue: number
  waterAttribution: number
  netCost: number
}
