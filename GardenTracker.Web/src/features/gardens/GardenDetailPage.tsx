import { useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { ChevronRight, MapPin } from 'lucide-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getGarden, deleteGarden } from '@/api/gardens'
import { getBeds, deleteBed } from '@/api/beds'
import { getSeasons, createSeason } from '@/api/seasons'
import { getPlantings, deletePlanting } from '@/api/plantings'
import { getExpenses, deleteExpense } from '@/api/expenses'
import { getHarvests, deleteHarvest } from '@/api/harvests'
import { getMarketPrices, deleteMarketPrice } from '@/api/marketPrices'
import type { Bed } from '@/types/bed'
import type { Planting } from '@/types/planting'
import type { Expense } from '@/types/expense'
import type { Harvest } from '@/types/harvest'
import type { MarketPrice } from '@/types/marketPrice'
import GardenFormDialog from './GardenFormDialog'
import GardenLayoutView from './GardenLayoutView'
import BedFormDialog from '@/features/beds/BedFormDialog'
import PlantingFormDialog from '@/features/plantings/PlantingFormDialog'
import ExpenseFormDialog from '@/features/expenses/ExpenseFormDialog'
import HarvestFormDialog from '@/features/harvests/HarvestFormDialog'
import MarketPriceFormDialog from '@/features/marketPrices/MarketPriceFormDialog'
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
  const [expenseFormOpen, setExpenseFormOpen] = useState(false)
  const [editingExpense, setEditingExpense] = useState<Expense | undefined>()
  const [harvestFormOpen, setHarvestFormOpen] = useState(false)
  const [editingHarvest, setEditingHarvest] = useState<Harvest | undefined>()
  const [marketPriceFormOpen, setMarketPriceFormOpen] = useState(false)
  const [editingMarketPrice, setEditingMarketPrice] = useState<MarketPrice | undefined>()
  const [selectedYear, setSelectedYear] = useState(CURRENT_YEAR)
  const [activeTab, setActiveTab] = useState<'detail' | 'layout'>('detail')

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

  const { data: expenses = [], isLoading: expensesLoading } = useQuery({
    queryKey: ['expenses', id, selectedYear],
    queryFn: () => getExpenses(id, selectedYear),
    enabled: !!selectedSeason,
  })

  const { data: marketPrices = [] } = useQuery({
    queryKey: ['market-prices', id, selectedYear],
    queryFn: () => getMarketPrices(id, selectedYear),
    enabled: !!selectedSeason,
  })

  const { data: harvests = [], isLoading: harvestsLoading } = useQuery({
    queryKey: ['harvests', id, selectedYear],
    queryFn: () => getHarvests(id, selectedYear),
    enabled: !!selectedSeason,
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

  const deleteExpenseMutation = useMutation({
    mutationFn: (expenseId: number) => deleteExpense(id, selectedYear, expenseId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['expenses', id, selectedYear] }),
  })

  const deleteHarvestMutation = useMutation({
    mutationFn: (harvestId: number) => deleteHarvest(id, selectedYear, harvestId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['harvests', id, selectedYear] }),
  })

  const deleteMarketPriceMutation = useMutation({
    mutationFn: (priceId: number) => deleteMarketPrice(id, selectedYear, priceId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['market-prices', id, selectedYear] }),
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

  function openAddExpense() {
    setEditingExpense(undefined)
    setExpenseFormOpen(true)
  }

  function openEditExpense(expense: Expense) {
    setEditingExpense(expense)
    setExpenseFormOpen(true)
  }

  function handleDeleteExpense(expense: Expense) {
    if (confirm(`Delete expense "${expense.description}"? This cannot be undone.`)) {
      deleteExpenseMutation.mutate(expense.id)
    }
  }

  function openAddHarvest() {
    setEditingHarvest(undefined)
    setHarvestFormOpen(true)
  }

  function openEditHarvest(harvest: Harvest) {
    setEditingHarvest(harvest)
    setHarvestFormOpen(true)
  }

  function handleDeleteHarvest(harvest: Harvest) {
    if (confirm(`Delete harvest of ${harvest.quantity} ${harvest.unit} of ${harvest.plantVarietyName}?`)) {
      deleteHarvestMutation.mutate(harvest.id)
    }
  }

  if (gardenLoading) return <p className="text-muted-foreground">Loading…</p>
  if (!garden) return <p className="text-destructive">Garden not found.</p>

  // Build year options: existing season years + a rolling window of 5 past and 2 future years
  const yearOptions = Array.from(
    new Set([
      ...seasons.map(s => s.year),
      ...Array.from({ length: 8 }, (_, i) => CURRENT_YEAR - 5 + i),
    ])
  ).sort((a, b) => b - a)

  return (
    <div className="flex flex-col h-full overflow-y-auto p-6 lg:p-8 space-y-8">
      {/* Breadcrumb */}
      <nav className="flex items-center text-sm text-muted-foreground gap-1.5">
        <Link to="/gardens" className="hover:text-foreground transition-colors">Gardens</Link>
        <ChevronRight className="w-4 h-4" />
        <span className="text-foreground font-medium">{garden.name}</span>
      </nav>

      {/* Garden header */}
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{garden.name}</h1>
          {garden.location && (
            <div className="flex items-center gap-1.5 mt-2 text-muted-foreground">
              <MapPin className="w-4 h-4" />
              <span>{garden.location}</span>
            </div>
          )}
          {garden.notes && (
            <p className="mt-3 text-sm text-muted-foreground max-w-2xl">{garden.notes}</p>
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

      {/* Tab bar */}
      <div className="flex gap-1 border-b border-border">
        {(['detail', 'layout'] as const).map(tab => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 text-sm font-medium capitalize border-b-2 -mb-px transition-colors ${
              activeTab === tab
                ? 'border-primary text-foreground'
                : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}
          >
            {tab === 'layout' ? 'Garden Layout' : 'Details'}
          </button>
        ))}
      </div>

      {/* Layout tab */}
      {activeTab === 'layout' && (
        <GardenLayoutView gardenId={id} beds={beds} />
      )}

      {/* Details tab content */}
      {activeTab === 'detail' && <>

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

      {/* Expenses */}
      {selectedSeason && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h2 className="text-lg font-medium">Expenses</h2>
              {expenses.length > 0 && (
                <span className="text-sm text-muted-foreground">
                  ${expenses.reduce((sum, e) => sum + e.amount, 0).toFixed(2)} total
                </span>
              )}
            </div>
            <Button size="sm" onClick={openAddExpense}>Add Expense</Button>
          </div>

          {expensesLoading ? (
            <p className="text-muted-foreground text-sm">Loading expenses…</p>
          ) : expenses.length === 0 ? (
            <p className="text-muted-foreground text-sm">No expenses yet.</p>
          ) : (
            <div className="space-y-2">
              {expenses.map((expense) => (
                <ExpenseRow
                  key={expense.id}
                  expense={expense}
                  onEdit={openEditExpense}
                  onDelete={handleDeleteExpense}
                  isDeleting={deleteExpenseMutation.isPending}
                />
              ))}
            </div>
          )}
        </div>
      )}

      {/* Harvests */}
      {selectedSeason && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h2 className="text-lg font-medium">Harvests</h2>
              {harvests.length > 0 && (
                <span className="text-sm text-muted-foreground">{harvests.length} logged</span>
              )}
            </div>
            <Button size="sm" onClick={openAddHarvest}>Log Harvest</Button>
          </div>

          {harvestsLoading ? (
            <p className="text-muted-foreground text-sm">Loading harvests…</p>
          ) : harvests.length === 0 ? (
            <p className="text-muted-foreground text-sm">No harvests logged yet.</p>
          ) : (
            <div className="space-y-2">
              {harvests.map((harvest) => (
                <HarvestRow
                  key={harvest.id}
                  harvest={harvest}
                  onEdit={openEditHarvest}
                  onDelete={handleDeleteHarvest}
                  isDeleting={deleteHarvestMutation.isPending}
                />
              ))}
            </div>
          )}
        </div>
      )}

      {/* Market Prices */}
      {selectedSeason && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h2 className="text-lg font-medium">Market Prices</h2>
              <span className="text-xs text-muted-foreground">Used to calculate harvest value in reports</span>
            </div>
            <Button size="sm" variant="outline" onClick={() => { setEditingMarketPrice(undefined); setMarketPriceFormOpen(true) }}>
              Add Price
            </Button>
          </div>

          {marketPrices.length === 0 ? (
            <p className="text-muted-foreground text-sm">
              No market prices set — harvest value in reports will show $0.
            </p>
          ) : (
            <div className="divide-y divide-border border border-border rounded-sm">
              {marketPrices.map((mp) => (
                <div key={mp.id} className="flex items-center justify-between px-4 py-2.5 text-sm">
                  <span className="text-muted-foreground">
                    {mp.plantTypeName}{mp.plantVarietyName ? ` — ${mp.plantVarietyName}` : ' (all varieties)'}
                  </span>
                  <div className="flex items-center gap-4">
                    <span className="font-medium tabular-nums">
                      ${mp.pricePerUnit.toFixed(2)} / {mp.unit.toLowerCase()}
                    </span>
                    <div className="flex gap-1">
                      <Button variant="ghost" size="sm" onClick={() => { setEditingMarketPrice(mp); setMarketPriceFormOpen(true) }}>Edit</Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive"
                        onClick={() => {
                          if (confirm(`Delete price for ${mp.plantTypeName}?`)) deleteMarketPriceMutation.mutate(mp.id)
                        }}
                      >
                        Delete
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

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

      </>}

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
      <ExpenseFormDialog
        open={expenseFormOpen}
        onClose={() => setExpenseFormOpen(false)}
        gardenId={id}
        year={selectedYear}
        beds={beds}
        editing={editingExpense}
      />
      <HarvestFormDialog
        open={harvestFormOpen}
        onClose={() => setHarvestFormOpen(false)}
        gardenId={id}
        year={selectedYear}
        beds={beds}
        editing={editingHarvest}
      />
      <MarketPriceFormDialog
        open={marketPriceFormOpen}
        onClose={() => setMarketPriceFormOpen(false)}
        gardenId={id}
        year={selectedYear}
        editing={editingMarketPrice}
      />
    </div>
  )
}

const UNIT_LABELS: Record<string, string> = {
  Pounds: 'lbs',
  Ounces: 'oz',
  Count: 'each',
  Bunch: 'bunch',
}

function HarvestRow({
  harvest,
  onEdit,
  onDelete,
  isDeleting,
}: {
  harvest: Harvest
  onEdit: (h: Harvest) => void
  onDelete: (h: Harvest) => void
  isDeleting: boolean
}) {
  return (
    <Card>
      <CardContent className="pt-3 pb-3">
        <div className="flex items-center justify-between gap-3">
          <div className="space-y-0.5 flex-1">
            <div className="flex items-center gap-2 flex-wrap">
              <span className="text-sm font-medium">
                {harvest.plantTypeName} — {harvest.plantVarietyName}
              </span>
              <Badge variant="secondary" className="text-xs">{harvest.bedName}</Badge>
            </div>
            <div className="flex items-center gap-3 text-xs text-muted-foreground">
              <span className="font-medium text-foreground">
                {harvest.quantity} {UNIT_LABELS[harvest.unit] ?? harvest.unit}
              </span>
              <span>{harvest.harvestDate}</span>
            </div>
            {harvest.notes && (
              <p className="text-xs text-muted-foreground">{harvest.notes}</p>
            )}
          </div>
          <div className="flex gap-1 shrink-0">
            <Button variant="ghost" size="sm" onClick={() => onEdit(harvest)}>Edit</Button>
            <Button
              variant="ghost"
              size="sm"
              className="text-destructive hover:text-destructive"
              onClick={() => onDelete(harvest)}
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

const CATEGORY_LABELS: Record<string, string> = {
  Seeds: 'Seeds',
  Transplants: 'Transplants',
  Soil: 'Soil',
  Fertilizer: 'Fertilizer',
  PestControl: 'Pest Control',
  BedMaterials: 'Bed Materials',
  Tools: 'Tools',
  Maintenance: 'Maintenance',
  Other: 'Other',
}

function ExpenseRow({
  expense,
  onEdit,
  onDelete,
  isDeleting,
}: {
  expense: Expense
  onEdit: (e: Expense) => void
  onDelete: (e: Expense) => void
  isDeleting: boolean
}) {
  return (
    <Card>
      <CardContent className="pt-3 pb-3">
        <div className="flex items-center justify-between gap-3">
          <div className="space-y-0.5 flex-1">
            <div className="flex items-center gap-2 flex-wrap">
              <span className="text-sm font-medium">{expense.description}</span>
              <Badge variant="secondary" className="text-xs">
                {CATEGORY_LABELS[expense.category] ?? expense.category}
              </Badge>
              {expense.bedName && (
                <Badge variant="outline" className="text-xs">{expense.bedName}</Badge>
              )}
            </div>
            <div className="flex items-center gap-3 text-xs text-muted-foreground">
              <span>${expense.amount.toFixed(2)}</span>
              <span>{expense.expenseDate}</span>
              {expense.supplierName && <span>{expense.supplierName}</span>}
            </div>
          </div>
          <div className="flex gap-1 shrink-0">
            <Button variant="ghost" size="sm" onClick={() => onEdit(expense)}>Edit</Button>
            <Button
              variant="ghost"
              size="sm"
              className="text-destructive hover:text-destructive"
              onClick={() => onDelete(expense)}
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
