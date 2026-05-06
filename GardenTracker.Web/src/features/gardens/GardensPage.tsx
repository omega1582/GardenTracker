import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getGardens, deleteGarden } from '@/api/gardens'
import type { Garden } from '@/types/garden'
import GardenFormDialog from './GardenFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'

export default function GardensPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editing, setEditing] = useState<Garden | undefined>()

  const { data: gardens = [], isLoading, isError } = useQuery({
    queryKey: ['gardens'],
    queryFn: getGardens,
  })

  const deleteMutation = useMutation({
    mutationFn: deleteGarden,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['gardens'] }),
  })

  function openCreate() {
    setEditing(undefined)
    setDialogOpen(true)
  }

  function openEdit(e: React.MouseEvent, garden: Garden) {
    e.stopPropagation()
    setEditing(garden)
    setDialogOpen(true)
  }

  function handleDelete(e: React.MouseEvent, id: number) {
    e.stopPropagation()
    if (confirm('Delete this garden? This cannot be undone.')) {
      deleteMutation.mutate(id)
    }
  }

  if (isLoading) return <p className="text-muted-foreground">Loading…</p>
  if (isError) return <p className="text-destructive">Failed to load gardens.</p>

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">Gardens</h1>
        <Button onClick={openCreate}>New Garden</Button>
      </div>

      {gardens.length === 0 ? (
        <p className="text-muted-foreground">No gardens yet. Create your first one!</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {gardens.map((garden) => (
            <Card
              key={garden.id}
              className="cursor-pointer transition-colors hover:bg-muted/50"
              onClick={() => navigate(`/gardens/${garden.id}`)}
            >
              <CardHeader className="pb-2">
                <CardTitle className="text-base">{garden.name}</CardTitle>
                {garden.location && (
                  <CardDescription>{garden.location}</CardDescription>
                )}
              </CardHeader>
              {garden.notes && (
                <CardContent className="pt-0">
                  <p className="text-sm text-muted-foreground line-clamp-2">{garden.notes}</p>
                </CardContent>
              )}
              <CardContent className="flex justify-end gap-2 pt-0">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={(e) => openEdit(e, garden)}
                >
                  Edit
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  className="text-destructive hover:text-destructive"
                  onClick={(e) => handleDelete(e, garden.id)}
                  disabled={deleteMutation.isPending}
                >
                  Delete
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <GardenFormDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        editing={editing}
      />
    </div>
  )
}
