import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getGardens } from '@/api/gardens'
import { getSeasonSummary, getYearOverYear } from '@/api/reports'
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

function fmt(n: number) {
  return `$${n.toFixed(2)}`
}

export default function ReportsPage() {
  const [selectedGardenId, setSelectedGardenId] = useState<number | ''>('')
  const [selectedYear, setSelectedYear] = useState(CURRENT_YEAR)

  const { data: gardens = [] } = useQuery({
    queryKey: ['gardens'],
    queryFn: getGardens,
  })

  const { data: season, isLoading: seasonLoading, error: seasonError } = useQuery({
    queryKey: ['reports', 'season', selectedGardenId, selectedYear],
    queryFn: () => getSeasonSummary(Number(selectedGardenId), selectedYear),
    enabled: !!selectedGardenId,
    retry: false,
  })

  const { data: yearOverYear = [] } = useQuery({
    queryKey: ['reports', 'yoy', selectedGardenId],
    queryFn: () => getYearOverYear(Number(selectedGardenId)),
    enabled: !!selectedGardenId,
  })

  return (
    <div className="space-y-8">
      <div className="flex items-start justify-between flex-wrap gap-4">
        <h1 className="text-2xl font-semibold tracking-tight">Reports</h1>
        <div className="flex items-center gap-3">
          <select
            className="rounded-md border border-input bg-background px-3 py-1.5 text-sm"
            value={selectedGardenId}
            onChange={(e) => setSelectedGardenId(e.target.value ? Number(e.target.value) : '')}
          >
            <option value="">Select garden…</option>
            {gardens.map(g => (
              <option key={g.id} value={g.id}>{g.name}</option>
            ))}
          </select>
          <select
            className="rounded-md border border-input bg-background px-3 py-1.5 text-sm"
            value={selectedYear}
            onChange={(e) => setSelectedYear(Number(e.target.value))}
          >
            {YEAR_OPTIONS.map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>
        </div>
      </div>

      {!selectedGardenId ? (
        <p className="text-muted-foreground">Select a garden to view reports.</p>
      ) : (
        <div className="space-y-8">

          {/* Season Summary */}
          <section className="space-y-4">
            <h2 className="text-lg font-medium">{selectedYear} Season Summary</h2>

            {seasonLoading ? (
              <p className="text-muted-foreground text-sm">Loading…</p>
            ) : seasonError || !season ? (
              <p className="text-muted-foreground text-sm">No data for {selectedYear}.</p>
            ) : (
              <div className="space-y-4">
                {/* Top-line numbers */}
                <div className="grid gap-3 sm:grid-cols-4">
                  <StatCard label="Total Expenses" value={fmt(season.totalExpenses)} />
                  <StatCard label="Harvest Value" value={fmt(season.totalHarvestValue)} />
                  <StatCard label="Water (est.)" value={fmt(season.waterAttribution)} />
                  <StatCard
                    label="Net Cost"
                    value={fmt(season.netCost)}
                    sub={season.netCost < 0 ? 'ahead of value' : 'over harvest value'}
                    highlight={season.netCost < 0 ? 'green' : undefined}
                  />
                </div>

                <div className="grid gap-4 sm:grid-cols-2">
                  {/* Expenses by category */}
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
                                <span>{fmt(total)}</span>
                              </div>
                            ))}
                        </div>
                      </CardContent>
                    </Card>
                  )}

                  {/* Harvests */}
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
                              <span>
                                {h.quantity} {UNIT_LABELS[h.unit] ?? h.unit}
                                {h.value != null && ` · ${fmt(h.value)}`}
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

          {/* Year-over-year */}
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
                          <tr
                            key={y.year}
                            className={`border-b border-border last:border-0 ${y.year === selectedYear ? 'font-medium' : ''}`}
                          >
                            <td className="py-2">{y.year}</td>
                            <td className="py-2 text-right">{fmt(y.totalExpenses)}</td>
                            <td className="py-2 text-right">{fmt(y.totalHarvestValue)}</td>
                            <td className="py-2 text-right">{fmt(y.waterAttribution)}</td>
                            <td className={`py-2 text-right ${y.netCost < 0 ? 'text-green-600' : ''}`}>
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

          {/* Monthly breakdown */}
          {season && season.harvestLines.length + Object.keys(season.expensesByCategory).length > 0 && (
            <section className="space-y-4">
              <h2 className="text-lg font-medium">Monthly Breakdown</h2>
              {yearOverYear.filter(y => y.year === selectedYear).map(y => (
                y.months.length > 0 && (
                  <Card key={y.year}>
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
                                <td className="py-2 text-right">{fmt(m.totalExpenses)}</td>
                                <td className="py-2 text-right">{fmt(m.totalHarvestValue)}</td>
                                <td className="py-2 text-right">{fmt(m.waterAttribution)}</td>
                                <td className={`py-2 text-right ${m.netCost < 0 ? 'text-green-600' : ''}`}>
                                  {fmt(m.netCost)}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </CardContent>
                  </Card>
                )
              ))}
            </section>
          )}

        </div>
      )}
    </div>
  )
}

function StatCard({ label, value, sub, highlight }: {
  label: string
  value: string
  sub?: string
  highlight?: 'green'
}) {
  return (
    <Card>
      <CardContent className="pt-4">
        <p className="text-xs text-muted-foreground uppercase tracking-wide">{label}</p>
        <p className={`text-2xl font-semibold mt-1 ${highlight === 'green' ? 'text-green-600' : ''}`}>
          {value}
        </p>
        {sub && <p className="text-xs text-muted-foreground mt-0.5">{sub}</p>}
      </CardContent>
    </Card>
  )
}
