import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createVariety, updateVariety } from '@/api/plants'
import type { PlantVariety, GrowthHabit, SunPreference } from '@/types/plant'
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

const selectClass =
  'h-8 w-full rounded-lg border border-input bg-transparent px-2.5 py-1 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50'

interface Props {
  open: boolean
  onClose: () => void
  plantTypeId: number
  plantTypeName: string
  editing?: PlantVariety
}

export default function PlantVarietyFormDialog({
  open,
  onClose,
  plantTypeId,
  plantTypeName,
  editing,
}: Props) {
  const qc = useQueryClient()
  const [name, setName] = useState('')
  const [notes, setNotes] = useState('')
  const [growthHabit, setGrowthHabit] = useState('')
  const [daysToMaturity, setDaysToMaturity] = useState('')
  const [spacingInches, setSpacingInches] = useState('')
  const [sunPreference, setSunPreference] = useState('')
  const [isPerennial, setIsPerennial] = useState('')

  useEffect(() => {
    if (open) {
      setName(editing?.name ?? '')
      setNotes(editing?.notes ?? '')
      setGrowthHabit(editing?.growthHabit ?? '')
      setDaysToMaturity(editing?.daysToMaturity?.toString() ?? '')
      setSpacingInches(editing?.spacingInches?.toString() ?? '')
      setSunPreference(editing?.sunPreference ?? '')
      setIsPerennial(editing?.isPerennial == null ? '' : editing.isPerennial ? 'true' : 'false')
    }
  }, [open, editing])

  const mutation = useMutation<void | PlantVariety>({
    mutationFn: () => {
      const payload = {
        name,
        notes: notes || null,
        growthHabit: (growthHabit || null) as GrowthHabit | null,
        daysToMaturity: daysToMaturity ? parseInt(daysToMaturity) : null,
        spacingInches: spacingInches ? parseInt(spacingInches) : null,
        sunPreference: (sunPreference || null) as SunPreference | null,
        isPerennial: isPerennial === '' ? null : isPerennial === 'true',
      }
      return editing ? updateVariety(editing.id, payload) : createVariety(plantTypeId, payload)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['varieties', plantTypeId] })
      onClose()
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate()
  }

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle>
            {editing ? 'Edit Variety' : `Add ${plantTypeName} Variety`}
          </DialogTitle>
        </DialogHeader>
        <form id="variety-form" onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="v-name">Variety Name</Label>
            <Input
              id="v-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              maxLength={150}
              placeholder="e.g. Cherokee Purple"
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="v-notes">Notes <span className="text-muted-foreground">(optional)</span></Label>
            <Textarea
              id="v-notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
              placeholder="Growing tips, etc."
            />
          </div>

          <div className="space-y-1">
            <Label htmlFor="v-habit">Growth Habit <span className="text-muted-foreground">(optional)</span></Label>
            <select id="v-habit" className={selectClass} value={growthHabit} onChange={(e) => setGrowthHabit(e.target.value)}>
              <option value="">Inherit from plant type</option>
              <option value="Upright">Upright</option>
              <option value="Vining">Vining</option>
              <option value="Bushy">Bushy</option>
              <option value="Spreading">Spreading</option>
              <option value="Rosette">Rosette</option>
            </select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <Label htmlFor="v-dtm">Days to Maturity <span className="text-muted-foreground">(opt)</span></Label>
              <Input id="v-dtm" type="number" min={1} value={daysToMaturity} onChange={(e) => setDaysToMaturity(e.target.value)} placeholder="e.g. 80" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="v-spacing">Spacing (in.) <span className="text-muted-foreground">(opt)</span></Label>
              <Input id="v-spacing" type="number" min={1} value={spacingInches} onChange={(e) => setSpacingInches(e.target.value)} placeholder="e.g. 24" />
            </div>
          </div>

          <div className="space-y-1">
            <Label htmlFor="v-sun">Sun Preference <span className="text-muted-foreground">(optional)</span></Label>
            <select id="v-sun" className={selectClass} value={sunPreference} onChange={(e) => setSunPreference(e.target.value)}>
              <option value="">Inherit from plant type</option>
              <option value="FullSun">Full Sun</option>
              <option value="PartialSun">Partial Sun</option>
              <option value="Shade">Shade</option>
            </select>
          </div>

          <div className="space-y-1">
            <Label htmlFor="v-perennial">Annual / Perennial <span className="text-muted-foreground">(optional)</span></Label>
            <select id="v-perennial" className={selectClass} value={isPerennial} onChange={(e) => setIsPerennial(e.target.value)}>
              <option value="">Inherit from plant type</option>
              <option value="false">Annual</option>
              <option value="true">Perennial</option>
            </select>
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">Something went wrong. Please try again.</p>
          )}
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="submit" form="variety-form" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
