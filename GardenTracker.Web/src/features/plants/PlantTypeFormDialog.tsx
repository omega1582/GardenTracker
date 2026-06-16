import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createPlantType, updatePlantType } from '@/api/plants'
import type { PlantType, GrowthHabit, SunPreference } from '@/types/plant'
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

const selectClass =
  'h-8 w-full rounded-lg border border-input bg-transparent px-2.5 py-1 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50'

interface Props {
  open: boolean
  onClose: () => void
  editing?: PlantType
}

export default function PlantTypeFormDialog({ open, onClose, editing }: Props) {
  const qc = useQueryClient()
  const [name, setName] = useState('')
  const [growthHabit, setGrowthHabit] = useState('')
  const [daysToMaturity, setDaysToMaturity] = useState('')
  const [spacingInches, setSpacingInches] = useState('')
  const [sunPreference, setSunPreference] = useState('')
  const [isPerennial, setIsPerennial] = useState('')

  useEffect(() => {
    if (open) {
      setName(editing?.name ?? '')
      setGrowthHabit(editing?.growthHabit ?? '')
      setDaysToMaturity(editing?.daysToMaturity?.toString() ?? '')
      setSpacingInches(editing?.spacingInches?.toString() ?? '')
      setSunPreference(editing?.sunPreference ?? '')
      setIsPerennial(editing?.isPerennial == null ? '' : editing.isPerennial ? 'true' : 'false')
    }
  }, [open, editing])

  const mutation = useMutation<void | PlantType>({
    mutationFn: () => {
      const payload = {
        name,
        growthHabit: (growthHabit || null) as GrowthHabit | null,
        daysToMaturity: daysToMaturity ? parseInt(daysToMaturity) : null,
        spacingInches: spacingInches ? parseInt(spacingInches) : null,
        sunPreference: (sunPreference || null) as SunPreference | null,
        isPerennial: isPerennial === '' ? null : isPerennial === 'true',
      }
      return editing ? updatePlantType(editing.id, payload) : createPlantType(payload)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['plant-types'] })
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
          <DialogTitle>{editing ? 'Edit Plant Type' : 'Add Plant Type'}</DialogTitle>
        </DialogHeader>
        <form id="plant-type-form" onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="pt-name">Name</Label>
            <Input
              id="pt-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              maxLength={100}
              placeholder="e.g. Tomato"
            />
          </div>

          <div className="space-y-1">
            <Label htmlFor="pt-habit">Growth Habit <span className="text-muted-foreground">(optional)</span></Label>
            <select id="pt-habit" className={selectClass} value={growthHabit} onChange={(e) => setGrowthHabit(e.target.value)}>
              <option value="">Not set</option>
              <option value="Upright">Upright</option>
              <option value="Vining">Vining</option>
              <option value="Bushy">Bushy</option>
              <option value="Spreading">Spreading</option>
              <option value="Rosette">Rosette</option>
            </select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <Label htmlFor="pt-dtm">Days to Maturity <span className="text-muted-foreground">(opt)</span></Label>
              <Input id="pt-dtm" type="number" min={1} value={daysToMaturity} onChange={(e) => setDaysToMaturity(e.target.value)} placeholder="e.g. 75" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="pt-spacing">Spacing (in.) <span className="text-muted-foreground">(opt)</span></Label>
              <Input id="pt-spacing" type="number" min={1} value={spacingInches} onChange={(e) => setSpacingInches(e.target.value)} placeholder="e.g. 18" />
            </div>
          </div>

          <div className="space-y-1">
            <Label htmlFor="pt-sun">Sun Preference <span className="text-muted-foreground">(optional)</span></Label>
            <select id="pt-sun" className={selectClass} value={sunPreference} onChange={(e) => setSunPreference(e.target.value)}>
              <option value="">Not set</option>
              <option value="FullSun">Full Sun</option>
              <option value="PartialSun">Partial Sun</option>
              <option value="Shade">Shade</option>
            </select>
          </div>

          <div className="space-y-1">
            <Label htmlFor="pt-perennial">Annual / Perennial <span className="text-muted-foreground">(optional)</span></Label>
            <select id="pt-perennial" className={selectClass} value={isPerennial} onChange={(e) => setIsPerennial(e.target.value)}>
              <option value="">Not set</option>
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
          <Button type="submit" form="plant-type-form" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
