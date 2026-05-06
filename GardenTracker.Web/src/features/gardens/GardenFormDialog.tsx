import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createGarden, updateGarden } from '@/api/gardens'
import type { Garden } from '@/types/garden'
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
  editing?: Garden
}

export default function GardenFormDialog({ open, onClose, editing }: Props) {
  const qc = useQueryClient()
  const [name, setName] = useState('')
  const [location, setLocation] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (open) {
      setName(editing?.name ?? '')
      setLocation(editing?.location ?? '')
      setNotes(editing?.notes ?? '')
    }
  }, [open, editing])

  const mutation = useMutation<void | Garden>({
    mutationFn: () =>
      editing
        ? updateGarden(editing.id, { name, location: location || undefined, notes: notes || undefined })
        : createGarden({ name, location: location || undefined, notes: notes || undefined }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['gardens'] })
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
          <DialogTitle>{editing ? 'Edit Garden' : 'New Garden'}</DialogTitle>
        </DialogHeader>
        <form id="garden-form" onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="g-name">Name</Label>
            <Input
              id="g-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              maxLength={150}
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="g-location">Location <span className="text-muted-foreground">(optional)</span></Label>
            <Input
              id="g-location"
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              maxLength={250}
              placeholder="e.g. Backyard, south fence"
            />
          </div>
          <div className="space-y-1">
            <Label htmlFor="g-notes">Notes <span className="text-muted-foreground">(optional)</span></Label>
            <Textarea
              id="g-notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={3}
            />
          </div>
          {mutation.isError && (
            <p className="text-sm text-destructive">Something went wrong. Please try again.</p>
          )}
        </form>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="submit" form="garden-form" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
