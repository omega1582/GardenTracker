import api from '@/lib/axios'
import type { Expense, CreateExpenseRequest, UpdateExpenseRequest } from '@/types/expense'

export async function getExpenses(gardenId: number, year: number): Promise<Expense[]> {
  const res = await api.get<Expense[]>(`/api/v1/gardens/${gardenId}/seasons/${year}/expenses`)
  return res.data
}

export async function createExpense(
  gardenId: number,
  year: number,
  data: CreateExpenseRequest
): Promise<Expense> {
  const res = await api.post<Expense>(`/api/v1/gardens/${gardenId}/seasons/${year}/expenses`, data)
  return res.data
}

export async function updateExpense(
  gardenId: number,
  year: number,
  id: number,
  data: UpdateExpenseRequest
): Promise<void> {
  await api.put(`/api/v1/gardens/${gardenId}/seasons/${year}/expenses/${id}`, data)
}

export async function deleteExpense(gardenId: number, year: number, id: number): Promise<void> {
  await api.delete(`/api/v1/gardens/${gardenId}/seasons/${year}/expenses/${id}`)
}
