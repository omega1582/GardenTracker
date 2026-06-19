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
  positionX: number | null
  positionY: number | null
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
  lengthFt: number
  widthFt: number
  depthIn: number
  material?: string
  expectedLifespanYears?: number
  installedDate: string
  notes?: string
}
