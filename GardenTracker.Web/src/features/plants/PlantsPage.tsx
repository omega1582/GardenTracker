import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getPlantTypes, getVarieties } from '@/api/plants'
import type { PlantType, PlantVariety, SunPreference } from '@/types/plant'
import PlantTypeFormDialog from './PlantTypeFormDialog'
import PlantVarietyFormDialog from './PlantVarietyFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'

const SUN_LABELS: Record<SunPreference, string> = { FullSun: 'Full Sun', PartialSun: 'Partial Sun', Shade: 'Shade' }

function formatAttributes(item: {
  growthHabit?: string | null
  daysToMaturity?: number | null
  spacingInches?: number | null
  sunPreference?: SunPreference | null
  isPerennial?: boolean | null
}): string {
  const parts: string[] = []
  if (item.growthHabit) parts.push(item.growthHabit)
  if (item.daysToMaturity) parts.push(`${item.daysToMaturity}d`)
  if (item.spacingInches) parts.push(`${item.spacingInches}" spacing`)
  if (item.sunPreference) parts.push(SUN_LABELS[item.sunPreference])
  if (item.isPerennial != null) parts.push(item.isPerennial ? 'Perennial' : 'Annual')
  return parts.join(' · ')
}

export default function PlantsPage() {
  const [typeFormOpen, setTypeFormOpen] = useState(false)
  const [editingType, setEditingType] = useState<PlantType | undefined>()

  const { data: plantTypes = [], isLoading } = useQuery({
    queryKey: ['plant-types'],
    queryFn: getPlantTypes,
  })

  function openAddType() {
    setEditingType(undefined)
    setTypeFormOpen(true)
  }

  function openEditType(type: PlantType) {
    setEditingType(type)
    setTypeFormOpen(true)
  }

  if (isLoading) return <p className="text-muted-foreground">Loading…</p>

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">Plants</h1>
        <Button size="sm" onClick={openAddType}>Add Plant Type</Button>
      </div>

      {plantTypes.length === 0 ? (
        <p className="text-muted-foreground">No plant types yet. Add your first one!</p>
      ) : (
        <div className="space-y-4">
          {plantTypes.map((type) => (
            <PlantTypeSection
              key={type.id}
              type={type}
              onEditType={openEditType}
            />
          ))}
        </div>
      )}

      <PlantTypeFormDialog
        open={typeFormOpen}
        onClose={() => setTypeFormOpen(false)}
        editing={editingType}
      />
    </div>
  )
}

function PlantTypeSection({
  type,
  onEditType,
}: {
  type: PlantType
  onEditType: (type: PlantType) => void
}) {
  const [varietyFormOpen, setVarietyFormOpen] = useState(false)
  const [editingVariety, setEditingVariety] = useState<PlantVariety | undefined>()

  const { data: varieties = [], isLoading } = useQuery({
    queryKey: ['varieties', type.id],
    queryFn: () => getVarieties(type.id),
  })

  function openAddVariety() {
    setEditingVariety(undefined)
    setVarietyFormOpen(true)
  }

  function openEditVariety(variety: PlantVariety) {
    setEditingVariety(variety)
    setVarietyFormOpen(true)
  }

  const typeAttrs = formatAttributes(type)

  return (
    <Card>
      <CardContent className="pt-4 space-y-3">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="font-semibold">{type.name}</h2>
            {typeAttrs && <p className="text-xs text-muted-foreground mt-0.5">{typeAttrs}</p>}
          </div>
          <div className="flex gap-1">
            <Button variant="ghost" size="sm" onClick={() => onEditType(type)}>Edit</Button>
            <Button variant="ghost" size="sm" onClick={openAddVariety}>Add Variety</Button>
          </div>
        </div>

        {isLoading ? (
          <p className="text-sm text-muted-foreground">Loading varieties…</p>
        ) : varieties.length === 0 ? (
          <p className="text-sm text-muted-foreground">No varieties yet.</p>
        ) : (
          <div className="divide-y divide-border">
            {varieties.map((variety) => {
              const attrs = formatAttributes(variety)
              return (
                <div key={variety.id} className="flex items-start justify-between py-2">
                  <div>
                    <p className="text-sm font-medium">{variety.name}</p>
                    {attrs && <p className="text-xs text-muted-foreground mt-0.5">{attrs}</p>}
                    {variety.notes && (
                      <p className="text-xs text-muted-foreground mt-0.5 italic">{variety.notes}</p>
                    )}
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="shrink-0"
                    onClick={() => openEditVariety(variety)}
                  >
                    Edit
                  </Button>
                </div>
              )
            })}
          </div>
        )}
      </CardContent>

      <PlantVarietyFormDialog
        open={varietyFormOpen}
        onClose={() => setVarietyFormOpen(false)}
        plantTypeId={type.id}
        plantTypeName={type.name}
        editing={editingVariety}
      />
    </Card>
  )
}
