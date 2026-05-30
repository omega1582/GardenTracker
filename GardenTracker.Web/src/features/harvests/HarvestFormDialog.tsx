import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createHarvest, updateHarvest } from '@/api/harvests'
import { getPlantTypes, getVarieties } from '@/api/plants'
import type { Bed } from '@/types/bed'
import type { Harvest, HarvestUnit, CreateHarvestRequest, UpdateHarvestRequest } from '@/types/harvest'
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

const UNITS: { value: HarvestUnit; label: string }[] = [
  { value: 'Pounds', label: 'Pounds (lbs)' },
  { value: 'Ounces', label: 'Ounces (oz)' },
  { value: 'Count', label: 'Count (each)' },
  { value: 'Bunch', label: 'Bunch' },
]

interface Props {
  open: boolean
  onClose: () => void
  gardenId: number
  year: number
  beds: Bed[]
  editing?: Harvest
}

export default function HarvestFormDialog({ open, onClose, gardenId, year, beds, editing }: Props) {
  const qc = useQueryClient()

  const [bedId, setBedId] = useState<number | ''>('')
  const [plantTypeId, setPlantTypeId] = useState<number | ''>('')
  const [plantVarietyId, setPlantVarietyId] = useState<number | ''>('')
  const [quantity, setQuantity] = useState('')
  const [unit, setUnit] = useState<HarvestUnit>('Pounds')
  const [harvestDate, setHarvestDate] = useState('')
  const [notes, setNotes] = useState('')

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
        setBedId(editing.bedId)
        setPlantVarietyId(editing.plantVarietyId)
        setQuantity(String(editing.quantity))
        setUnit(editing.unit)
        setHarvestDate(editing.harvestDate)
        setNotes(editing.notes ?? '')
        setPlantTypeId('')
      } else {
        setBedId(beds.length === 1 ? beds[0].id : '')
        setPlantTypeId('')
        setPlantVarietyId('')
        setQuantity('')
        setUnit('Pounds')
        setHarvestDate(new Date().toISOString().slice(0, 10))
        setNotes('')
      }
    }
  }, [open, editing, beds])

  function handlePlantTypeChange(val: string) {
    setPlantTypeId(val ? Number(val) : '')
    setPlantVarietyId('')
  }

  const mutation = useMutation<void>({
    mutationFn: () => {
      if (editing) {
        const payload: UpdateHarvestRequest = {
          quantity: Number(quantity),
          unit,
          harvestDate,
          notes: notes || null,
        }
        return updateHarvest(gardenId, year, editing.id, payload)
      }
      const payload: CreateHarvestRequest = {
        bedId: Number(bedId),
        plantVarietyId: Number(plantVarietyId),
        quantity: Number(quantity),
        unit,
        harvestDate,
        notes: notes || null,
      }
      return createHarvest(gardenId, year, payload).then(() => {})
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['harvests', gardenId, year] })
      onClose()
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate()
  }

  const canSubmit = editing
    ? !!quantity && !!harvestDate
    : !!bedId && !!plantVarietyId && !!quantity && !!harvestDate

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle>{editing ? 'Edit Harvest' : `Log Harvest — ${year}`}</DialogTitle>
        </DialogHeader>
        <form id="harvest-form" onSubmit={handleSubmit} className="space-y-4">

          {/* Bed — fixed on edit */}
          {editing ? (
            <p className="text-sm text-muted-foreground">Bed: <span className="text-foreground font-medium">{editing.bedName}</span></p>
          ) : (
            <div className="space-y-1">
              <Label htmlFor="hv-bed">Bed</Label>
              <select
                id="hv-bed"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={bedId}
                onChange={(e) => setBedId(e.target.value ? Number(e.target.value) : '')}
                required
              >
                <option value="">Select bed…</option>
                {beds.map((b) => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            </div>
          )}

          {/* Plant type + variety — fixed on edit */}
          {editing ? (
            <p className="text-sm text-muted-foreground">
              Variety: <span className="text-foreground font-medium">{editing.plantTypeName} — {editing.plantVarietyName}</span>
            </p>
          ) : (
            <>
              <div className="space-y-1">
                <Label htmlFor="hv-ptype">Plant Type</Label>
                <select
                  id="hv-ptype"
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={plantTypeId}
                  onChange={(e) => handlePlantTypeChange(e.target.value)}
                  required
                >
                  <option value="">Select plant type…</option>
                  {plantTypes.map((pt) => (
                    <option key={pt.id} value={pt.id}>{pt.name}</option>
                  ))}
                </select>
              </div>
              <div className="space-y-1">
                <Label htmlFor="hv-variety">Variety</Label>
                <select
                  id="hv-variety"
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={plantVarietyId}
                  onChange={(e) => setPlantVarietyId(e.target.value ? Number(e.target.value) : '')}
                  required
                  disabled={!plantTypeId}
                >
                  <option value="">Select variety…</option>
                  {varieties.map((v) => (
                    <option key={v.id} value={v.id}>{v.name}</option>
                  ))}
                </select>
              </div>
            </>
          )}

          {/* Quantity + unit */}
          <div className="flex gap-3">
            <div className="space-y-1 flex-1">
              <Label htmlFor="hv-qty">Quantity</Label>
              <Input
                id="hv-qty"
                type="number"
                min={0}
                step="0.01"
                value={quantity}
                onChange={(e) => setQuantity(e.target.value)}
                required
                placeholder="e.g. 2.5"
              />
            </div>
            <div className="space-y-1 w-36">
              <Label htmlFor="hv-unit">Unit</Label>
              <select
                id="hv-unit"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={unit}
                onChange={(e) => setUnit(e.target.value as HarvestUnit)}
              >
                {UNITS.map((u) => (
                  <option key={u.value} value={u.value}>{u.label}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Date */}
          <div className="space-y-1">
            <Label htmlFor="hv-date">Harvest Date</Label>
            <Input
              id="hv-date"
              type="date"
              value={harvestDate}
              onChange={(e) => setHarvestDate(e.target.value)}
              required
            />
          </div>

          {/* Notes */}
          <div className="space-y-1">
            <Label htmlFor="hv-notes">Notes <span className="text-muted-foreground">(optional)</span></Label>
            <Textarea
              id="hv-notes"
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
          <Button type="submit" form="harvest-form" disabled={mutation.isPending || !canSubmit}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
