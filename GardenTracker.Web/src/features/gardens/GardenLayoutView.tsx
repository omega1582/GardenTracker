import { useState, useRef, useCallback } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { updateBedPosition } from '@/api/beds'
import type { Bed } from '@/types/bed'

const CANVAS_W = 800
const CANVAS_H = 600
const MIN_TILE_W = 80
const MIN_TILE_H = 60

function bedTileSize(bed: Bed) {
  // Scale feet to pixels: 1 ft ≈ 20px, capped at canvas bounds
  const w = Math.max(MIN_TILE_W, Math.min(bed.widthFt * 20, CANVAS_W / 2))
  const h = Math.max(MIN_TILE_H, Math.min(bed.lengthFt * 20, CANVAS_H / 2))
  return { w, h }
}

interface DragState {
  bedId: number
  startMouseX: number
  startMouseY: number
  startTileX: number
  startTileY: number
}

export default function GardenLayoutView({
  gardenId,
  beds,
}: {
  gardenId: number
  beds: Bed[]
}) {
  const qc = useQueryClient()
  const canvasRef = useRef<HTMLDivElement>(null)
  const [dragging, setDragging] = useState<DragState | null>(null)
  // Local optimistic positions while dragging
  const [localPositions, setLocalPositions] = useState<Record<number, { x: number; y: number }>>({})

  const positionMutation = useMutation({
    mutationFn: ({ bedId, x, y }: { bedId: number; x: number; y: number }) =>
      updateBedPosition(gardenId, bedId, x, y),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['beds', gardenId] }),
  })

  const positioned = beds.filter(b => b.positionX !== null && b.positionY !== null)
  const unpositioned = beds.filter(b => b.positionX === null || b.positionY === null)

  function getTilePos(bed: Bed): { x: number; y: number } {
    if (localPositions[bed.id]) return localPositions[bed.id]
    return { x: bed.positionX ?? 0, y: bed.positionY ?? 0 }
  }

  const onMouseDown = useCallback((e: React.MouseEvent, bed: Bed) => {
    e.preventDefault()
    const { x, y } = getTilePos(bed)
    setDragging({
      bedId: bed.id,
      startMouseX: e.clientX,
      startMouseY: e.clientY,
      startTileX: x,
      startTileY: y,
    })
  }, [localPositions]) // eslint-disable-line react-hooks/exhaustive-deps

  const onMouseMove = useCallback((e: React.MouseEvent) => {
    if (!dragging || !canvasRef.current) return
    const dx = e.clientX - dragging.startMouseX
    const dy = e.clientY - dragging.startMouseY
    const bed = beds.find(b => b.id === dragging.bedId)
    if (!bed) return
    const { w, h } = bedTileSize(bed)
    const newX = Math.max(0, Math.min(CANVAS_W - w, dragging.startTileX + dx))
    const newY = Math.max(0, Math.min(CANVAS_H - h, dragging.startTileY + dy))
    setLocalPositions(prev => ({ ...prev, [dragging.bedId]: { x: newX, y: newY } }))
  }, [dragging, beds])

  const onMouseUp = useCallback(() => {
    if (!dragging) return
    const pos = localPositions[dragging.bedId]
    if (pos) {
      positionMutation.mutate({ bedId: dragging.bedId, x: Math.round(pos.x), y: Math.round(pos.y) })
    }
    setDragging(null)
  }, [dragging, localPositions, positionMutation])

  function placeOnCanvas(bed: Bed) {
    // Put in top-left area, nudged so beds don't stack exactly
    const offset = positioned.length * 10
    const { w, h } = bedTileSize(bed)
    const x = Math.min(offset, CANVAS_W - w)
    const y = Math.min(offset, CANVAS_H - h)
    positionMutation.mutate({ bedId: bed.id, x, y })
  }

  function removeFromCanvas(bed: Bed) {
    setLocalPositions(prev => {
      const next = { ...prev }
      delete next[bed.id]
      return next
    })
    updateBedPosition(gardenId, bed.id, null, null)
      .then(() => qc.invalidateQueries({ queryKey: ['beds', gardenId] }))
  }

  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">
        Drag beds to arrange your garden layout. Positions are saved automatically.
      </p>

      <div className="flex gap-4 flex-wrap lg:flex-nowrap">
        {/* Canvas */}
        <div
          ref={canvasRef}
          className="relative shrink-0 rounded-lg border border-border bg-muted/20 overflow-hidden"
          style={{ width: CANVAS_W, height: CANVAS_H, maxWidth: '100%', cursor: dragging ? 'grabbing' : 'default' }}
          onMouseMove={onMouseMove}
          onMouseUp={onMouseUp}
          onMouseLeave={onMouseUp}
        >
          {/* Grid lines */}
          <svg className="absolute inset-0 pointer-events-none opacity-20" width={CANVAS_W} height={CANVAS_H}>
            {Array.from({ length: Math.floor(CANVAS_W / 40) + 1 }, (_, i) => (
              <line key={`v${i}`} x1={i * 40} y1={0} x2={i * 40} y2={CANVAS_H} stroke="currentColor" strokeWidth="1" />
            ))}
            {Array.from({ length: Math.floor(CANVAS_H / 40) + 1 }, (_, i) => (
              <line key={`h${i}`} x1={0} y1={i * 40} x2={CANVAS_W} y2={i * 40} stroke="currentColor" strokeWidth="1" />
            ))}
          </svg>

          {positioned.map(bed => {
            const { w, h } = bedTileSize(bed)
            const { x, y } = getTilePos(bed)
            const isDraggingThis = dragging?.bedId === bed.id
            return (
              <div
                key={bed.id}
                className={`absolute rounded border-2 select-none flex flex-col items-center justify-center text-center p-1
                  ${isDraggingThis
                    ? 'border-primary bg-primary/20 shadow-lg z-10'
                    : 'border-green-600 bg-green-500/15 hover:bg-green-500/25 cursor-grab'
                  }`}
                style={{ left: x, top: y, width: w, height: h }}
                onMouseDown={e => onMouseDown(e, bed)}
              >
                <p className="text-xs font-semibold leading-tight">{bed.name}</p>
                <p className="text-[10px] text-muted-foreground leading-tight">
                  {bed.lengthFt}′×{bed.widthFt}′
                </p>
                <button
                  className="absolute top-0.5 right-0.5 text-[10px] text-muted-foreground hover:text-destructive leading-none"
                  onMouseDown={e => e.stopPropagation()}
                  onClick={() => removeFromCanvas(bed)}
                  title="Remove from layout"
                >
                  ✕
                </button>
              </div>
            )
          })}
        </div>

        {/* Unpositioned sidebar */}
        {unpositioned.length > 0 && (
          <div className="space-y-2 min-w-[160px]">
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">Not placed</p>
            {unpositioned.map(bed => (
              <div
                key={bed.id}
                className="flex items-center justify-between gap-2 rounded border border-border px-3 py-2 text-sm"
              >
                <span>{bed.name}</span>
                <button
                  className="text-xs text-primary hover:underline whitespace-nowrap"
                  onClick={() => placeOnCanvas(bed)}
                >
                  Place
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {beds.length === 0 && (
        <p className="text-muted-foreground text-sm">Add beds first, then arrange them here.</p>
      )}
    </div>
  )
}
