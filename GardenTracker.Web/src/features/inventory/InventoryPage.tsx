import { useState, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getInventory, deleteInventoryItem, adjustInventoryRemaining, exportInventoryCsv, importInventoryCsv } from '@/api/inventory'
import type { InventoryItem } from '@/types/inventory'
import InventoryFormDialog from './InventoryFormDialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'

const CURRENT_YEAR = new Date().getFullYear()
const ALL_YEARS = 'all'

export default function InventoryPage() {
  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<InventoryItem | undefined>()
  const [selectedYear, setSelectedYear] = useState<number | 'all'>(CURRENT_YEAR)
  const [importStatus, setImportStatus] = useState<{ created: number; updated: number; errors: string[] } | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const qc = useQueryClient()

  const { data: items = [], isLoading } = useQuery({
    queryKey: ['inventory'],
    queryFn: getInventory,
  })

  const exportMutation = useMutation({
    mutationFn: exportInventoryCsv,
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `inventory-${new Date().toISOString().slice(0, 10)}.csv`
      a.click()
      URL.revokeObjectURL(url)
    },
  })

  const importMutation = useMutation({
    mutationFn: (file: File) => importInventoryCsv(file),
    onSuccess: (result) => {
      setImportStatus(result)
      qc.invalidateQueries({ queryKey: ['inventory'] })
      if (fileInputRef.current) fileInputRef.current.value = ''
    },
  })

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) importMutation.mutate(file)
  }

  function openAdd() {
    setEditing(undefined)
    setFormOpen(true)
  }

  function openEdit(item: InventoryItem) {
    setEditing(item)
    setFormOpen(true)
  }

  if (isLoading) return <p className="text-muted-foreground">Loading…</p>

  // Build year options from purchase dates present in data
  const years = Array.from(
    new Set(items.map(i => Number(i.purchaseDate.slice(0, 4))))
  ).sort((a, b) => b - a)

  const filtered = selectedYear === ALL_YEARS
    ? items
    : items.filter(i => i.purchaseDate.startsWith(String(selectedYear)))

  // Group by plant type, then variety
  const grouped = filtered.reduce<Record<string, Record<string, InventoryItem[]>>>((acc, item) => {
    const pt = item.plantTypeName
    const pv = item.plantVarietyName
    acc[pt] ??= {}
    acc[pt][pv] ??= []
    acc[pt][pv].push(item)
    return acc
  }, {})

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">Inventory</h1>
        <div className="flex items-center gap-3">
          {years.length > 0 && (
            <select
              className="rounded-md border border-input bg-background px-3 py-1.5 text-sm"
              value={selectedYear}
              onChange={(e) => setSelectedYear(e.target.value === ALL_YEARS ? ALL_YEARS : Number(e.target.value))}
            >
              <option value={ALL_YEARS}>All years</option>
              {years.map(y => (
                <option key={y} value={y}>{y}</option>
              ))}
            </select>
          )}
          <Button size="sm" variant="outline" onClick={() => exportMutation.mutate()} disabled={exportMutation.isPending}>
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </Button>
          <Button size="sm" variant="outline" onClick={() => fileInputRef.current?.click()} disabled={importMutation.isPending}>
            {importMutation.isPending ? 'Importing…' : 'Import CSV'}
          </Button>
          <input ref={fileInputRef} type="file" accept=".csv" className="hidden" onChange={handleFileChange} />
          <Button size="sm" onClick={openAdd}>Add Item</Button>
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

      {filtered.length === 0 ? (
        <p className="text-muted-foreground">
          {items.length === 0
            ? 'No inventory yet. Add seeds or plants you\'ve purchased.'
            : `No inventory for ${selectedYear}.`}
        </p>
      ) : (
        <div className="space-y-6">
          {Object.entries(grouped).sort(([a], [b]) => a.localeCompare(b)).map(([plantType, varieties]) => (
            <div key={plantType}>
              <h2 className="text-base font-semibold mb-2">{plantType}</h2>
              <div className="space-y-3">
                {Object.entries(varieties).sort(([a], [b]) => a.localeCompare(b)).map(([variety, purchases]) => (
                  <Card key={variety}>
                    <CardContent className="pt-4 space-y-2">
                      <p className="font-medium text-sm">{variety}</p>
                      <div className="divide-y divide-border">
                        {purchases.map((item) => (
                          <InventoryRow key={item.id} item={item} onEdit={openEdit} />
                        ))}
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      <InventoryFormDialog
        open={formOpen}
        onClose={() => setFormOpen(false)}
        editing={editing}
      />
    </div>
  )
}

function InventoryRow({ item, onEdit }: { item: InventoryItem; onEdit: (item: InventoryItem) => void }) {
  const qc = useQueryClient()
  const [adjusting, setAdjusting] = useState(false)
  const [newRemaining, setNewRemaining] = useState(String(item.quantityRemaining))

  const deleteMutation = useMutation({
    mutationFn: () => deleteInventoryItem(item.id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['inventory'] }),
  })

  const adjustMutation = useMutation({
    mutationFn: () => adjustInventoryRemaining(item.id, Number(newRemaining)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['inventory'] })
      setAdjusting(false)
    },
  })

  const pct = item.quantityPurchased > 0 ? item.quantityRemaining / item.quantityPurchased : 0
  const remainingColor = pct > 0.5 ? 'text-green-600' : pct > 0.2 ? 'text-yellow-600' : 'text-red-600'

  return (
    <div className="py-3 flex items-start justify-between gap-3">
      <div className="flex-1 space-y-1">
        <div className="flex items-center gap-2 flex-wrap">
          <Badge variant="secondary">{item.type}</Badge>
          {item.supplierName && (
            <span className="text-xs text-muted-foreground">{item.supplierName}</span>
          )}
          <span className="text-xs text-muted-foreground">{item.purchaseDate}</span>
          <span className="text-xs text-muted-foreground">${item.totalCost.toFixed(2)}</span>
        </div>

        {adjusting ? (
          <div className="flex items-center gap-2 mt-1">
            <input
              type="number"
              min={0}
              className="w-20 rounded border border-input bg-background px-2 py-1 text-sm"
              value={newRemaining}
              onChange={(e) => setNewRemaining(e.target.value)}
            />
            <span className="text-xs text-muted-foreground">/ {item.quantityPurchased}</span>
            <Button
              size="sm"
              variant="outline"
              onClick={() => adjustMutation.mutate()}
              disabled={adjustMutation.isPending}
            >
              Save
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setAdjusting(false)}>Cancel</Button>
          </div>
        ) : (
          <div className="flex items-center gap-1.5">
            <span className={`text-sm font-medium ${remainingColor}`}>
              {item.quantityRemaining}
            </span>
            <span className="text-xs text-muted-foreground">/ {item.quantityPurchased} remaining</span>
            <button
              onClick={() => { setNewRemaining(String(item.quantityRemaining)); setAdjusting(true) }}
              className="text-xs text-muted-foreground underline-offset-2 hover:underline ml-1"
            >
              adjust
            </button>
          </div>
        )}

        {item.notes && <p className="text-xs text-muted-foreground">{item.notes}</p>}
      </div>

      <div className="flex gap-1 shrink-0">
        <Button variant="ghost" size="sm" onClick={() => onEdit(item)}>Edit</Button>
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:text-destructive"
          onClick={() => {
            if (confirm('Delete this inventory item?')) deleteMutation.mutate()
          }}
          disabled={deleteMutation.isPending}
        >
          Delete
        </Button>
      </div>
    </div>
  )
}
