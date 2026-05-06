import { useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getGarden, deleteGarden } from '@/api/gardens'
import { getBeds, deleteBed } from '@/api/beds'
import type { Bed } from '@/types/bed'
import GardenFormDialog from './GardenFormDialog'
import BedFormDialog from '@/features/beds/BedFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'

export default function GardenDetailPage() {
  const { gardenId } = useParams<{ gardenId: string }>()
  const id = Number(gardenId)
  const navigate = useNavigate()
  const qc = useQueryClient()

  const [editGardenOpen, setEditGardenOpen] = useState(false)
  const [bedFormOpen, setBedFormOpen] = useState(false)
  const [editingBed, setEditingBed] = useState<Bed | undefined>()

  const { data: garden, isLoading: gardenLoading } = useQuery({
    queryKey: ['gardens', id],
    queryFn: () => getGarden(id),
  })

  const { data: beds = [], isLoading: bedsLoading } = useQuery({
    queryKey: ['beds', id],
    queryFn: () => getBeds(id),
  })

  const deleteGardenMutation = useMutation({
    mutationFn: () => deleteGarden(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['gardens'] })
      navigate('/gardens')
    },
  })

  const deleteBedMutation = useMutation({
    mutationFn: (bedId: number) => deleteBed(id, bedId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['beds', id] }),
  })

  function openEditBed(bed: Bed) {
    setEditingBed(bed)
    setBedFormOpen(true)
  }

  function openAddBed() {
    setEditingBed(undefined)
    setBedFormOpen(true)
  }

  function handleDeleteGarden() {
    if (confirm(`Delete "${garden?.name}"? This cannot be undone.`)) {
      deleteGardenMutation.mutate()
    }
  }

  function handleDeleteBed(bed: Bed) {
    if (confirm(`Delete "${bed.name}"? This cannot be undone.`)) {
      deleteBedMutation.mutate(bed.id)
    }
  }

  if (gardenLoading) return <p className="text-muted-foreground">Loading…</p>
  if (!garden) return <p className="text-destructive">Garden not found.</p>

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <nav className="text-sm text-muted-foreground">
        <Link to="/gardens" className="hover:text-foreground">Gardens</Link>
        <span className="mx-2">/</span>
        <span className="text-foreground">{garden.name}</span>
      </nav>

      {/* Garden header */}
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">{garden.name}</h1>
          {garden.location && (
            <p className="mt-1 text-sm text-muted-foreground">{garden.location}</p>
          )}
          {garden.notes && (
            <p className="mt-2 text-sm">{garden.notes}</p>
          )}
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={() => setEditGardenOpen(true)}>
            Edit
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="text-destructive hover:text-destructive"
            onClick={handleDeleteGarden}
            disabled={deleteGardenMutation.isPending}
          >
            Delete
          </Button>
        </div>
      </div>

      {/* Beds section */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-medium">Beds</h2>
          <Button size="sm" onClick={openAddBed}>Add Bed</Button>
        </div>

        {bedsLoading ? (
          <p className="text-muted-foreground">Loading beds…</p>
        ) : beds.length === 0 ? (
          <p className="text-muted-foreground">No beds yet. Add your first bed!</p>
        ) : (
          <div className="grid gap-3 sm:grid-cols-2">
            {beds.map((bed) => (
              <Card key={bed.id}>
                <CardContent className="pt-4 space-y-3">
                  <div>
                    <p className="font-medium">{bed.name}</p>
                    <p className="text-sm text-muted-foreground">
                      {bed.lengthFt}′ × {bed.widthFt}′ × {bed.depthIn}″
                      {bed.material && ` · ${bed.material}`}
                    </p>
                  </div>

                  <p className="text-xs text-muted-foreground">
                    Installed {bed.installedDate} · {bed.expectedLifespanYears} yr lifespan
                  </p>

                  {bed.notes && <p className="text-sm text-muted-foreground">{bed.notes}</p>}

                  <div className="flex justify-end gap-1">
                    <Button variant="ghost" size="sm" onClick={() => openEditBed(bed)}>Edit</Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-destructive hover:text-destructive"
                      onClick={() => handleDeleteBed(bed)}
                      disabled={deleteBedMutation.isPending}
                    >
                      Delete
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>

      <GardenFormDialog
        open={editGardenOpen}
        onClose={() => setEditGardenOpen(false)}
        editing={garden}
      />
      <BedFormDialog
        open={bedFormOpen}
        onClose={() => setBedFormOpen(false)}
        gardenId={id}
        editing={editingBed}
      />
    </div>
  )
}
