export interface Garden {
  id: number
  name: string
  location: string | null
  notes: string | null
  createdAt: string
}

export interface CreateGardenRequest {
  name: string
  location?: string
  notes?: string
}

export interface UpdateGardenRequest {
  name: string
  location?: string
  notes?: string
}
