import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useSearchParams } from 'react-router-dom'
import { getGardens } from '@/api/gardens'
import { getHarvests, deleteHarvest } from '@/api/harvests'
import { getMarketPrices } from '@/api/marketPrices'
import { getAllVarieties } from '@/api/plants'
import type { Harvest, HarvestUnit } from '@/types/harvest'
import HarvestFormDialog from './HarvestFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Scale, Plus, Edit2, Trash2, Search, ArrowUpRight, ArrowDownRight, Equal } from 'lucide-react'

const CURRENT_YEAR = new Date().getFullYear()

interface EnrichedHarvest extends Harvest {
  gardenId: number
  gardenName: string
  pricePerUnit: number | null
  value: number | null
}

export default function HarvestsPage() {
  const qc = useQueryClient()
  const [searchParams] = useSearchParams()
  const selectedYear = Number(searchParams.get('year')) || CURRENT_YEAR

  const [activeTab, setActiveTab] = useState<'list' | 'compare'>('list')
  const [compareYear, setCompareYear] = useState<number>(selectedYear - 1)
  const [searchQuery, setSearchQuery] = useState('')
  const [formOpen, setFormOpen] = useState(false)
  const [editingHarvest, setEditingHarvest] = useState<Harvest | undefined>()

  // Fetch foundational data
  const { data: gardens = [] } = useQuery({ queryKey: ['gardens'], queryFn: getGardens })
  const { data: allVarieties = [] } = useQuery({ queryKey: ['varieties'], queryFn: getAllVarieties })

  // Query to fetch harvests for the selected year across all gardens
  const { data: harvestsData = [], isLoading: harvestsLoading } = useQuery({
    queryKey: ['harvests-all-gardens', selectedYear, gardens.map(g => g.id)],
    queryFn: async () => {
      if (gardens.length === 0) return []
      const res = await Promise.all(gardens.map(async (g) => {
        try {
          const list = await getHarvests(g.id, selectedYear)
          return list.map(h => ({ ...h, gardenId: g.id, gardenName: g.name }))
        } catch {
          return []
        }
      }))
      return res.flat()
    },
    enabled: gardens.length > 0,
  })

  // Query to fetch harvests for the comparison year across all gardens
  const { data: compareHarvestsData = [], isLoading: compareHarvestsLoading } = useQuery({
    queryKey: ['harvests-all-gardens', compareYear, gardens.map(g => g.id)],
    queryFn: async () => {
      if (gardens.length === 0) return []
      const res = await Promise.all(gardens.map(async (g) => {
        try {
          const list = await getHarvests(g.id, compareYear)
          return list.map(h => ({ ...h, gardenId: g.id, gardenName: g.name }))
        } catch {
          return []
        }
      }))
      return res.flat()
    },
    enabled: gardens.length > 0 && activeTab === 'compare',
  })

  // Query to fetch market prices for the selected year across all gardens
  const { data: marketPricesData = {} } = useQuery({
    queryKey: ['market-prices-all-gardens', selectedYear, gardens.map(g => g.id)],
    queryFn: async () => {
      if (gardens.length === 0) return {}
      const res = await Promise.all(gardens.map(async (g) => {
        try {
          const list = await getMarketPrices(g.id, selectedYear)
          return { gardenId: g.id, list }
        } catch {
          return { gardenId: g.id, list: [] }
        }
      }))
      return res.reduce((acc, item) => {
        acc[item.gardenId] = item.list
        return acc
      }, {} as Record<number, any[]>)
    },
    enabled: gardens.length > 0,
  })

  const deleteMutation = useMutation({
    mutationFn: ({ gardenId, year, id }: { gardenId: number, year: number, id: number }) => deleteHarvest(gardenId, year, id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['harvests'] })
      qc.invalidateQueries({ queryKey: ['reports'] })
    },
  })

  function handleDelete(harvest: EnrichedHarvest) {
    if (confirm(`Delete harvest of ${harvest.quantity} ${harvest.unit} ${harvest.plantVarietyName}?`)) {
      const yearOfHarvest = Number(harvest.harvestDate.slice(0, 4))
      deleteMutation.mutate({ gardenId: harvest.gardenId, year: yearOfHarvest, id: harvest.id })
    }
  }

  // Enrich harvest items with calculated estimated market values
  function enrichHarvests(rawList: any[], prices: Record<number, any[]>): EnrichedHarvest[] {
    return rawList.map(h => {
      const variety = allVarieties.find(v => v.id === h.plantVarietyId)
      const gardenPrices = prices[h.gardenId] || []

      // Matching algorithm
      let match = gardenPrices.find(mp => mp.plantVarietyId === h.plantVarietyId && mp.unit === h.unit)
      if (!match && variety) {
        match = gardenPrices.find(mp => mp.plantTypeId === variety.plantTypeId && !mp.plantVarietyId && mp.unit === h.unit)
      }

      const pricePerUnit = match ? match.pricePerUnit : null
      const value = pricePerUnit != null ? h.quantity * pricePerUnit : null

      return {
        ...h,
        pricePerUnit,
        value,
      }
    })
  }

  const enrichedHarvests = enrichHarvests(harvestsData, marketPricesData)
    .sort((a, b) => new Date(b.harvestDate).getTime() - new Date(a.harvestDate).getTime())

  // Filter lists by search query
  const filteredHarvests = enrichedHarvests.filter(h =>
    h.plantVarietyName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    h.plantTypeName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    h.bedName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    h.gardenName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    (h.notes && h.notes.toLowerCase().includes(searchQuery.toLowerCase()))
  )

  // Calculations for summary stats
  const totalValue = enrichedHarvests.reduce((sum, h) => sum + (h.value ?? 0), 0)
  const totalCount = enrichedHarvests.length

  const weightSummary = enrichedHarvests.reduce((acc, h) => {
    acc[h.unit] = (acc[h.unit] || 0) + h.quantity
    return acc
  }, {} as Record<HarvestUnit, number>)

  const primaryWeightStr = Object.entries(weightSummary)
    .map(([unit, qty]) => `${qty.toFixed(1)} ${unit === 'Pounds' ? 'lbs' : unit === 'Ounces' ? 'oz' : unit.toLowerCase()}`)
    .join(', ') || '0.0 lbs'

  // Generate Year-Over-Year yield comparison lists
  interface YieldComparisonRow {
    key: string
    plantTypeName: string
    plantVarietyName: string
    unit: HarvestUnit
    selectedQty: number
    compareQty: number
  }

  const yieldComparisonRows: YieldComparisonRow[] = []
  if (activeTab === 'compare') {
    const selectedGroup: Record<string, { pType: string, pVar: string, unit: HarvestUnit, qty: number }> = {}
    const compareGroup: Record<string, { pType: string, pVar: string, unit: HarvestUnit, qty: number }> = {}

    harvestsData.forEach(h => {
      const key = `${h.plantTypeName}-${h.plantVarietyName}-${h.unit}`
      if (!selectedGroup[key]) {
        selectedGroup[key] = { pType: h.plantTypeName, pVar: h.plantVarietyName, unit: h.unit, qty: 0 }
      }
      selectedGroup[key].qty += h.quantity
    })

    compareHarvestsData.forEach(h => {
      const key = `${h.plantTypeName}-${h.plantVarietyName}-${h.unit}`
      if (!compareGroup[key]) {
        compareGroup[key] = { pType: h.plantTypeName, pVar: h.plantVarietyName, unit: h.unit, qty: 0 }
      }
      compareGroup[key].qty += h.quantity
    })

    const allKeys = Array.from(new Set([...Object.keys(selectedGroup), ...Object.keys(compareGroup)]))
    allKeys.forEach(key => {
      const sel = selectedGroup[key]
      const cmp = compareGroup[key]
      yieldComparisonRows.push({
        key,
        plantTypeName: sel?.pType ?? cmp.pType,
        plantVarietyName: sel?.pVar ?? cmp.pVar,
        unit: sel?.unit ?? cmp.unit,
        selectedQty: sel?.qty ?? 0,
        compareQty: cmp?.qty ?? 0,
      })
    })

    yieldComparisonRows.sort((a, b) => b.selectedQty - a.selectedQty)
  }

  return (
    <div className="flex flex-col h-full overflow-y-auto p-6 lg:p-8 space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground flex items-center gap-3">
            <Scale className="w-8 h-8 text-emerald-500" />
            Harvests — {selectedYear}
          </h1>
          <p className="mt-1.5 text-muted-foreground">
            Track and compare produce harvested across your gardens.
          </p>
        </div>
        <Button size="sm" className="gap-2" onClick={() => { setEditingHarvest(undefined); setFormOpen(true) }}>
          <Plus className="w-4 h-4" />
          Log Harvest
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="border-border shadow-sm">
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Total Weight / Yields
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold truncate" title={primaryWeightStr}>
              {primaryWeightStr}
            </div>
            <p className="text-xs text-muted-foreground mt-1">Across all logged items</p>
          </CardContent>
        </Card>

        <Card className="border-border shadow-sm">
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Estimated Market Value
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">
              ${totalValue.toFixed(2)}
            </div>
            <p className="text-xs text-muted-foreground mt-1">Based on recorded market prices</p>
          </CardContent>
        </Card>

        <Card className="border-border shadow-sm">
          <CardHeader className="pb-2">
            <CardTitle className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
              Total Entries
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {totalCount}
            </div>
            <p className="text-xs text-muted-foreground mt-1">Harvest receipts logged</p>
          </CardContent>
        </Card>
      </div>

      {/* Tabs Menu */}
      <div className="border-b border-border flex items-center justify-between gap-4">
        <div className="flex gap-4">
          <button
            onClick={() => setActiveTab('list')}
            className={`pb-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'list'
                ? 'border-emerald-500 text-foreground'
                : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}
          >
            All Harvests
          </button>
          <button
            onClick={() => setActiveTab('compare')}
            className={`pb-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'compare'
                ? 'border-emerald-500 text-foreground'
                : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}
          >
            Yearly Yield Comparison
          </button>
        </div>

        {activeTab === 'compare' && (
          <div className="flex items-center gap-2 mb-2">
            <Label htmlFor="compare-year-select" className="text-xs text-muted-foreground">Compare to:</Label>
            <select
              id="compare-year-select"
              className="rounded-md border border-input bg-background px-2 py-1 text-xs"
              value={compareYear}
              onChange={(e) => setCompareYear(Number(e.target.value))}
            >
              {[1, 2, 3, 4, 5].map(offset => (
                <option key={offset} value={selectedYear - offset}>{selectedYear - offset}</option>
              ))}
            </select>
          </div>
        )}
      </div>

      {/* Tab Contents */}
      {activeTab === 'list' ? (
        <div className="space-y-4">
          {/* Filters & Search */}
          <div className="flex flex-col sm:flex-row items-center gap-3">
            <div className="relative flex-1 w-full">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input
                type="text"
                placeholder="Search by variety, type, bed, garden, notes..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9 w-full"
              />
            </div>
          </div>

          {/* Harvests Table */}
          {harvestsLoading ? (
            <p className="text-muted-foreground text-sm">Loading harvests...</p>
          ) : filteredHarvests.length === 0 ? (
            <p className="text-muted-foreground text-sm text-center py-8">No harvests found matching your criteria.</p>
          ) : (
            <div className="border border-border rounded-lg overflow-hidden bg-card shadow-sm">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b bg-muted/30 text-muted-foreground text-xs uppercase tracking-wider">
                    <th className="px-4 py-3 font-medium">Date</th>
                    <th className="px-4 py-3 font-medium">Garden & Bed</th>
                    <th className="px-4 py-3 font-medium">Plant Type & Variety</th>
                    <th className="px-4 py-3 font-medium text-right">Yield</th>
                    <th className="px-4 py-3 font-medium text-right">Value</th>
                    <th className="px-4 py-3 font-medium">Notes</th>
                    <th className="px-4 py-3 font-medium text-center">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {filteredHarvests.map((h) => (
                    <tr key={h.id} className="hover:bg-muted/10 transition-colors">
                      <td className="px-4 py-3 whitespace-nowrap">
                        {h.harvestDate}
                      </td>
                      <td className="px-4 py-3 whitespace-nowrap">
                        <div className="font-medium text-foreground">{h.gardenName}</div>
                        <div className="text-xs text-muted-foreground">{h.bedName}</div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="font-medium text-foreground">{h.plantVarietyName}</div>
                        <div className="text-xs text-muted-foreground">{h.plantTypeName}</div>
                      </td>
                      <td className="px-4 py-3 text-right font-medium">
                        {h.quantity} {h.unit === 'Pounds' ? 'lbs' : h.unit === 'Ounces' ? 'oz' : h.unit.toLowerCase()}
                      </td>
                      <td className="px-4 py-3 text-right text-emerald-600 dark:text-emerald-400 font-medium">
                        {h.value != null ? `$${h.value.toFixed(2)}` : '—'}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground max-w-xs truncate" title={h.notes ?? ''}>
                        {h.notes ?? <span className="text-muted-foreground/35">—</span>}
                      </td>
                      <td className="px-4 py-3 whitespace-nowrap">
                        <div className="flex items-center justify-center gap-2">
                          <Button
                            size="icon-sm"
                            variant="ghost"
                            onClick={() => { setEditingHarvest(h); setFormOpen(true) }}
                          >
                            <Edit2 className="w-3.5 h-3.5 text-muted-foreground" />
                          </Button>
                          <Button
                            size="icon-sm"
                            variant="ghost"
                            onClick={() => handleDelete(h)}
                            disabled={deleteMutation.isPending}
                          >
                            <Trash2 className="w-3.5 h-3.5 text-destructive" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      ) : (
        <div className="space-y-4">
          {/* Yearly Comparison View */}
          {harvestsLoading || compareHarvestsLoading ? (
            <p className="text-muted-foreground text-sm">Calculating yield comparison...</p>
          ) : yieldComparisonRows.length === 0 ? (
            <p className="text-muted-foreground text-sm text-center py-8">No yields logged in either {selectedYear} or {compareYear}.</p>
          ) : (
            <div className="border border-border rounded-lg overflow-hidden bg-card shadow-sm">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b bg-muted/30 text-muted-foreground text-xs uppercase tracking-wider">
                    <th className="px-4 py-3 font-medium">Plant Type & Variety</th>
                    <th className="px-4 py-3 font-medium text-right">{compareYear} Yield</th>
                    <th className="px-4 py-3 font-medium text-right">{selectedYear} Yield</th>
                    <th className="px-4 py-3 font-medium text-center">Yield Difference</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {yieldComparisonRows.map((row) => {
                    const diff = row.selectedQty - row.compareQty
                    const unitSuffix = row.unit === 'Pounds' ? ' lbs' : row.unit === 'Ounces' ? ' oz' : ` ${row.unit.toLowerCase()}`

                    let diffBadge = (
                      <span className="inline-flex items-center gap-1 text-xs font-semibold text-slate-500 bg-slate-100 dark:bg-slate-800/80 px-2 py-0.5 rounded">
                        <Equal className="w-3.5 h-3.5" /> No change
                      </span>
                    )
                    if (diff > 0) {
                      diffBadge = (
                        <span className="inline-flex items-center gap-1 text-xs font-semibold text-emerald-600 bg-emerald-50 dark:bg-emerald-950/40 px-2 py-0.5 rounded">
                          <ArrowUpRight className="w-3.5 h-3.5" /> +{diff.toFixed(1)}{unitSuffix}
                        </span>
                      )
                    } else if (diff < 0) {
                      diffBadge = (
                        <span className="inline-flex items-center gap-1 text-xs font-semibold text-rose-600 bg-rose-50 dark:bg-rose-950/40 px-2 py-0.5 rounded">
                          <ArrowDownRight className="w-3.5 h-3.5" /> {diff.toFixed(1)}{unitSuffix}
                        </span>
                      )
                    }

                    return (
                      <tr key={row.key} className="hover:bg-muted/10 transition-colors">
                        <td className="px-4 py-3">
                          <div className="font-medium text-foreground">{row.plantVarietyName}</div>
                          <div className="text-xs text-muted-foreground">{row.plantTypeName}</div>
                        </td>
                        <td className="px-4 py-3 text-right text-muted-foreground font-medium">
                          {row.compareQty > 0 ? `${row.compareQty.toFixed(1)}${unitSuffix}` : '—'}
                        </td>
                        <td className="px-4 py-3 text-right font-medium">
                          {row.selectedQty > 0 ? `${row.selectedQty.toFixed(1)}${unitSuffix}` : '—'}
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap text-center">
                          {diffBadge}
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Harvest Form Dialog */}
      <HarvestFormDialog
        open={formOpen}
        onClose={() => { setFormOpen(false); setEditingHarvest(undefined) }}
        editing={editingHarvest}
        gardenId={editingHarvest ? (editingHarvest as EnrichedHarvest).gardenId : undefined}
        year={editingHarvest ? Number(editingHarvest.harvestDate.slice(0, 4)) : selectedYear}
      />
    </div>
  )
}
