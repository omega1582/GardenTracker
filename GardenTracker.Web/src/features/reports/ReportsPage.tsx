import { useQuery } from '@tanstack/react-query'
import { getSeasonSummary, getYearOverYear, getWaterAttribution, getBedBreakdown } from '@/api/reports'
import { Card, CardContent } from '@/components/ui/card'
import { useSearchParams } from 'react-router-dom'

const CURRENT_YEAR = new Date().getFullYear()

const MONTH_NAMES = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
  'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
]

const UNIT_LABELS: Record<string, string> = {
  Pounds: 'lbs', Ounces: 'oz', Count: 'each', Bunch: 'bunch',
}

function fmt(n: number) { return `$${n.toFixed(2)}` }

export default function ReportsPage() {
  const [searchParams] = useSearchParams()
  const paramGardenId = searchParams.get('gardenId')
  const paramYear = searchParams.get('year')
  
  const selectedGardenId = paramGardenId ? Number(paramGardenId) : ''
  const selectedYear = paramYear ? Number(paramYear) : CURRENT_YEAR

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
    <div className="flex flex-col h-full overflow-y-auto p-6 lg:p-8 space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">Reports</h1>
          <p className="mt-1 text-muted-foreground">Analyze your garden's performance and costs.</p>
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
                    <Card className="border-border shadow-sm">
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
                    <Card className="border-border shadow-sm">
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
              <h2 className="text-xl font-semibold mb-4 text-emerald-700 dark:text-emerald-400 border-b pb-2">By Bed</h2>
              <div className="grid gap-4 sm:grid-cols-2">
                {bedBreakdown.map(bed => (
                  <Card key={bed.bedId} className="border-border shadow-sm">
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
              <h2 className="text-xl font-semibold mb-4 text-blue-700 dark:text-blue-400 border-b pb-2">Water Attribution — {selectedYear}</h2>
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
                    <Card className="border-border shadow-sm">
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
              <h2 className="text-xl font-semibold mb-4 border-b pb-2">Year over Year</h2>
              <Card className="border-border shadow-sm">
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
                <h2 className="text-xl font-semibold mb-4 border-b pb-2">Monthly Breakdown</h2>
                <Card className="border-border shadow-sm">
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
    <Card className="border-border shadow-sm">
      <CardContent className="pt-5">
        <p className="text-xs text-muted-foreground font-semibold uppercase tracking-wider">{label}</p>
        <p className={`text-3xl font-bold mt-1 tabular-nums ${highlight === 'green' ? 'text-green-600 dark:text-green-400' : warn ? 'text-muted-foreground' : 'text-foreground'}`}>
          {value}
        </p>
        {sub && <p className={`text-sm mt-1 ${warn ? 'text-destructive/70' : 'text-muted-foreground'}`}>{sub}</p>}
      </CardContent>
    </Card>
  )
}
