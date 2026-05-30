import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createMarketPrice, updateMarketPrice } from '@/api/marketPrices'
import { getPlantTypes, getVarieties } from '@/api/plants'
import type { MarketPrice, CreateMarketPriceRequest } from '@/types/marketPrice'
import type { HarvestUnit } from '@/types/harvest'
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

const UNITS: { value: HarvestUnit; label: string }[] = [
  { value: 'Pounds', label: 'per lb' },
  { value: 'Ounces', label: 'per oz' },
  { value: 'Count', label: 'per each' },
  { value: 'Bunch', label: 'per bunch' },
]

interface Props {
  open: boolean
  onClose: () => void
  gardenId: number
  year: number
  editing?: MarketPrice
}

export default function MarketPriceFormDialog({ open, onClose, gardenId, year, editing }: Props) {
  const qc = useQueryClient()

  const [plantTypeId, setPlantTypeId] = useState<number | ''>('')
  const [plantVarietyId, setPlantVarietyId] = useState<number | ''>('')
  const [pricePerUnit, setPricePerUnit] = useState('')
  const [unit, setUnit] = useState<HarvestUnit>('Pounds')
  const [recordedDate, setRecordedDate] = useState('')

  const { data: plantTypes = [] } = useQuery({
    queryKey: ['plant-types'],
    queryFn: getPlantTypes,
  })

  const { data: varieties = [] } = useQuery({
    queryKey: ['varieties', plantTypeId],
    queryFn: () => getVarieties(Number(plantTypeId)),
    enabled: !!plantTypeId,
  })

  useEffect(() => {
    if (open) {
      if (editing) {
        setPlantTypeId(editing.plantTypeId)
        setPlantVarietyId(editing.plantVarietyId ?? '')
        setPricePerUnit(String(editing.pricePerUnit))
        setUnit(editing.unit)
        setRecordedDate(editing.recordedDate)
      } else {
        setPlantTypeId('')
        setPlantVarietyId('')
        setPricePerUnit('')
        setUnit('Pounds')
        setRecordedDate(new Date().toISOString().slice(0, 10))
      }
    }
  }, [open, editing])

  const mutation = useMutation<void>({
    mutationFn: () => {
      const payload: CreateMarketPriceRequest = {
        plantTypeId: Number(plantTypeId),
        plantVarietyId: plantVarietyId ? Number(plantVarietyId) : null,
        pricePerUnit: Number(pricePerUnit),
        unit,
        recordedDate,
      }
      if (editing) return updateMarketPrice(gardenId, year, editing.id, payload)
      return createMarketPrice(gardenId, year, payload).then(() => {})
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['market-prices', gardenId, year] })
      onClose()
    },
  })

  const canSubmit = !!plantTypeId && !!pricePerUnit && !!recordedDate

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle>{editing ? 'Edit Market Price' : 'Add Market Price'}</DialogTitle>
        </DialogHeader>
        <form
          id="mp-form"
          onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}
          className="space-y-4"
        >
          {/* Plant type */}
          <div className="space-y-1">
            <Label htmlFor="mp-type">Plant Type</Label>
            <select
              id="mp-type"
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={plantTypeId}
              onChange={(e) => {
                setPlantTypeId(e.target.value ? Number(e.target.value) : '')
                setPlantVarietyId('')
              }}
              required
              disabled={!!editing}
            >
              <option value="">Select type…</option>
              {plantTypes.map(pt => (
                <option key={pt.id} value={pt.id}>{pt.name}</option>
              ))}
            </select>
          </div>

          {/* Variety (optional) */}
          <div className="space-y-1">
            <Label htmlFor="mp-variety">
              Variety <span className="text-muted-foreground">(optional — leave blank for type-wide price)</span>
            </Label>
            <select
              id="mp-variety"
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={plantVarietyId}
              onChange={(e) => setPlantVarietyId(e.target.value ? Number(e.target.value) : '')}
              disabled={!plantTypeId || !!editing}
            >
              <option value="">All varieties</option>
              {varieties.map(v => (
                <option key={v.id} value={v.id}>{v.name}</option>
              ))}
            </select>
          </div>

          {/* Price + unit */}
          <div className="flex gap-3">
            <div className="space-y-1 flex-1">
              <Label htmlFor="mp-price">Price ($)</Label>
              <Input
                id="mp-price"
                type="number"
                min={0.01}
                step="0.01"
                value={pricePerUnit}
                onChange={(e) => setPricePerUnit(e.target.value)}
                required
                placeholder="e.g. 3.50"
              />
            </div>
            <div className="space-y-1 w-36">
              <Label htmlFor="mp-unit">Unit</Label>
              <select
                id="mp-unit"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={unit}
                onChange={(e) => setUnit(e.target.value as HarvestUnit)}
              >
                {UNITS.map(u => (
                  <option key={u.value} value={u.value}>{u.label}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Date */}
          <div className="space-y-1">
            <Label htmlFor="mp-date">As of Date</Label>
            <Input
              id="mp-date"
              type="date"
              value={recordedDate}
              onChange={(e) => setRecordedDate(e.target.value)}
              required
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">Something went wrong. Please try again.</p>
          )}
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="submit" form="mp-form" disabled={mutation.isPending || !canSubmit}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
