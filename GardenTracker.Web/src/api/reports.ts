import api from '@/lib/axios'
import type { SeasonSummary, YearSummary, WaterAttributionResult, BedBreakdown } from '@/types/report'

export async function getSeasonSummary(gardenId: number, year: number): Promise<SeasonSummary> {
  const res = await api.get<SeasonSummary>(`/api/v1/reports/gardens/${gardenId}/season/${year}`)
  return res.data
}

export async function getYearOverYear(gardenId: number): Promise<YearSummary[]> {
  const res = await api.get<YearSummary[]>(`/api/v1/reports/gardens/${gardenId}/year-over-year`)
  return res.data
}

export async function getWaterAttribution(year?: number): Promise<WaterAttributionResult[]> {
  const res = await api.get<WaterAttributionResult[]>('/api/v1/reports/water-attribution', {
    params: year ? { year } : undefined,
  })
  return res.data
}

export async function getBedBreakdown(gardenId: number, year: number): Promise<BedBreakdown[]> {
  const res = await api.get<BedBreakdown[]>(`/api/v1/reports/gardens/${gardenId}/season/${year}/beds`)
  return res.data
}
