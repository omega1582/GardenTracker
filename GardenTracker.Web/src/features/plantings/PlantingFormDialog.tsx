import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createPlanting, updatePlanting } from '@/api/plantings'
import { getPlantTypes, getVarieties } from '@/api/plants'
import { getInventoryByVariety } from '@/api/inventory'
import type { Bed } from '@/types/bed'
import type { Planting, StartMethod } from '@/types/planting'
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

const START_METHODS: { value: StartMethod; label: string }[] = [
  { value: 'Seed', label: 'Seed' },
  { value: 'Transplant', label: 'Transplant' },
  { value: 'SeedSaved', label: 'Seed (saved)' },
  { value: 'Cutting', label: 'Cutting' },
]

interface Props {
  open: boolean
  onClose: () => void
  gardenId: number
  year: number
  beds: Bed[]
  editing?: Planting
}

export default function PlantingFormDialog({ open, onClose, gardenId, year, beds, editing }: Props) {
  const qc = useQueryClient()

  const [bedId, setBedId] = useState<number | ''>('')
  const [plantTypeId, setPlantTypeId] = useState<number | ''>('')
  const [plantVarietyId, setPlantVarietyId] = useState<number | ''>('')
  const [startMethod, setStartMethod] = useState<StartMethod>('Seed')
  const [quantity, setQuantity] = useState('')
  const [totalCost, setTotalCost] = useState('0')
  const [notes, setNotes] = useState('')
  const [inventoryItemId, setInventoryItemId] = useState<number | ''>('')
  const [quantityUsed, setQuantityUsed] = useState('')

  const { data: plantTypes = [] } = useQuery({
    queryKey: ['plant-types'],
    queryFn: getPlantTypes,
  })

  const { data: varieties = [] } = useQuery({
    queryKey: ['varieties', plantTypeId],
    queryFn: () => getVarieties(Number(plantTypeId)),
    enabled: !!plantTypeId,
  })

  const { data: inventoryItems = [] } = useQuery({
    queryKey: ['inventory', 'variety', plantVarietyId],
    queryFn: () => getInventoryByVariety(Number(plantVarietyId)),
    enabled: !!plantVarietyId,
  })

  const availableInventory = inventoryItems.filter(i => i.quantityRemaining > 0)

  useEffect(() => {
    if (open) {
      if (editing) {
        setBedId(editing.bedId)
        setPlantVarietyId(editing.plantVarietyId)
        setStartMethod(editing.startMethod)
        setQuantity(String(editing.quantity))
        setTotalCost(String(editing.totalCost))
        setNotes(editing.notes ?? '')
        setInventoryItemId(editing.inventoryItemId ?? '')
        setQuantityUsed(editing.quantityUsedFromInventory ? String(editing.quantityUsedFromInventory) : '')
        setPlantTypeId('')
      } else {
        setBedId(beds.length === 1 ? beds[0].id : '')
        setPlantTypeId('')
        setPlantVarietyId('')
        setStartMethod('Seed')
        setQuantity('')
        setTotalCost('0')
        setNotes('')
        setInventoryItemId('')
        setQuantityUsed('')
      }
    }
  }, [open, editing, beds])

  function handlePlantTypeChange(val: string) {
    setPlantTypeId(val ? Number(val) : '')
    setPlantVarietyId('')
    setInventoryItemId('')
    setQuantityUsed('')
  }

  function handleVarietyChange(val: string) {
    setPlantVarietyId(val ? Number(val) : '')
    setInventoryItemId('')
    setQuantityUsed('')
  }

  const mutation = useMutation<void>({
    mutationFn: () => {
      const invId = inventoryItemId ? Number(inventoryItemId) : null
      const invQty = quantityUsed ? Number(quantityUsed) : null

      if (editing) {
        return updatePlanting(gardenId, year, editing.id, {
          startMethod,
          quantity: Number(quantity),
          totalCost: Number(totalCost),
          notes: notes || null,
          inventoryItemId: invId,
          quantityUsedFromInventory: invQty,
        })
      }
      return createPlanting(gardenId, year, {
        bedId: Number(bedId),
        plantVarietyId: Number(plantVarietyId),
        startMethod,
        quantity: Number(quantity),
        totalCost: Number(totalCost),
        notes: notes || null,
        inventoryItemId: invId,
        quantityUsedFromInventory: invQty,
      }).then(() => {})
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['plantings', gardenId, year] })
      qc.invalidateQueries({ queryKey: ['inventory'] })
      onClose()
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate()
  }

  const canSubmit = editing
    ? !!quantity
    : !!bedId && !!plantVarietyId && !!quantity

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-sm max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{editing ? 'Edit Planting' : `Add Planting — ${year}`}</DialogTitle>
        </DialogHeader>
        <form id="planting-form" onSubmit={handleSubmit} className="space-y-4">

          {/* Bed — fixed on edit */}
          {editing ? (
            <p className="text-sm text-muted-foreground">Bed: <span className="text-foreground font-medium">{editing.bedName}</span></p>
          ) : (
            <div className="space-y-1">
              <Label htmlFor="plt-bed">Bed</Label>
              <select
                id="plt-bed"
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
                <Label htmlFor="plt-ptype">Plant Type</Label>
                <select
                  id="plt-ptype"
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
                <Label htmlFor="plt-variety">Variety</Label>
                <select
                  id="plt-variety"
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={plantVarietyId}
                  onChange={(e) => handleVarietyChange(e.target.value)}
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

          {/* Start method */}
          <div className="space-y-1">
            <Label htmlFor="plt-method">Start Method</Label>
            <select
              id="plt-method"
              className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={startMethod}
              onChange={(e) => setStartMethod(e.target.value as StartMethod)}
            >
              {START_METHODS.map((m) => (
                <option key={m.value} value={m.value}>{m.label}</option>
              ))}
            </select>
          </div>

          {/* Quantity */}
          <div className="space-y-1">
            <Label htmlFor="plt-qty">Plants in Ground</Label>
            <Input
              id="plt-qty"
              type="number"
              min={1}
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              required
              placeholder="e.g. 6"
            />
          </div>

          {/* Cost */}
          <div className="space-y-1">
            <Label htmlFor="plt-cost">Total Cost ($)</Label>
            <Input
              id="plt-cost"
              type="number"
              min={0}
              step="0.01"
              value={totalCost}
              onChange={(e) => setTotalCost(e.target.value)}
              required
            />
          </div>

          {/* Inventory link — only shown when a variety is selected and inventory exists */}
          {(!!plantVarietyId || editing) && availableInventory.length > 0 && (
            <div className="space-y-3 rounded-md border border-border p-3">
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">Inventory Link (optional)</p>
              <div className="space-y-1">
                <Label htmlFor="plt-inv">Use from Inventory</Label>
                <select
                  id="plt-inv"
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={inventoryItemId}
                  onChange={(e) => {
                    setInventoryItemId(e.target.value ? Number(e.target.value) : '')
                    setQuantityUsed('')
                  }}
                >
                  <option value="">None</option>
                  {availableInventory.map((i) => (
                    <option key={i.id} value={i.id}>
                      {i.type} — {i.supplierName ?? 'No supplier'} ({i.quantityRemaining} remaining) · {i.purchaseDate}
                    </option>
                  ))}
                </select>
              </div>
              {inventoryItemId && (
                <div className="space-y-1">
                  <Label htmlFor="plt-invqty">Seeds / Plants Used</Label>
                  <Input
                    id="plt-invqty"
                    type="number"
                    min={1}
                    value={quantityUsed}
                    onChange={(e) => setQuantityUsed(e.target.value)}
                    placeholder="e.g. 12"
                  />
                  <p className="text-xs text-muted-foreground">How many you sowed or used — may differ from plants in ground.</p>
                </div>
              )}
            </div>
          )}

          {/* Notes */}
          <div className="space-y-1">
            <Label htmlFor="plt-notes">Notes <span className="text-muted-foreground">(optional)</span></Label>
            <Textarea
              id="plt-notes"
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
          <Button type="submit" form="planting-form" disabled={mutation.isPending || !canSubmit}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
