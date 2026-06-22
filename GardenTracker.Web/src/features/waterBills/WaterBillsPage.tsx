import { useState, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getWaterBills, deleteWaterBill, importWaterBillsCsv } from '@/api/waterBills'
import type { WaterBill } from '@/types/waterBill'
import { useSearchParams } from 'react-router-dom'
import WaterBillFormDialog from './WaterBillFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Upload, Plus, Droplets } from 'lucide-react'

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
  const [searchParams] = useSearchParams()
  const summaryYear = Number(searchParams.get('year')) || CURRENT_YEAR
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

  // Pagination over the full sorted list (already DESC from DB)
  const totalPages = Math.max(1, Math.ceil(allBills.length / PAGE_SIZE))
  const pageBills = allBills.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  return (
    <div className="flex flex-col h-full overflow-y-auto p-6 lg:p-8 space-y-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">Water Bills</h1>
          <p className="mt-1 text-muted-foreground">
            Track monthly usage and estimate garden water cost.
          </p>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          <Button size="sm" variant="outline" className="gap-2" onClick={() => fileInputRef.current?.click()} disabled={importMutation.isPending}>
            <Upload className="w-4 h-4" />
            {importMutation.isPending ? 'Importing…' : 'Import'}
          </Button>
          <input ref={fileInputRef} type="file" accept=".csv" className="hidden" onChange={handleFileChange} />
          <Button size="sm" className="gap-2" onClick={openAdd}>
            <Plus className="w-4 h-4" /> Add Bill
          </Button>
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
        <div className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-3">
            <Card className="border-border shadow-sm">
              <CardContent className="pt-5">
                <p className="text-xs text-muted-foreground font-semibold uppercase tracking-wider">Year Total</p>
                <p className="text-3xl font-bold mt-1 text-foreground">${yearTotal.toFixed(2)}</p>
                <p className="text-sm text-muted-foreground mt-1">{summaryBills.length} months logged</p>
              </CardContent>
            </Card>
            <Card className="border-border shadow-sm">
              <CardContent className="pt-5">
                <p className="text-xs text-muted-foreground font-semibold uppercase tracking-wider">Monthly Baseline</p>
                {baseline != null ? (
                  <>
                    <p className="text-3xl font-bold mt-1 text-foreground">${baseline.toFixed(2)}</p>
                    <p className="text-sm text-muted-foreground mt-1">
                      avg of {summaryBills.filter(b => !b.isGardenActive).length} non-garden months
                    </p>
                  </>
                ) : (
                  <p className="text-sm text-muted-foreground mt-3">No baseline months yet</p>
                )}
              </CardContent>
            </Card>
            <Card className="border-border shadow-sm bg-blue-50/50 dark:bg-blue-900/10 border-blue-100 dark:border-blue-800/30">
              <CardContent className="pt-5">
                <p className="text-xs text-blue-600 dark:text-blue-400 font-semibold uppercase tracking-wider">Garden Attribution</p>
                {totalGardenAttribution != null ? (
                  <>
                    <p className="text-3xl font-bold mt-1 text-blue-700 dark:text-blue-300">${totalGardenAttribution.toFixed(2)}</p>
                    <p className="text-sm text-blue-600/80 dark:text-blue-400/80 mt-1">
                      est. above baseline across {gardenMonths.length} garden months
                    </p>
                  </>
                ) : (
                  <p className="text-sm text-blue-600/80 dark:text-blue-400/80 mt-3">Needs baseline to calculate</p>
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
        <div className="flex flex-col items-center justify-center py-12 text-center border-2 border-dashed rounded-xl h-64 mt-4">
          <div className="h-12 w-12 rounded-full bg-blue-500/10 flex items-center justify-center mb-4">
            <Droplets className="h-6 w-6 text-blue-500" />
          </div>
          <h3 className="text-lg font-medium">No water bills</h3>
          <p className="text-sm text-muted-foreground max-w-sm mt-1 mb-4">
            Add your monthly water bills to calculate how much water your garden uses.
          </p>
          <Button onClick={openAdd} variant="outline">Add Bill</Button>
        </div>
      ) : (
        <div className="space-y-4">
          <div className="space-y-2">
            {pageBills.map((bill) => {
              const billBaseline = computeBaseline(allBills.filter(b => b.year === bill.year))
              const attribution = billBaseline != null && bill.isGardenActive
                ? Math.max(0, bill.totalCost - billBaseline)
                : null

              return (
                <Card key={bill.id} className="border-border shadow-sm">
                  <CardContent className="pt-4 pb-4">
                    <div className="flex items-center justify-between gap-4">
                      <div className="flex items-center gap-6 flex-1 flex-wrap">
                        <div className="w-24 text-base font-medium">
                          {MONTH_NAMES[bill.month - 1]} {bill.year}
                        </div>
                        <div className="text-lg font-semibold w-20">${bill.totalCost.toFixed(2)}</div>
                        <div className="text-sm text-muted-foreground w-32">
                          {bill.usageCcf} CCF <br className="hidden sm:block" />
                          <span className="text-xs">({Math.round(bill.usageGallons).toLocaleString()} gal)</span>
                        </div>
                        <div className="w-32">
                          {bill.isGardenActive ? (
                            <Badge variant="secondary" className="bg-blue-500/10 text-blue-600 dark:text-blue-400">Garden active</Badge>
                          ) : (
                            <Badge variant="outline" className="text-muted-foreground">Baseline</Badge>
                          )}
                        </div>
                        <div className="flex-1">
                          {attribution != null && attribution > 0 && (
                            <span className="text-sm font-medium text-blue-600 dark:text-blue-400">
                              ~${attribution.toFixed(2)} garden est.
                            </span>
                          )}
                          {bill.notes && (
                            <p className="text-xs text-muted-foreground mt-1">{bill.notes}</p>
                          )}
                        </div>
                      </div>
                      <div className="flex gap-2 shrink-0">
                        <Button variant="outline" size="sm" onClick={() => openEdit(bill)}>Edit</Button>
                        <Button
                          variant="outline"
                          size="sm"
                          className="text-destructive hover:bg-destructive/10"
                          onClick={() => handleDelete(bill)}
                          disabled={deleteMutation.isPending}
                        >
                          Delete
                        </Button>
                      </div>
                    </div>
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
