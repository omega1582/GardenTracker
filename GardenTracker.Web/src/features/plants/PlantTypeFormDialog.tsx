import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createPlantType, updatePlantType } from '@/api/plants'
import type { PlantType } from '@/types/plant'
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

interface Props {
  open: boolean
  onClose: () => void
  editing?: PlantType
}

export default function PlantTypeFormDialog({ open, onClose, editing }: Props) {
  const qc = useQueryClient()
  const [name, setName] = useState('')

  useEffect(() => {
    if (open) setName(editing?.name ?? '')
  }, [open, editing])

  const mutation = useMutation<void | PlantType>({
    mutationFn: () =>
      editing ? updatePlantType(editing.id, name) : createPlantType(name),
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
