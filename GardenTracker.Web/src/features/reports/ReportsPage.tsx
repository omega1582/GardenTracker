import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getGardens } from '@/api/gardens'
import { getSeasonSummary, getYearOverYear, getWaterAttribution, getBedBreakdown } from '@/api/reports'
import { Card, CardContent } from '@/components/ui/card'

const CURRENT_YEAR = new Date().getFullYear()
const YEAR_OPTIONS = Array.from({ length: 6 }, (_, i) => CURRENT_YEAR - i)

const MONTH_NAMES = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
  'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
]

const UNIT_LABELS: Record<string, string> = {
  Pounds: 'lbs', Ounces: 'oz', Count: 'each', Bunch: 'bunch',
}

function fmt(n: number) { return `$${n.toFixed(2)}` }

export default function ReportsPage() {
  const [selectedGardenId, setSelectedGardenId] = useState<number | ''>('')
  const [selectedYear, setSelectedYear] = useState(CURRENT_YEAR)

  const { data: gardens = [] } = useQuery({ queryKey: ['gardens'], queryFn: getGardens })

  const enabled = !!selectedGardenId

  const { data: season, isLoading: seasonLoading, error: seasonError } = useQuery({
    queryKey: ['reports', 'season', selectedGardenId, selectedYear],
    queryFn: () => getSeasonSummary(Number(selectedGardenId), selectedYear),
    enabled, retry: false,
  })

  const { data: yearOverYear = [] } = useQuery({
    queryKey: ['reports', 'yoy', selectedGardenId],
    queryFn: () => getYearOverYear(Number(selectedGardenId)),
    enabled,
  })

  const { data: waterAttribution = [] } = useQuery({
    queryKey: ['reports', 'water', selectedYear],
    queryFn: () => getWaterAttribution(selectedYear),
    enabled,
  })

  const { data: bedBreakdown = [] } = useQuery({
    queryKey: ['reports', 'beds', selectedGardenId, selectedYear],
    queryFn: () => getBedBreakdown(Number(selectedGardenId), selectedYear),
    enabled,
  })

  const waterYear = waterAttribution[0]
  const hasHarvestValue = season && season.totalHarvestValue === 0 && season.harvestLines.length > 0

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-start justify-between flex-wrap gap-4">
        <h1 className="text-2xl font-semibold tracking-tight">Reports</h1>
        <div className="flex items-center gap-3">
          <select
            className="rounded-md border border-input bg-background px-3 py-1.5 text-sm"
            value={selectedGardenId}
            onChange={(e) => setSelectedGardenId(e.target.value ? Number(e.target.value) : '')}
          >
            <option value="">Select garden…</option>
            {gardens.map(g => <option key={g.id} value={g.id}>{g.name}</option>)}
          </select>
          <select
            className="rounded-md border border-input bg-background px-3 py-1.5 text-sm"
            value={selectedYear}
            onChange={(e) => setSelectedYear(Number(e.target.value))}
          >
            {YEAR_OPTIONS.map(y => <option key={y} value={y}>{y}</option>)}
          </select>
        </div>
      </div>

      {!selectedGardenId ? (
        <p className="text-muted-foreground">Select a garden to view reports.</p>
      ) : (
        <div className="space-y-10">

          {/* ── Season Summary ──────────────────────────────────────── */}
          <section className="space-y-4">
            <h2 className="text-lg font-medium">{selectedYear} Season Summary</h2>
            {seasonLoading ? (
              <p className="text-muted-foreground text-sm">Loading…</p>
            ) : seasonError || !season ? (
              <p className="text-muted-foreground text-sm">No data for {selectedYear}.</p>
            ) : (
              <div className="space-y-4">
                <div className="grid gap-3 sm:grid-cols-4">
                  <StatCard label="Total Expenses" value={fmt(season.totalExpenses)} />
                  <StatCard
                    label="Harvest Value"
                    value={fmt(season.totalHarvestValue)}
                    sub={hasHarvestValue ? 'Set market prices to calculate' : undefined}
                    warn={!!hasHarvestValue}
                  />
                  <StatCard label="Water (est.)" value={fmt(season.waterAttribution)} />
                  <StatCard
                    label="Net Cost"
                    value={fmt(season.netCost)}
                    sub={season.netCost < 0 ? 'ahead of harvest value' : 'over harvest value'}
                    highlight={season.netCost < 0 ? 'green' : undefined}
                  />
                </div>

                <div className="grid gap-4 sm:grid-cols-2">
                  {Object.keys(season.expensesByCategory).length > 0 && (
                    <Card>
                      <CardContent className="pt-4 space-y-2">
                        <p className="text-sm font-medium">Expenses by Category</p>
                        <div className="space-y-1">
                          {Object.entries(season.expensesByCategory)
                            .sort(([, a], [, b]) => b - a)
                            .map(([cat, total]) => (
                              <div key={cat} className="flex justify-between text-sm">
                                <span className="text-muted-foreground">{cat}</span>
                                <span className="tabular-nums">{fmt(total)}</span>
                              </div>
                            ))}
                        </div>
                      </CardContent>
                    </Card>
                  )}

                  {season.harvestLines.length > 0 && (
                    <Card>
                      <CardContent className="pt-4 space-y-2">
                        <p className="text-sm font-medium">Harvests</p>
                        <div className="space-y-1">
                          {season.harvestLines.map((h, i) => (
                            <div key={i} className="flex justify-between text-sm">
                              <span className="text-muted-foreground">
                                {h.plantTypeName} — {h.varietyName}
                              </span>
                              <span className="tabular-nums">
                                {h.quantity} {UNIT_LABELS[h.unit] ?? h.unit}
                                {h.value != null ? ` · ${fmt(h.value)}` : ' · no price set'}
                              </span>
                            </div>
                          ))}
                        </div>
                      </CardContent>
                    </Card>
                  )}
                </div>
              </div>
            )}
          </section>

          {/* ── Per-bed Breakdown ───────────────────────────────────── */}
          {bedBreakdown.length > 0 && (
            <section className="space-y-4">
              <h2 className="text-lg font-medium">By Bed</h2>
              <div className="grid gap-3 sm:grid-cols-2">
                {bedBreakdown.map(bed => (
                  <Card key={bed.bedId}>
                    <CardContent className="pt-4 space-y-3">
                      <div className="flex items-center justify-between">
                        <p className="font-medium">{bed.bedName}</p>
                        <span className={`text-sm tabular-nums font-medium ${bed.netCost < 0 ? 'text-green-600' : ''}`}>
                          {fmt(bed.netCost)} net
                        </span>
                      </div>
                      <div className="space-y-1">
                        {bed.totalExpenses > 0 && (
                          <div className="flex justify-between text-sm">
                            <span className="text-muted-foreground">Expenses</span>
                            <span className="tabular-nums">{fmt(bed.totalExpenses)}</span>
                          </div>
                        )}
                        {bed.harvestLines.map((h, i) => (
                          <div key={i} className="flex justify-between text-sm">
                            <span className="text-muted-foreground">
                              {h.plantTypeName} — {h.varietyName}
                            </span>
                            <span className="tabular-nums">
                              {h.quantity} {UNIT_LABELS[h.unit] ?? h.unit}
                              {h.value != null && ` · ${fmt(h.value)}`}
                            </span>
                          </div>
                        ))}
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </section>
          )}

          {/* ── Water Attribution ───────────────────────────────────── */}
          {waterYear && (
            <section className="space-y-4">
              <h2 className="text-lg font-medium">Water Attribution — {selectedYear}</h2>
              {!waterYear.baselineMonthlyCost ? (
                <p className="text-muted-foreground text-sm">
                  No baseline months — mark some water bills as non-garden months to calculate attribution.
                </p>
              ) : (
                <div className="space-y-4">
                  <div className="grid gap-3 sm:grid-cols-3">
                    <StatCard
                      label="Monthly Baseline"
                      value={fmt(waterYear.baselineMonthlyCost)}
                      sub={`${Math.round(waterYear.baselineMonthlyGallons ?? 0).toLocaleString()} gal avg`}
                    />
                    <StatCard label="Garden Attribution" value={fmt(waterYear.totalAttributedCost)} />
                    <StatCard
                      label="Attributed Gallons"
                      value={`${Math.round(waterYear.totalAttributedGallons).toLocaleString()}`}
                    />
                  </div>
                  {waterYear.activeMonths.length > 0 && (
                    <Card>
                      <CardContent className="pt-4">
                        <div className="overflow-x-auto">
                          <table className="w-full text-sm">
                            <thead>
                              <tr className="border-b border-border">
                                <th className="text-left font-medium pb-2 text-muted-foreground">Month</th>
                                <th className="text-right font-medium pb-2 text-muted-foreground">Bill</th>
                                <th className="text-right font-medium pb-2 text-muted-foreground">Gallons</th>
                                <th className="text-right font-medium pb-2 text-muted-foreground">Garden est.</th>
                              </tr>
                            </thead>
                            <tbody>
                              {waterYear.activeMonths.map(m => (
                                <tr key={m.month} className="border-b border-border last:border-0">
                                  <td className="py-2">{MONTH_NAMES[m.month - 1]}</td>
                                  <td className="py-2 text-right tabular-nums">{fmt(m.totalCost)}</td>
                                  <td className="py-2 text-right tabular-nums">{Math.round(m.usageGallons).toLocaleString()}</td>
                                  <td className="py-2 text-right tabular-nums">
                                    {m.attributedCost != null ? fmt(m.attributedCost) : '—'}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </CardContent>
                    </Card>
                  )}
                </div>
              )}
            </section>
          )}

          {/* ── Year over Year ──────────────────────────────────────── */}
          {yearOverYear.length > 1 && (
            <section className="space-y-4">
              <h2 className="text-lg font-medium">Year over Year</h2>
              <Card>
                <CardContent className="pt-4">
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-border">
                          <th className="text-left font-medium pb-2 text-muted-foreground">Year</th>
                          <th className="text-right font-medium pb-2 text-muted-foreground">Expenses</th>
                          <th className="text-right font-medium pb-2 text-muted-foreground">Harvest Value</th>
                          <th className="text-right font-medium pb-2 text-muted-foreground">Water</th>
                          <th className="text-right font-medium pb-2 text-muted-foreground">Net Cost</th>
                        </tr>
                      </thead>
                      <tbody>
                        {yearOverYear.map(y => (
                          <tr key={y.year} className={`border-b border-border last:border-0 ${y.year === selectedYear ? 'font-medium' : ''}`}>
                            <td className="py-2">{y.year}</td>
                            <td className="py-2 text-right tabular-nums">{fmt(y.totalExpenses)}</td>
                            <td className="py-2 text-right tabular-nums">{fmt(y.totalHarvestValue)}</td>
                            <td className="py-2 text-right tabular-nums">{fmt(y.waterAttribution)}</td>
                            <td className={`py-2 text-right tabular-nums ${y.netCost < 0 ? 'text-green-600' : ''}`}>
                              {fmt(y.netCost)}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </CardContent>
              </Card>
            </section>
          )}

          {/* ── Monthly Breakdown ───────────────────────────────────── */}
          {yearOverYear.filter(y => y.year === selectedYear).map(y =>
            y.months.length > 0 ? (
              <section key={y.year} className="space-y-4">
                <h2 className="text-lg font-medium">Monthly Breakdown</h2>
                <Card>
                  <CardContent className="pt-4">
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="border-b border-border">
                            <th className="text-left font-medium pb-2 text-muted-foreground">Month</th>
                            <th className="text-right font-medium pb-2 text-muted-foreground">Expenses</th>
                            <th className="text-right font-medium pb-2 text-muted-foreground">Harvest Value</th>
                            <th className="text-right font-medium pb-2 text-muted-foreground">Water</th>
                            <th className="text-right font-medium pb-2 text-muted-foreground">Net</th>
                          </tr>
                        </thead>
                        <tbody>
                          {y.months.map(m => (
                            <tr key={m.month} className="border-b border-border last:border-0">
                              <td className="py-2">{MONTH_NAMES[m.month - 1]}</td>
                              <td className="py-2 text-right tabular-nums">{fmt(m.totalExpenses)}</td>
                              <td className="py-2 text-right tabular-nums">{fmt(m.totalHarvestValue)}</td>
                              <td className="py-2 text-right tabular-nums">{fmt(m.waterAttribution)}</td>
                              <td className={`py-2 text-right tabular-nums ${m.netCost < 0 ? 'text-green-600' : ''}`}>
                                {fmt(m.netCost)}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </CardContent>
                </Card>
              </section>
            ) : null
          )}

        </div>
      )}
    </div>
  )
}

function StatCard({ label, value, sub, highlight, warn }: {
  label: string
  value: string
  sub?: string
  highlight?: 'green'
  warn?: boolean
}) {
  return (
    <Card>
      <CardContent className="pt-4">
        <p className="text-xs text-muted-foreground uppercase tracking-wide">{label}</p>
        <p className={`text-2xl font-semibold mt-1 tabular-nums ${highlight === 'green' ? 'text-green-600' : warn ? 'text-muted-foreground' : ''}`}>
          {value}
        </p>
        {sub && <p className={`text-xs mt-0.5 ${warn ? 'text-destructive/70' : 'text-muted-foreground'}`}>{sub}</p>}
      </CardContent>
    </Card>
  )
}
