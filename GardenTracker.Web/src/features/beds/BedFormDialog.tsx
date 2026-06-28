import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createBed, updateBed } from '@/api/beds'
import type { Bed } from '@/types/bed'
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
  gardenId: number
  editing?: Bed
}

export default function BedFormDialog({ open, onClose, gardenId, editing }: Props) {
  const qc = useQueryClient()
  const [name, setName] = useState('')
  const [lengthFt, setLengthFt] = useState('')
  const [widthFt, setWidthFt] = useState('')
  const [depthIn, setDepthIn] = useState('')
  const [material, setMaterial] = useState('')
  const [lifespanYears, setLifespanYears] = useState('10')
  const [installedDate, setInstalledDate] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (open) {
      setName(editing?.name ?? '')
      setLengthFt(editing?.lengthFt.toString() ?? '')
      setWidthFt(editing?.widthFt.toString() ?? '')
      setDepthIn(editing?.depthIn.toString() ?? '')
      setMaterial(editing?.material ?? '')
      setLifespanYears(editing?.expectedLifespanYears.toString() ?? '10')
      setInstalledDate(editing?.installedDate ?? '')
      setNotes(editing?.notes ?? '')
    }
  }, [open, editing])

  const mutation = useMutation<void | Bed>({
    mutationFn: () =>
      editing
        ? updateBed(gardenId, editing.id, {
            name,
            lengthFt: Number(lengthFt),
            widthFt: Number(widthFt),
            depthIn: Number(depthIn),
            material: material || undefined,
            expectedLifespanYears: Number(lifespanYears),
            installedDate,
            notes: notes || undefined,
          })
        : createBed(gardenId, {
            name,
            lengthFt: Number(lengthFt),
            widthFt: Number(widthFt),
            depthIn: Number(depthIn),
            material: material || undefined,
            expectedLifespanYears: Number(lifespanYears),
            installedDate,
            notes: notes || undefined,
          }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['beds', gardenId] })
      onClose()
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    mutation.mutate()
  }

  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{editing ? 'Edit Bed' : 'Add Bed'}</DialogTitle>
        </DialogHeader>
        <form id="bed-form" onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="b-name">Name</Label>
            <Input id="b-name" value={name} onChange={(e) => setName(e.target.value)} required maxLength={100} />
          </div>

          <div className="grid grid-cols-3 gap-3">
              <div className="space-y-1">
                <Label htmlFor="b-length">Length (ft)</Label>
                <Input id="b-length" type="number" step="0.1" min="0.1" value={lengthFt} onChange={(e) => setLengthFt(e.target.value)} required />
              </div>
              <div className="space-y-1">
                <Label htmlFor="b-width">Width (ft)</Label>
                <Input id="b-width" type="number" step="0.1" min="0.1" value={widthFt} onChange={(e) => setWidthFt(e.target.value)} required />
              </div>
              <div className="space-y-1">
                <Label htmlFor="b-depth">Depth (in)</Label>
                <Input id="b-depth" type="number" step="0.1" min="1" value={depthIn} onChange={(e) => setDepthIn(e.target.value)} required />
              </div>
            </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <Label htmlFor="b-material">Materials <span className="text-muted-foreground">(optional)</span></Label>
              <Input id="b-material" value={material} onChange={(e) => setMaterial(e.target.value)} maxLength={200} placeholder="e.g. Cedar, galvanized steel" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="b-lifespan">Lifespan (years)</Label>
              <Input id="b-lifespan" type="number" min="1" max="100" value={lifespanYears} onChange={(e) => setLifespanYears(e.target.value)} required />
            </div>
          </div>

          <div className="space-y-1">
            <Label htmlFor="b-installed">Installed Date</Label>
            <Input id="b-installed" type="date" value={installedDate} onChange={(e) => setInstalledDate(e.target.value)} required />
          </div>

          <div className="space-y-1">
            <Label htmlFor="b-notes">Notes <span className="text-muted-foreground">(optional)</span></Label>
            <Textarea id="b-notes" value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">Something went wrong. Please try again.</p>
          )}
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="submit" form="bed-form" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
