import { useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getGarden, deleteGarden } from '@/api/gardens'
import { getBeds, deleteBed } from '@/api/beds'
import { getSeasons, createSeason } from '@/api/seasons'
import { getPlantings, deletePlanting } from '@/api/plantings'
import type { Bed } from '@/types/bed'
import type { Planting } from '@/types/planting'
import GardenFormDialog from './GardenFormDialog'
import BedFormDialog from '@/features/beds/BedFormDialog'
import PlantingFormDialog from '@/features/plantings/PlantingFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'

const CURRENT_YEAR = new Date().getFullYear()

const START_METHOD_LABELS: Record<string, string> = {
  Seed: 'Seed',
  Transplant: 'Transplant',
  SeedSaved: 'Seed (saved)',
  Cutting: 'Cutting',
}

export default function GardenDetailPage() {
  const { gardenId } = useParams<{ gardenId: string }>()
  const id = Number(gardenId)
  const navigate = useNavigate()
  const qc = useQueryClient()

  const [editGardenOpen, setEditGardenOpen] = useState(false)
  const [bedFormOpen, setBedFormOpen] = useState(false)
  const [editingBed, setEditingBed] = useState<Bed | undefined>()
  const [plantingFormOpen, setPlantingFormOpen] = useState(false)
  const [editingPlanting, setEditingPlanting] = useState<Planting | undefined>()
  const [selectedYear, setSelectedYear] = useState(CURRENT_YEAR)

  const { data: garden, isLoading: gardenLoading } = useQuery({
    queryKey: ['gardens', id],
    queryFn: () => getGarden(id),
  })

  const { data: beds = [], isLoading: bedsLoading } = useQuery({
    queryKey: ['beds', id],
    queryFn: () => getBeds(id),
  })

  const { data: seasons = [] } = useQuery({
    queryKey: ['seasons', id],
    queryFn: () => getSeasons(id),
  })

  const { data: plantings = [], isLoading: plantingsLoading } = useQuery({
    queryKey: ['plantings', id, selectedYear],
    queryFn: () => getPlantings(id, selectedYear),
  })

  const selectedSeason = seasons.find(s => s.year === selectedYear)

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

  const createSeasonMutation = useMutation({
    mutationFn: () => createSeason(id, selectedYear),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['seasons', id] }),
  })

  const deletePlantingMutation = useMutation({
    mutationFn: ({ year, plantingId }: { year: number; plantingId: number }) =>
      deletePlanting(id, year, plantingId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['plantings', id, selectedYear] })
      qc.invalidateQueries({ queryKey: ['inventory'] })
    },
  })

  function openEditBed(bed: Bed) {
    setEditingBed(bed)
    setBedFormOpen(true)
  }

  function openAddBed() {
    setEditingBed(undefined)
    setBedFormOpen(true)
  }

  function openAddPlanting() {
    setEditingPlanting(undefined)
    setPlantingFormOpen(true)
  }

  function openEditPlanting(planting: Planting) {
    setEditingPlanting(planting)
    setPlantingFormOpen(true)
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

  function handleDeletePlanting(planting: Planting) {
    if (confirm(`Remove ${planting.plantTypeName} — ${planting.plantVarietyName} from ${planting.bedName}?`)) {
      deletePlantingMutation.mutate({ year: selectedYear, plantingId: planting.id })
    }
  }

  if (gardenLoading) return <p className="text-muted-foreground">Loading…</p>
  if (!garden) return <p className="text-destructive">Garden not found.</p>

  // Build year options: all existing season years + current year
  const yearOptions = Array.from(
    new Set([...seasons.map(s => s.year), CURRENT_YEAR])
  ).sort((a, b) => b - a)

  return (
    <div className="space-y-8">
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
          <Button variant="outline" size="sm" onClick={() => setEditGardenOpen(true)}>Edit</Button>
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

      {/* Season + Plantings */}
      <div className="space-y-4">
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <h2 className="text-lg font-medium">Plantings</h2>
            <select
              className="rounded-md border border-input bg-background px-3 py-1.5 text-sm"
              value={selectedYear}
              onChange={(e) => setSelectedYear(Number(e.target.value))}
            >
              {yearOptions.map(y => (
                <option key={y} value={y}>{y}</option>
              ))}
            </select>
          </div>
          {selectedSeason ? (
            <Button size="sm" onClick={openAddPlanting}>Add Planting</Button>
          ) : (
            <Button
              size="sm"
              variant="outline"
              onClick={() => createSeasonMutation.mutate()}
              disabled={createSeasonMutation.isPending}
            >
              Start {selectedYear} Season
            </Button>
          )}
        </div>

        {!selectedSeason ? (
          <p className="text-muted-foreground text-sm">No season for {selectedYear} yet.</p>
        ) : plantingsLoading ? (
          <p className="text-muted-foreground text-sm">Loading plantings…</p>
        ) : plantings.length === 0 ? (
          <p className="text-muted-foreground text-sm">No plantings yet. Add your first!</p>
        ) : (
          <div className="space-y-2">
            {plantings.map((planting) => (
              <PlantingRow
                key={planting.id}
                planting={planting}
                onEdit={openEditPlanting}
                onDelete={handleDeletePlanting}
                isDeleting={deletePlantingMutation.isPending}
              />
            ))}
          </div>
        )}
      </div>

      {/* Beds */}
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
      <PlantingFormDialog
        open={plantingFormOpen}
        onClose={() => setPlantingFormOpen(false)}
        gardenId={id}
        year={selectedYear}
        beds={beds}
        editing={editingPlanting}
      />
    </div>
  )
}

function PlantingRow({
  planting,
  onEdit,
  onDelete,
  isDeleting,
}: {
  planting: Planting
  onEdit: (p: Planting) => void
  onDelete: (p: Planting) => void
  isDeleting: boolean
}) {
  return (
    <Card>
      <CardContent className="pt-3 pb-3">
        <div className="flex items-start justify-between gap-3">
          <div className="space-y-1 flex-1">
            <div className="flex items-center gap-2 flex-wrap">
              <span className="text-sm font-medium">
                {planting.plantTypeName} — {planting.plantVarietyName}
              </span>
              <Badge variant="secondary" className="text-xs">{planting.bedName}</Badge>
              <Badge variant="outline" className="text-xs">
                {START_METHOD_LABELS[planting.startMethod] ?? planting.startMethod}
              </Badge>
            </div>
            <div className="flex items-center gap-3 text-xs text-muted-foreground">
              <span>{planting.quantity} plants</span>
              {planting.totalCost > 0 && <span>${planting.totalCost.toFixed(2)}</span>}
              {planting.inventoryItemId && planting.quantityUsedFromInventory && (
                <span>{planting.quantityUsedFromInventory} from inventory</span>
              )}
              {planting.supplierName && <span>{planting.supplierName}</span>}
            </div>
            {planting.notes && (
              <p className="text-xs text-muted-foreground">{planting.notes}</p>
            )}
          </div>
          <div className="flex gap-1 shrink-0">
            <Button variant="ghost" size="sm" onClick={() => onEdit(planting)}>Edit</Button>
            <Button
              variant="ghost"
              size="sm"
              className="text-destructive hover:text-destructive"
              onClick={() => onDelete(planting)}
              disabled={isDeleting}
            >
              Delete
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
