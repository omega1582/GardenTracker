import { useState, useRef, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getPlantTypes, getAllVarieties, importPlantCatalogCsv } from '@/api/plants'
import type { PlantType, PlantVariety, SunPreference } from '@/types/plant'
import PlantTypeFormDialog from './PlantTypeFormDialog'
import PlantVarietyFormDialog from './PlantVarietyFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Badge } from '@/components/ui/badge'
import { Leaf, Plus, Edit2 } from 'lucide-react'
import { useSearchParams } from 'react-router-dom'

const SUN_LABELS: Record<SunPreference, string> = { FullSun: 'Full Sun', PartialSun: 'Partial Sun', Shade: 'Shade' }

export default function PlantsPage() {
  const queryClient = useQueryClient()
  const [typeFormOpen, setTypeFormOpen] = useState(false)
  const [editingType, setEditingType] = useState<PlantType | undefined>()
  const [varietyFormOpen, setVarietyFormOpen] = useState(false)
  const [editingVariety, setEditingVariety] = useState<PlantVariety | undefined>()

  // Navigation State
  // Navigation State from URL
  const [searchParams] = useSearchParams()
  const selectedCategory = searchParams.get('category') || 'All'
  const selectedTypeId = Number(searchParams.get('typeId')) || null

  const [importStatus, setImportStatus] = useState<{ created: number; updated: number; errors: string[] } | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const { data: plantTypes = [] } = useQuery({
    queryKey: ['plant-types'],
    queryFn: getPlantTypes,
  })

  const { data: allVarieties = [], isLoading: isLoadingVarieties } = useQuery({
    queryKey: ['varieties'],
    queryFn: getAllVarieties,
  })

  // Import Mutation
  const importMutation = useMutation({
    mutationFn: (file: File) => importPlantCatalogCsv(file),
    onSuccess: (result) => {
      setImportStatus(result)
      queryClient.invalidateQueries({ queryKey: ['plant-types'] })
      queryClient.invalidateQueries({ queryKey: ['varieties'] })
      if (fileInputRef.current) fileInputRef.current.value = ''
    },
    onError: (err: Error) => {
      setImportStatus({ created: 0, updated: 0, errors: [err.message ?? 'Import failed.'] })
      if (fileInputRef.current) fileInputRef.current.value = ''
    },
  })

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) importMutation.mutate(file)
  }

  // Determine what to show in the main grid
  const selectedTypeObj = selectedTypeId ? plantTypes.find(t => t.id === selectedTypeId) : null

  const displayedVarieties = useMemo(() => {
    if (selectedTypeId) {
      return allVarieties.filter(v => v.plantTypeId === selectedTypeId)
    }
    if (selectedCategory !== 'All') {
      const typeIdsInCategory = plantTypes.filter(t => t.category === selectedCategory).map(t => t.id)
      return allVarieties.filter(v => typeIdsInCategory.includes(v.plantTypeId))
    }
    return allVarieties
  }, [allVarieties, plantTypes, selectedCategory, selectedTypeId])

  const headerTitle = selectedTypeId ? selectedTypeObj?.name : (selectedCategory === 'All' ? 'All Plants' : selectedCategory)

  return (
    <div className="flex flex-col h-full p-6">
      {/* Header */}
      <div className="flex items-center justify-between pb-4 border-b">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Plants Catalog</h1>
          <p className="text-muted-foreground mt-1">Manage your plant types and varieties.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => fileInputRef.current?.click()} disabled={importMutation.isPending}>
            {importMutation.isPending ? 'Importing…' : 'Import CSV'}
          </Button>
          <input ref={fileInputRef} type="file" accept=".csv" className="hidden" onChange={handleFileChange} />
          <Button onClick={() => { setEditingType(undefined); setTypeFormOpen(true) }}>
            <Plus className="w-4 h-4 mr-2" /> Add Plant Type
          </Button>
        </div>
      </div>

      {importStatus && (
        <div className={`mt-4 rounded-lg border px-4 py-3 text-sm ${importStatus.errors.length > 0 ? 'border-destructive bg-destructive/5' : 'border-green-500 bg-green-500/5'}`}>
          <p className="font-medium">
            Import complete: {importStatus.created} created, {importStatus.updated} updated
          </p>
          {importStatus.errors.length > 0 && (
            <ul className="mt-1 text-muted-foreground">
              {importStatus.errors.map((e, i) => <li key={i}>• {e}</li>)}
            </ul>
          )}
        </div>
      )}

      <div className="flex flex-1 overflow-hidden mt-6">

        {/* Main Content Area - Varieties Grid */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Type/Category Detail Header */}
          <div className="flex items-start justify-between mb-6">
            <div>
              <div className="flex items-center gap-3">
                <h2 className="text-2xl font-semibold">{headerTitle}</h2>
                {selectedTypeObj && (
                  <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => { setEditingType(selectedTypeObj); setTypeFormOpen(true) }}>
                    <Edit2 className="w-4 h-4 text-muted-foreground" />
                  </Button>
                )}
              </div>
              <p className="text-muted-foreground mt-1 text-sm">
                {displayedVarieties.length} varieties found
              </p>

              {selectedTypeObj && (
                <div className="flex flex-wrap gap-2 mt-3">
                  {selectedTypeObj.growthHabit && <Badge variant="secondary">{selectedTypeObj.growthHabit}</Badge>}
                  {selectedTypeObj.daysToMaturity && <Badge variant="secondary">{selectedTypeObj.daysToMaturity} days</Badge>}
                  {selectedTypeObj.sunPreference && <Badge variant="outline">{SUN_LABELS[selectedTypeObj.sunPreference]}</Badge>}
                  {selectedTypeObj.isPerennial != null && <Badge variant="outline">{selectedTypeObj.isPerennial ? 'Perennial' : 'Annual'}</Badge>}
                </div>
              )}
            </div>

            <div className="flex items-center gap-2">
              {selectedTypeObj ? (
                <Button onClick={() => { setEditingVariety(undefined); setVarietyFormOpen(true) }}>
                  <Plus className="w-4 h-4 mr-2" /> Add Variety to {selectedTypeObj.name}
                </Button>
              ) : (
                <p className="text-sm text-muted-foreground">Select a specific Plant Type to add varieties.</p>
              )}
            </div>
          </div>

          {/* Varieties Grid */}
          <ScrollArea className="flex-1 pr-4">
            {isLoadingVarieties ? (
              <p className="text-muted-foreground">Loading varieties...</p>
            ) : displayedVarieties.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12 text-center border-2 border-dashed rounded-xl h-64">
                <div className="h-12 w-12 rounded-full bg-muted flex items-center justify-center mb-4">
                  <Leaf className="h-6 w-6 text-muted-foreground" />
                </div>
                <h3 className="text-lg font-medium">No varieties found</h3>
                <p className="text-sm text-muted-foreground max-w-sm mt-1">
                  Try adding some new varieties to this category.
                </p>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 pb-4">
                {displayedVarieties.map(variety => (
                  <Card key={variety.id} className="overflow-hidden group flex flex-col">
                    <div className="aspect-[4/3] w-full bg-gradient-to-br from-green-50 to-emerald-100 dark:from-green-900/20 dark:to-emerald-900/40 flex items-center justify-center relative">
                      <Leaf className="w-12 h-12 text-emerald-600/20 dark:text-emerald-400/20" />
                      <Button
                        variant="secondary"
                        size="icon"
                        className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity h-8 w-8"
                        onClick={() => { setEditingVariety(variety); setVarietyFormOpen(true) }}
                      >
                        <Edit2 className="w-4 h-4" />
                      </Button>
                    </div>
                    <CardHeader className="p-4 pb-2">
                      <div className="text-xs font-medium text-emerald-600 mb-1">{variety.plantTypeName}</div>
                      <h3 className="font-semibold text-lg leading-tight truncate" title={variety.name}>{variety.name}</h3>
                    </CardHeader>
                    <CardContent className="p-4 pt-0 flex-1 flex flex-col justify-between">
                      <div>
                        <div className="flex flex-wrap gap-1.5 mb-3">
                          {variety.growthHabit && <Badge variant="secondary" className="text-[10px] px-1.5 py-0">{variety.growthHabit}</Badge>}
                          {variety.daysToMaturity && <Badge variant="outline" className="text-[10px] px-1.5 py-0">{variety.daysToMaturity}d</Badge>}
                          {variety.sunPreference && <Badge variant="outline" className="text-[10px] px-1.5 py-0">{SUN_LABELS[variety.sunPreference]}</Badge>}
                        </div>
                        {variety.notes && (
                          <p className="text-xs text-muted-foreground line-clamp-2" title={variety.notes}>
                            {variety.notes}
                          </p>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            )}
          </ScrollArea>
        </div>
      </div>

      <PlantTypeFormDialog
        open={typeFormOpen}
        onClose={() => setTypeFormOpen(false)}
        editing={editingType}
      />
      <PlantVarietyFormDialog
        open={varietyFormOpen}
        onClose={() => setVarietyFormOpen(false)}
        plantTypeId={editingVariety?.plantTypeId ?? selectedTypeObj?.id ?? 0}
        plantTypeName={editingVariety?.plantTypeName ?? selectedTypeObj?.name ?? ''}
        editing={editingVariety}
      />
    </div>
  )
}
