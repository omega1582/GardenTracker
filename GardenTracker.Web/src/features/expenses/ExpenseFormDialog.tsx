import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createExpense, updateExpense } from '@/api/expenses'
import type { Bed } from '@/types/bed'
import type { Expense, ExpenseCategory, CreateExpenseRequest, UpdateExpenseRequest } from '@/types/expense'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'

const CATEGORIES: { value: ExpenseCategory; label: string }[] = [
  { value: 'Seeds', label: 'Seeds' },
  { value: 'Transplants', label: 'Transplants' },
  { value: 'Soil', label: 'Soil' },
  { value: 'Fertilizer', label: 'Fertilizer' },
  { value: 'PestControl', label: 'Pest Control' },
  { value: 'BedMaterials', label: 'Bed Materials' },
  { value: 'Tools', label: 'Tools' },
  { value: 'Maintenance', label: 'Maintenance' },
  { value: 'Other', label: 'Other' },
]

interface Props {
  open: boolean
  onClose: () => void
  gardenId: number
  year: number
  beds: Bed[]
  editing?: Expense
}

export default function ExpenseFormDialog({ open, onClose, gardenId, year, beds, editing }: Props) {
  const qc = useQueryClient()

  const [bedId, setBedId] = useState<number | ''>('')
  const [category, setCategory] = useState<ExpenseCategory>('Seeds')
  const [description, setDescription] = useState('')
  const [amount, setAmount] = useState('')
  const [expenseDate, setExpenseDate] = useState('')

  useEffect(() => {
    if (open) {
      if (editing) {
        setBedId(editing.bedId ?? '')
        setCategory(editing.category)
        setDescription(editing.description)
        setAmount(String(editing.amount))
        setExpenseDate(editing.expenseDate)
      } else {
        setBedId('')
        setCategory('Seeds')
        setDescription('')
        setAmount('')
        setExpenseDate(new Date().toISOString().slice(0, 10))
      }
    }
  }, [open, editing])

  const mutation = useMutation<void>({
    mutationFn: () => {
      const payload: CreateExpenseRequest & UpdateExpenseRequest = {
        bedId: bedId ? Number(bedId) : null,
        category,
        description,
        amount: Number(amount),
        expenseDate,
      }
      if (editing) {
        return updateExpense(gardenId, year, editing.id, payload)
      }
      return createExpense(gardenId, year, payload).then(() => {})
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['expenses', gardenId, year] })
      onClose()
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate()
  }

  const canSubmit = !!description && !!amount && !!expenseDate

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle>{editing ? 'Edit Expense' : `Add Expense — ${year}`}</DialogTitle>
        </DialogHeader>
        <form id="expense-form" onSubmit={handleSubmit} className="space-y-4">

          {/* Category */}
          <div className="space-y-1">
            <Label htmlFor="exp-category">Category</Label>
            <select
              id="exp-category"
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={category}
              onChange={(e) => setCategory(e.target.value as ExpenseCategory)}
            >
              {CATEGORIES.map((c) => (
                <option key={c.value} value={c.value}>{c.label}</option>
              ))}
            </select>
          </div>

          {/* Description */}
          <div className="space-y-1">
            <Label htmlFor="exp-desc">Description</Label>
            <Input
              id="exp-desc"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="e.g. Tomato transplants from nursery"
              required
            />
          </div>

          {/* Amount */}
          <div className="space-y-1">
            <Label htmlFor="exp-amount">Amount ($)</Label>
            <Input
              id="exp-amount"
              type="number"
              min={0}
              step="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              required
              placeholder="0.00"
            />
          </div>

          {/* Date */}
          <div className="space-y-1">
            <Label htmlFor="exp-date">Date</Label>
            <Input
              id="exp-date"
              type="date"
              value={expenseDate}
              onChange={(e) => setExpenseDate(e.target.value)}
              required
            />
          </div>

          {/* Bed (optional) */}
          {beds.length > 0 && (
            <div className="space-y-1">
              <Label htmlFor="exp-bed">Bed <span className="text-muted-foreground">(optional)</span></Label>
              <select
                id="exp-bed"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={bedId}
                onChange={(e) => setBedId(e.target.value ? Number(e.target.value) : '')}
              >
                <option value="">No specific bed</option>
                {beds.map((b) => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            </div>
          )}

          {mutation.isError && (
            <p className="text-sm text-destructive">Something went wrong. Please try again.</p>
          )}
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="submit" form="expense-form" disabled={mutation.isPending || !canSubmit}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
