export type ExpenseCategory =
  | 'Seeds'
  | 'Transplants'
  | 'Soil'
  | 'Fertilizer'
  | 'PestControl'
  | 'BedMaterials'
  | 'Tools'
  | 'Maintenance'
  | 'Other'

export interface Expense {
  id: number
  seasonId: number
  bedId?: number | null
  bedName?: string | null
  supplierId?: number | null
  supplierName?: string | null
  category: ExpenseCategory
  description: string
  amount: number
  expenseDate: string
}

export interface CreateExpenseRequest {
  bedId?: number | null
  category: ExpenseCategory
  description: string
  amount: number
  expenseDate: string
}

export interface UpdateExpenseRequest {
  bedId?: number | null
  category: ExpenseCategory
  description: string
  amount: number
  expenseDate: string
}
