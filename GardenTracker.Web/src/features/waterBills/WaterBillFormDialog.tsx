import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createWaterBill, updateWaterBill } from '@/api/waterBills'
import type { WaterBill } from '@/types/waterBill'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'

const MONTHS = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
]

const CURRENT_YEAR = new Date().getFullYear()

interface Props {
  open: boolean
  onClose: () => void
  editing?: WaterBill
  defaultYear?: number
}

export default function WaterBillFormDialog({ open, onClose, editing, defaultYear }: Props) {
  const qc = useQueryClient()

  const [year, setYear] = useState(defaultYear ?? CURRENT_YEAR)
  const [month, setMonth] = useState(new Date().getMonth() + 1)
  const [usageCcf, setUsageCcf] = useState('')
  const [usageGallons, setUsageGallons] = useState('')
  const [totalCost, setTotalCost] = useState('')
  const [isGardenActive, setIsGardenActive] = useState(false)
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (open) {
      if (editing) {
        setYear(editing.year)
        setMonth(editing.month)
        setUsageCcf(String(editing.usageCcf))
        setUsageGallons(String(editing.usageGallons))
        setTotalCost(String(editing.totalCost))
        setIsGardenActive(editing.isGardenActive)
        setNotes(editing.notes ?? '')
      } else {
        setYear(defaultYear ?? CURRENT_YEAR)
        setMonth(new Date().getMonth() + 1)
        setUsageCcf('')
        setUsageGallons('')
        setTotalCost('')
        setIsGardenActive(false)
        setNotes('')
      }
    }
  }, [open, editing, defaultYear])

  // Auto-convert CCF to gallons when CCF changes (1 CCF = 748.05 gallons)
  function handleCcfChange(val: string) {
    setUsageCcf(val)
    if (val && !isNaN(Number(val))) {
      setUsageGallons((Number(val) * 748.05).toFixed(0))
    }
  }

  const mutation = useMutation<void>({
    mutationFn: () => {
      const payload = {
        usageCcf: Number(usageCcf),
        usageGallons: Number(usageGallons),
        totalCost: Number(totalCost),
        isGardenActive,
        notes: notes || null,
      }
      if (editing) {
        return updateWaterBill(editing.id, payload)
      }
      return createWaterBill({ ...payload, year, month }).then(() => {})
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['water-bills'] })
      onClose()
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate()
  }

  const canSubmit = !!usageCcf && !!totalCost

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle>{editing ? 'Edit Water Bill' : 'Add Water Bill'}</DialogTitle>
        </DialogHeader>
        <form id="water-bill-form" onSubmit={handleSubmit} className="space-y-4">

          {/* Year + Month — fixed on edit */}
          {editing ? (
            <p className="text-sm text-muted-foreground">
              {MONTHS[editing.month - 1]} {editing.year}
            </p>
          ) : (
            <div className="flex gap-3">
              <div className="space-y-1 w-24">
                <Label htmlFor="wb-year">Year</Label>
                <Input
                  id="wb-year"
                  type="number"
                  value={year}
                  onChange={(e) => setYear(Number(e.target.value))}
                  required
                />
              </div>
              <div className="space-y-1 flex-1">
                <Label htmlFor="wb-month">Month</Label>
                <select
                  id="wb-month"
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={month}
                  onChange={(e) => setMonth(Number(e.target.value))}
                >
                  {MONTHS.map((name, i) => (
                    <option key={i + 1} value={i + 1}>{name}</option>
                  ))}
                </select>
              </div>
            </div>
          )}

          {/* Usage */}
          <div className="flex gap-3">
            <div className="space-y-1 flex-1">
              <Label htmlFor="wb-ccf">Usage (CCF)</Label>
              <Input
                id="wb-ccf"
                type="number"
                min={0}
                step="0.01"
                value={usageCcf}
                onChange={(e) => handleCcfChange(e.target.value)}
                required
                placeholder="e.g. 4.5"
              />
            </div>
            <div className="space-y-1 flex-1">
              <Label htmlFor="wb-gal">Gallons</Label>
              <Input
                id="wb-gal"
                type="number"
                min={0}
                step="1"
                value={usageGallons}
                onChange={(e) => setUsageGallons(e.target.value)}
                placeholder="auto-filled"
              />
            </div>
          </div>

          {/* Total cost */}
          <div className="space-y-1">
            <Label htmlFor="wb-cost">Total Bill ($)</Label>
            <Input
              id="wb-cost"
              type="number"
              min={0}
              step="0.01"
              value={totalCost}
              onChange={(e) => setTotalCost(e.target.value)}
              required
              placeholder="e.g. 68.50"
            />
          </div>

          {/* Garden active toggle */}
          <div className="flex items-center gap-3 rounded-md border border-border p-3">
            <input
              id="wb-garden"
              type="checkbox"
              className="h-4 w-4"
              checked={isGardenActive}
              onChange={(e) => setIsGardenActive(e.target.checked)}
            />
            <div>
              <Label htmlFor="wb-garden" className="cursor-pointer">Garden active this month</Label>
              <p className="text-xs text-muted-foreground">Check if you were watering the garden during this billing period.</p>
            </div>
          </div>

          {/* Notes */}
          <div className="space-y-1">
            <Label htmlFor="wb-notes">Notes <span className="text-muted-foreground">(optional)</span></Label>
            <Textarea
              id="wb-notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">Something went wrong. Please try again.</p>
          )}
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="submit" form="water-bill-form" disabled={mutation.isPending || !canSubmit}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
