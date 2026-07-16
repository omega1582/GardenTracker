import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createHarvest, updateHarvest } from '@/api/harvests'
import { getPlantTypes, getVarieties } from '@/api/plants'
import { getGardens } from '@/api/gardens'
import { getBeds } from '@/api/beds'
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
  { value: 'Ounces', label: 'Ounces (oz)' },
  { value: 'Pounds', label: 'Pounds (lbs)' },
  { value: 'Count', label: 'Count (each)' },
  { value: 'Bunch', label: 'Bunch' },
]

interface Props {
  open: boolean
  onClose: () => void
  gardenId?: number
  year?: number
  beds?: Bed[]
  editing?: Harvest
}

export default function HarvestFormDialog({ open, onClose, gardenId, year, beds, editing }: Props) {
  const qc = useQueryClient()

  const [selectedGardenId, setSelectedGardenId] = useState<number | ''>('')
  const [bedId, setBedId] = useState<number | ''>('')
  const [plantTypeId, setPlantTypeId] = useState<number | ''>('')
  const [plantVarietyId, setPlantVarietyId] = useState<number | ''>('')
  const [quantity, setQuantity] = useState('')
  const [lbs, setLbs] = useState('')
  const [oz, setOz] = useState('')
  const [unit, setUnit] = useState<HarvestUnit>('Ounces')
  const [harvestDate, setHarvestDate] = useState('')
  const [notes, setNotes] = useState('')

  // Query to fetch all gardens (only if gardenId is not provided as prop)
  const { data: allGardens = [] } = useQuery({
    queryKey: ['gardens'],
    queryFn: getGardens,
    enabled: !gardenId && open,
  })

  const activeGardenId = gardenId ?? (selectedGardenId || 0)

  // Query to fetch beds for selected garden (only if beds is not provided as prop)
  const { data: fetchedBeds = [] } = useQuery({
    queryKey: ['beds', activeGardenId],
    queryFn: () => getBeds(Number(activeGardenId)),
    enabled: !beds && !!activeGardenId && open,
  })

  const bedsList = beds ?? fetchedBeds
  const sortedBeds = [...bedsList].sort((a, b) =>
    a.name.localeCompare(b.name, undefined, { numeric: true, sensitivity: 'base' })
  )

  const { data: plantTypes = [] } = useQuery({
    queryKey: ['plant-types'],
    queryFn: getPlantTypes,
    enabled: open,
  })

  const { data: varieties = [] } = useQuery({
    queryKey: ['varieties', plantTypeId],
    queryFn: () => getVarieties(Number(plantTypeId)),
    enabled: !!plantTypeId && open,
  })

  // Initialize form state
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

        // Initialize lbs/oz based on stored quantity and unit
        if (editing.unit === 'Ounces') {
          const totalOz = Number(editing.quantity)
          const calculatedLbs = Math.floor(totalOz / 16)
          const calculatedOz = Number((totalOz % 16).toFixed(2))
          setLbs(calculatedLbs > 0 ? String(calculatedLbs) : '')
          setOz(calculatedOz > 0 ? String(calculatedOz) : '')
        } else if (editing.unit === 'Pounds') {
          const totalLbs = Number(editing.quantity)
          const calculatedLbs = Math.floor(totalLbs)
          const calculatedOz = Number(((totalLbs - calculatedLbs) * 16).toFixed(2))
          setLbs(calculatedLbs > 0 ? String(calculatedLbs) : '')
          setOz(calculatedOz > 0 ? String(calculatedOz) : '')
        } else {
          setLbs('')
          setOz('')
        }
      } else {
        setSelectedGardenId(gardenId ?? '')
        setBedId(beds && beds.length === 1 ? beds[0].id : '')
        setPlantTypeId('')
        setPlantVarietyId('')
        setQuantity('')
        setLbs('')
        setOz('')
        setUnit('Ounces') // Default to Ounces as requested
        setHarvestDate(new Date().toISOString().slice(0, 10))
        setNotes('')
      }
    }
  }, [open, editing, beds, gardenId])

  // Reset bed when selected garden changes (only if not editing and not locked to a gardenId prop)
  useEffect(() => {
    if (!editing && !gardenId) {
      setBedId('')
    }
  }, [selectedGardenId, editing, gardenId])

  // Synchronize split lbs/oz inputs to the single quantity field
  useEffect(() => {
    if (unit === 'Pounds' || unit === 'Ounces') {
      const l = lbs ? Number(lbs) : 0
      const o = oz ? Number(oz) : 0
      if (unit === 'Ounces') {
        const total = l * 16 + o
        setQuantity(total > 0 ? String(Number(total.toFixed(2))) : '')
      } else {
        const total = l + o / 16
        setQuantity(total > 0 ? String(Number(total.toFixed(4))) : '')
      }
    }
  }, [lbs, oz, unit])

  // Extract year from harvestDate
  const calculatedYear = harvestDate ? Number(harvestDate.slice(0, 4)) : new Date().getFullYear()
  const activeYear = year ?? calculatedYear

  function handlePlantTypeChange(val: string) {
    setPlantTypeId(val ? Number(val) : '')
    setPlantVarietyId('')
  }

  function handleUnitChange(val: HarvestUnit) {
    setUnit(val)
    if (val === 'Count' || val === 'Bunch') {
      setLbs('')
      setOz('')
      setQuantity('')
    }
  }

  const mutation = useMutation<void>({
    mutationFn: () => {
      if (!activeGardenId) {
        throw new Error('Garden selection is required.')
      }
      if (!activeYear) {
        throw new Error('Harvest date / year is required.')
      }

      if (editing) {
        const payload: UpdateHarvestRequest = {
          quantity: Number(quantity),
          unit,
          harvestDate,
          notes: notes || null,
        }
        return updateHarvest(Number(activeGardenId), activeYear, editing.id, payload)
      }
      const payload: CreateHarvestRequest = {
        bedId: Number(bedId),
        plantVarietyId: Number(plantVarietyId),
        quantity: Number(quantity),
        unit,
        harvestDate,
        notes: notes || null,
      }
      return createHarvest(Number(activeGardenId), activeYear, payload).then(() => {})
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['harvests'] })
      qc.invalidateQueries({ queryKey: ['harvests-all-gardens'] })
      qc.invalidateQueries({ queryKey: ['reports'] })
      onClose()
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate()
  }

  const canSubmit = editing
    ? !!quantity && !!harvestDate
    : !!activeGardenId && !!bedId && !!plantVarietyId && !!quantity && !!harvestDate

  return (
    <Dialog
      open={open}
      onOpenChange={(o, eventDetails) => {
        if (!o) {
          const reason = eventDetails?.reason
          if (reason === 'outside-press' || reason === 'escape-key') {
            return
          }
          onClose()
        }
      }}
    >
      <DialogContent className="max-w-sm" showCloseButton={false}>
        <DialogHeader>
          <DialogTitle>{editing ? 'Edit Harvest' : `Log Harvest — ${activeYear}`}</DialogTitle>
        </DialogHeader>
        <form id="harvest-form" onSubmit={handleSubmit} className="space-y-4">

          {/* Garden Selection (only if not pre-locked) */}
          {!gardenId && !editing && (
            <div className="space-y-1">
              <Label htmlFor="hv-garden">Garden</Label>
              <select
                id="hv-garden"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={selectedGardenId}
                onChange={(e) => setSelectedGardenId(e.target.value ? Number(e.target.value) : '')}
                required
              >
                <option value="">Select garden…</option>
                {allParamsGarden(allGardens).map((g) => (
                  <option key={g.id} value={g.id}>{g.name}</option>
                ))}
              </select>
            </div>
          )}

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
                disabled={!activeGardenId}
              >
                <option value="">{activeGardenId ? 'Select bed…' : 'Select garden first…'}</option>
                {sortedBeds.map((b) => (
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
          <div className="flex gap-3 items-end">
            {unit === 'Pounds' || unit === 'Ounces' ? (
              <>
                <div className="space-y-1 flex-1">
                  <Label htmlFor="hv-lbs">Lbs</Label>
                  <Input
                    id="hv-lbs"
                    type="number"
                    min={0}
                    step="1"
                    value={lbs}
                    onChange={(e) => setLbs(e.target.value)}
                    placeholder="0"
                  />
                </div>
                <div className="space-y-1 flex-1">
                  <Label htmlFor="hv-oz">Oz</Label>
                  <Input
                    id="hv-oz"
                    type="number"
                    min={0}
                    max={15.99}
                    step="0.01"
                    value={oz}
                    onChange={(e) => setOz(e.target.value)}
                    placeholder="0.0"
                  />
                </div>
              </>
            ) : (
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
            )}
            
            <div className="space-y-1 w-36">
              <Label htmlFor="hv-unit">Unit</Label>
              <select
                id="hv-unit"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={unit}
                onChange={(e) => handleUnitChange(e.target.value as HarvestUnit)}
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

function allParamsGarden(gardens: any[]) {
  return gardens;
}
