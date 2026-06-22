import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getGardens, deleteGarden } from '@/api/gardens'
import type { Garden } from '@/types/garden'
import GardenFormDialog from './GardenFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardHeader, CardContent } from '@/components/ui/card'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Trees, Plus, Edit2, Trash2, MapPin } from 'lucide-react'

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

  return (
    <div className="flex flex-col h-full p-6 lg:p-8">
      {/* Header */}
      <div className="flex items-center justify-between pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">Gardens</h1>
          <p className="mt-1 text-muted-foreground">Manage your garden beds and layouts.</p>
        </div>
        <Button onClick={openCreate} className="gap-2">
          <Plus className="w-4 h-4" /> New Garden
        </Button>
      </div>

      <ScrollArea className="flex-1 pr-4 -mr-4">
        {isLoading ? (
          <p className="text-muted-foreground">Loading gardens...</p>
        ) : isError ? (
          <p className="text-destructive">Failed to load gardens.</p>
        ) : gardens.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-center border-2 border-dashed rounded-xl h-64 mt-4">
            <div className="h-12 w-12 rounded-full bg-emerald-500/10 flex items-center justify-center mb-4">
              <Trees className="h-6 w-6 text-emerald-600 dark:text-emerald-400" />
            </div>
            <h3 className="text-lg font-medium">No gardens yet</h3>
            <p className="text-sm text-muted-foreground max-w-sm mt-1 mb-4">
              Create your first garden to start tracking your beds and plantings.
            </p>
            <Button onClick={openCreate}>Create Garden</Button>
          </div>
        ) : (
          <div className="grid gap-6 grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 pb-6">
            {gardens.map((garden) => (
              <Card
                key={garden.id}
                className="overflow-hidden group flex flex-col cursor-pointer transition-all hover:shadow-md hover:border-emerald-500/30"
                onClick={() => navigate(`/gardens/${garden.id}`)}
              >
                {/* Photo Header */}
                <div className="aspect-[21/9] w-full bg-gradient-to-br from-emerald-100 to-teal-100 dark:from-emerald-900/30 dark:to-teal-900/20 flex items-center justify-center relative">
                  <Trees className="w-12 h-12 text-emerald-600/20 dark:text-emerald-400/20" />
                  <div className="absolute top-2 right-2 flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <Button
                      variant="secondary"
                      size="icon"
                      className="h-8 w-8 bg-background/80 backdrop-blur-sm hover:bg-background"
                      onClick={(e) => openEdit(e, garden)}
                    >
                      <Edit2 className="w-4 h-4" />
                    </Button>
                    <Button
                      variant="destructive"
                      size="icon"
                      className="h-8 w-8"
                      onClick={(e) => handleDelete(e, garden.id)}
                      disabled={deleteMutation.isPending}
                    >
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  </div>
                </div>

                <CardHeader className="p-4 pb-2">
                  <h3 className="font-semibold text-lg leading-tight truncate">{garden.name}</h3>
                </CardHeader>
                <CardContent className="p-4 pt-0 flex-1 flex flex-col justify-between">
                  <div>
                    {garden.location && (
                      <div className="flex items-center gap-1.5 text-sm text-muted-foreground mb-3">
                        <MapPin className="w-4 h-4 shrink-0" />
                        <span className="truncate">{garden.location}</span>
                      </div>
                    )}
                    {garden.notes && (
                      <p className="text-sm text-muted-foreground line-clamp-2">
                        {garden.notes}
                      </p>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </ScrollArea>

      <GardenFormDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        editing={editing}
      />
    </div>
  )
}
