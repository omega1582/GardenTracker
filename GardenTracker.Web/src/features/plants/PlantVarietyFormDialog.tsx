import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createVariety, updateVariety } from '@/api/plants'
import type { PlantVariety } from '@/types/plant'
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

  useEffect(() => {
    if (open) {
      setName(editing?.name ?? '')
      setNotes(editing?.notes ?? '')
    }
  }, [open, editing])

  const mutation = useMutation<void | PlantVariety>({
    mutationFn: () =>
      editing
        ? updateVariety(editing.id, { name, notes: notes || undefined })
        : createVariety(plantTypeId, { name, notes: notes || undefined }),
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
              placeholder="Days to maturity, growing tips, etc."
            />
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
