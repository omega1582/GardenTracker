import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getWaterBills, deleteWaterBill } from '@/api/waterBills'
import type { WaterBill } from '@/types/waterBill'
import WaterBillFormDialog from './WaterBillFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'

const CURRENT_YEAR = new Date().getFullYear()

const MONTH_NAMES = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
  'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
]

function computeBaseline(bills: WaterBill[]): number | null {
  const baselineMonths = bills.filter(b => !b.isGardenActive)
  if (baselineMonths.length === 0) return null
  return baselineMonths.reduce((sum, b) => sum + b.totalCost, 0) / baselineMonths.length
}

export default function WaterBillsPage() {
  const qc = useQueryClient()
  const [selectedYear, setSelectedYear] = useState(CURRENT_YEAR)
  const [formOpen, setFormOpen] = useState(false)
  const [editingBill, setEditingBill] = useState<WaterBill | undefined>()

  const { data: bills = [], isLoading } = useQuery({
    queryKey: ['water-bills', selectedYear],
    queryFn: () => getWaterBills(selectedYear),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteWaterBill(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['water-bills'] }),
  })

  const baseline = computeBaseline(bills)
  const gardenMonths = bills.filter(b => b.isGardenActive)
  const totalGardenAttribution = baseline != null
    ? gardenMonths.reduce((sum, b) => sum + Math.max(0, b.totalCost - baseline), 0)
    : null
  const yearTotal = bills.reduce((sum, b) => sum + b.totalCost, 0)

  // Build year options from the last 5 years
  const yearOptions = Array.from({ length: 5 }, (_, i) => CURRENT_YEAR - i)

  function openAdd() {
    setEditingBill(undefined)
    setFormOpen(true)
  }

  function openEdit(bill: WaterBill) {
    setEditingBill(bill)
    setFormOpen(true)
  }

  function handleDelete(bill: WaterBill) {
    if (confirm(`Delete ${MONTH_NAMES[bill.month - 1]} ${bill.year} water bill?`)) {
      deleteMutation.mutate(bill.id)
    }
  }

  // Sort bills by month
  const sorted = [...bills].sort((a, b) => a.month - b.month)

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Water Bills</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Track monthly usage and estimate garden water cost.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <select
            className="rounded-md border border-input bg-background px-3 py-1.5 text-sm"
            value={selectedYear}
            onChange={(e) => setSelectedYear(Number(e.target.value))}
          >
            {yearOptions.map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>
          <Button size="sm" onClick={openAdd}>Add Bill</Button>
        </div>
      </div>

      {/* Summary cards */}
      {bills.length > 0 && (
        <div className="grid gap-3 sm:grid-cols-3">
          <Card>
            <CardContent className="pt-4">
              <p className="text-xs text-muted-foreground uppercase tracking-wide">Year Total</p>
              <p className="text-2xl font-semibold mt-1">${yearTotal.toFixed(2)}</p>
              <p className="text-xs text-muted-foreground mt-0.5">{bills.length} months logged</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-4">
              <p className="text-xs text-muted-foreground uppercase tracking-wide">Monthly Baseline</p>
              {baseline != null ? (
                <>
                  <p className="text-2xl font-semibold mt-1">${baseline.toFixed(2)}</p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    avg of {bills.filter(b => !b.isGardenActive).length} non-garden months
                  </p>
                </>
              ) : (
                <p className="text-sm text-muted-foreground mt-2">No baseline months yet</p>
              )}
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-4">
              <p className="text-xs text-muted-foreground uppercase tracking-wide">Garden Attribution</p>
              {totalGardenAttribution != null ? (
                <>
                  <p className="text-2xl font-semibold mt-1">${totalGardenAttribution.toFixed(2)}</p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    est. above baseline across {gardenMonths.length} garden months
                  </p>
                </>
              ) : (
                <p className="text-sm text-muted-foreground mt-2">Needs baseline to calculate</p>
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {/* Bill list */}
      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : bills.length === 0 ? (
        <p className="text-muted-foreground">No water bills logged for {selectedYear} yet.</p>
      ) : (
        <div className="space-y-2">
          {sorted.map((bill) => {
            const attribution = baseline != null && bill.isGardenActive
              ? Math.max(0, bill.totalCost - baseline)
              : null

            return (
              <Card key={bill.id}>
                <CardContent className="pt-3 pb-3">
                  <div className="flex items-center justify-between gap-3">
                    <div className="flex items-center gap-4 flex-1 flex-wrap">
                      <div className="w-10 text-sm font-medium">{MONTH_NAMES[bill.month - 1]}</div>
                      <div className="text-sm font-semibold">${bill.totalCost.toFixed(2)}</div>
                      <div className="text-xs text-muted-foreground">
                        {bill.usageCcf} CCF · {Math.round(bill.usageGallons).toLocaleString()} gal
                      </div>
                      {bill.isGardenActive ? (
                        <Badge variant="secondary" className="text-xs">Garden active</Badge>
                      ) : (
                        <Badge variant="outline" className="text-xs text-muted-foreground">Baseline month</Badge>
                      )}
                      {attribution != null && attribution > 0 && (
                        <span className="text-xs text-muted-foreground">
                          ~${attribution.toFixed(2)} garden est.
                        </span>
                      )}
                    </div>
                    <div className="flex gap-1 shrink-0">
                      <Button variant="ghost" size="sm" onClick={() => openEdit(bill)}>Edit</Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive"
                        onClick={() => handleDelete(bill)}
                        disabled={deleteMutation.isPending}
                      >
                        Delete
                      </Button>
                    </div>
                  </div>
                  {bill.notes && (
                    <p className="text-xs text-muted-foreground mt-1 ml-14">{bill.notes}</p>
                  )}
                </CardContent>
              </Card>
            )
          })}
        </div>
      )}

      <WaterBillFormDialog
        open={formOpen}
        onClose={() => setFormOpen(false)}
        editing={editingBill}
        defaultYear={selectedYear}
      />
    </div>
  )
}
