import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createInventoryItem, updateInventoryItem } from '@/api/inventory'
import { getPlantTypes, getVarieties } from '@/api/plants'
import type { InventoryItem, InventoryType } from '@/types/inventory'
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

interface Props {
  open: boolean
  onClose: () => void
  editing?: InventoryItem
}

export default function InventoryFormDialog({ open, onClose, editing }: Props) {
  const qc = useQueryClient()

  const [plantTypeId, setPlantTypeId] = useState<number | ''>('')
  const [plantVarietyId, setPlantVarietyId] = useState<number | ''>('')
  const [type, setType] = useState<InventoryType>('Seed')
  const [quantityPurchased, setQuantityPurchased] = useState('')
  const [totalCost, setTotalCost] = useState('')
  const [purchaseDate, setPurchaseDate] = useState('')
  const [notes, setNotes] = useState('')

  const { data: plantTypes = [] } = useQuery({
    queryKey: ['plant-types'],
    queryFn: getPlantTypes,
  })

  const { data: varieties = [] } = useQuery({
    queryKey: ['varieties', plantTypeId],
    queryFn: () => (plantTypeId ? getVarieties(Number(plantTypeId)) : Promise.resolve([])),
    enabled: !!plantTypeId,
  })

  useEffect(() => {
    if (open) {
      if (editing) {
        setPlantVarietyId(editing.plantVarietyId)
        setType(editing.type)
        setQuantityPurchased(String(editing.quantityPurchased))
        setTotalCost(String(editing.totalCost))
        setPurchaseDate(editing.purchaseDate)
        setNotes(editing.notes ?? '')
        // We don't know the plantTypeId from the item directly; let the user see it by variety name
        setPlantTypeId('')
      } else {
        setPlantTypeId('')
        setPlantVarietyId('')
        setType('Seed')
        setQuantityPurchased('')
        setTotalCost('')
        setPurchaseDate(new Date().toISOString().slice(0, 10))
        setNotes('')
      }
    }
  }, [open, editing])

  // When plant type changes, reset variety
  function handlePlantTypeChange(val: string) {
    setPlantTypeId(val ? Number(val) : '')
    setPlantVarietyId('')
  }

  const mutation = useMutation<void>({
    mutationFn: () => {
      if (editing) {
        return updateInventoryItem(editing.id, {
          quantityPurchased: Number(quantityPurchased),
          totalCost: Number(totalCost),
          purchaseDate,
          notes: notes || undefined,
        })
      }
      return createInventoryItem({
        plantVarietyId: Number(plantVarietyId),
        type,
        quantityPurchased: Number(quantityPurchased),
        totalCost: Number(totalCost),
        purchaseDate,
        notes: notes || undefined,
      }).then(() => {})
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inventory'] })
      onClose()
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate()
  }

  const canSubmit = editing
    ? !!quantityPurchased && !!totalCost && !!purchaseDate
    : !!plantVarietyId && !!quantityPurchased && !!totalCost && !!purchaseDate

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle>{editing ? 'Edit Inventory Item' : 'Add Inventory'}</DialogTitle>
        </DialogHeader>
        <form id="inv-form" onSubmit={handleSubmit} className="space-y-4">
          {!editing && (
            <>
              <div className="space-y-1">
                <Label htmlFor="inv-ptype">Plant Type</Label>
                <select
                  id="inv-ptype"
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
                <Label htmlFor="inv-variety">Variety</Label>
                <select
                  id="inv-variety"
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
              <div className="space-y-1">
                <Label htmlFor="inv-type">Type</Label>
                <select
                  id="inv-type"
                  className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={type}
                  onChange={(e) => setType(e.target.value as InventoryType)}
                >
                  <option value="Seed">Seed</option>
                  <option value="Plant">Plant</option>
                </select>
              </div>
            </>
          )}

          {editing && (
            <p className="text-sm text-muted-foreground">
              {editing.plantTypeName} — {editing.plantVarietyName} ({editing.type})
            </p>
          )}

          <div className="space-y-1">
            <Label htmlFor="inv-qty">Quantity Purchased</Label>
            <Input
              id="inv-qty"
              type="number"
              min={1}
              value={quantityPurchased}
              onChange={(e) => setQuantityPurchased(e.target.value)}
              required
              placeholder="e.g. 50"
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="inv-cost">Total Cost ($)</Label>
            <Input
              id="inv-cost"
              type="number"
              min={0}
              step="0.01"
              value={totalCost}
              onChange={(e) => setTotalCost(e.target.value)}
              required
              placeholder="e.g. 4.99"
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="inv-date">Purchase Date</Label>
            <Input
              id="inv-date"
              type="date"
              value={purchaseDate}
              onChange={(e) => setPurchaseDate(e.target.value)}
              required
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="inv-notes">Notes <span className="text-muted-foreground">(optional)</span></Label>
            <Textarea
              id="inv-notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
              placeholder="Seed packet brand, batch number, etc."
            />
          </div>
          {mutation.isError && (
            <p className="text-sm text-destructive">Something went wrong. Please try again.</p>
          )}
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="submit" form="inv-form" disabled={mutation.isPending || !canSubmit}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
