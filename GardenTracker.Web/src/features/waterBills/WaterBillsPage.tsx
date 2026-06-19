import { useState, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getWaterBills, deleteWaterBill, importWaterBillsCsv } from '@/api/waterBills'
import type { WaterBill } from '@/types/waterBill'
import WaterBillFormDialog from './WaterBillFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'

const CURRENT_YEAR = new Date().getFullYear()
const PAGE_SIZE = 12

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
  const [summaryYear, setSummaryYear] = useState(CURRENT_YEAR)
  const [page, setPage] = useState(1)
  const [formOpen, setFormOpen] = useState(false)
  const [editingBill, setEditingBill] = useState<WaterBill | undefined>()
  const [importStatus, setImportStatus] = useState<{ created: number; updated: number; errors: string[] } | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  // Fetch all bills — DB returns them Year DESC, Month DESC
  const { data: allBills = [], isLoading } = useQuery({
    queryKey: ['water-bills'],
    queryFn: () => getWaterBills(),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteWaterBill(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['water-bills'] }),
  })

  const importMutation = useMutation({
    mutationFn: (file: File) => importWaterBillsCsv(file),
    onSuccess: (result) => {
      setImportStatus(result)
      setPage(1)
      qc.invalidateQueries({ queryKey: ['water-bills'] })
      if (fileInputRef.current) fileInputRef.current.value = ''
    },
    onError: (err: Error) => {
      setImportStatus({ created: 0, updated: 0, errors: [err.message ?? 'Import failed. Check the API is running.'] })
      if (fileInputRef.current) fileInputRef.current.value = ''
    },
  })

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) importMutation.mutate(file)
  }

  function openAdd() { setEditingBill(undefined); setFormOpen(true) }
  function openEdit(bill: WaterBill) { setEditingBill(bill); setFormOpen(true) }
  function handleDelete(bill: WaterBill) {
    if (confirm(`Delete ${MONTH_NAMES[bill.month - 1]} ${bill.year} water bill?`))
      deleteMutation.mutate(bill.id)
  }

  // Summary stats scoped to the selected year
  const summaryBills = allBills.filter(b => b.year === summaryYear)
  const baseline = computeBaseline(summaryBills)
  const gardenMonths = summaryBills.filter(b => b.isGardenActive)
  const yearTotal = summaryBills.reduce((sum, b) => sum + b.totalCost, 0)
  const totalGardenAttribution = baseline != null
    ? gardenMonths.reduce((sum, b) => sum + Math.max(0, b.totalCost - baseline), 0)
    : null

  // Available years for the summary selector
  const availableYears = Array.from(new Set(allBills.map(b => b.year))).sort((a, b) => b - a)
  const yearOptions = availableYears.length > 0
    ? availableYears
    : Array.from({ length: 3 }, (_, i) => CURRENT_YEAR - i)

  // Pagination over the full sorted list (already DESC from DB)
  const totalPages = Math.max(1, Math.ceil(allBills.length / PAGE_SIZE))
  const pageBills = allBills.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

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
          <Button size="sm" variant="outline" onClick={() => fileInputRef.current?.click()} disabled={importMutation.isPending}>
            {importMutation.isPending ? 'Importing…' : 'Import CSV'}
          </Button>
          <input ref={fileInputRef} type="file" accept=".csv" className="hidden" onChange={handleFileChange} />
          <Button size="sm" onClick={openAdd}>Add Bill</Button>
        </div>
      </div>

      {importStatus && (
        <div className={`rounded-lg border px-4 py-3 text-sm ${importStatus.errors.length > 0 ? 'border-destructive bg-destructive/5' : 'border-green-500 bg-green-500/5'}`}>
          <p className="font-medium">
            Import complete: {importStatus.created} created, {importStatus.updated} updated
            {importStatus.errors.length > 0 && `, ${importStatus.errors.length} error(s)`}
          </p>
          {importStatus.errors.length > 0 && (
            <ul className="mt-1 space-y-0.5 text-destructive">
              {importStatus.errors.map((e, i) => <li key={i}>• {e}</li>)}
            </ul>
          )}
          <button onClick={() => setImportStatus(null)} className="mt-1 text-xs underline-offset-2 hover:underline text-muted-foreground">Dismiss</button>
        </div>
      )}

      {/* Summary cards */}
      {allBills.length > 0 && (
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <span className="text-sm font-medium">Summary for</span>
            <select
              className="rounded-md border border-input bg-background px-2 py-1 text-sm"
              value={summaryYear}
              onChange={(e) => setSummaryYear(Number(e.target.value))}
            >
              {yearOptions.map(y => <option key={y} value={y}>{y}</option>)}
            </select>
          </div>
          <div className="grid gap-3 sm:grid-cols-3">
            <Card>
              <CardContent className="pt-4">
                <p className="text-xs text-muted-foreground uppercase tracking-wide">Year Total</p>
                <p className="text-2xl font-semibold mt-1">${yearTotal.toFixed(2)}</p>
                <p className="text-xs text-muted-foreground mt-0.5">{summaryBills.length} months logged</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="pt-4">
                <p className="text-xs text-muted-foreground uppercase tracking-wide">Monthly Baseline</p>
                {baseline != null ? (
                  <>
                    <p className="text-2xl font-semibold mt-1">${baseline.toFixed(2)}</p>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      avg of {summaryBills.filter(b => !b.isGardenActive).length} non-garden months
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
        </div>
      )}

      {/* Bill list */}
      {isLoading ? (
        <p className="text-muted-foreground">Loading…</p>
      ) : allBills.length === 0 ? (
        <p className="text-muted-foreground">No water bills logged yet.</p>
      ) : (
        <div className="space-y-4">
          <div className="space-y-2">
            {pageBills.map((bill) => {
              const billBaseline = computeBaseline(allBills.filter(b => b.year === bill.year))
              const attribution = billBaseline != null && bill.isGardenActive
                ? Math.max(0, bill.totalCost - billBaseline)
                : null

              return (
                <Card key={bill.id}>
                  <CardContent className="pt-3 pb-3">
                    <div className="flex items-center justify-between gap-3">
                      <div className="flex items-center gap-4 flex-1 flex-wrap">
                        <div className="w-16 text-sm font-medium">
                          {MONTH_NAMES[bill.month - 1]} {bill.year}
                        </div>
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
                      <p className="text-xs text-muted-foreground mt-1 ml-[4.5rem]">{bill.notes}</p>
                    )}
                  </CardContent>
                </Card>
              )
            })}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">
                Page {page} of {totalPages} · {allBills.length} bills
              </span>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(p => p - 1)}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(p => p + 1)}
                  disabled={page === totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </div>
      )}

      <WaterBillFormDialog
        open={formOpen}
        onClose={() => setFormOpen(false)}
        editing={editingBill}
        defaultYear={CURRENT_YEAR}
      />
    </div>
  )
}
