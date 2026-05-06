export interface Bed {
  id: number
  gardenId: number
  name: string
  lengthFt: number
  widthFt: number
  depthIn: number
  material: string | null
  expectedLifespanYears: number
  installedDate: string   // ISO date string (YYYY-MM-DD)
  notes: string | null
}

export interface CreateBedRequest {
  name: string
  lengthFt: number
  widthFt: number
  depthIn: number
  material?: string
  expectedLifespanYears?: number
  installedDate: string   // YYYY-MM-DD
  notes?: string
}

export interface UpdateBedRequest {
  name: string
  material?: string
  expectedLifespanYears?: number
  notes?: string
}
