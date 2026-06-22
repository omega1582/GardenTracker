import { useState } from 'react'
import { Link, Outlet, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getPlantTypes } from '@/api/plants'
import { getGardens } from '@/api/gardens'
import { getInventory } from '@/api/inventory'
import { getWaterBills } from '@/api/waterBills'
import { useAuth } from '@/features/auth/AuthContext'
import { toggleTheme, getCurrentTheme } from '@/lib/theme'
import { LayoutDashboard, Trees, Leaf, Package, Droplets, LineChart, LogOut, Sun, Moon, ChevronDown, ChevronRight } from 'lucide-react'
import { cn } from '@/lib/utils'

const CATEGORIES = ['Vegetables', 'Fruits', 'Herbs', 'Flowers', 'Ornamentals', 'Other']

const NAV_LINKS = [
  { to: '/',            label: 'Dashboard', icon: LayoutDashboard },
  { to: '/gardens',     label: 'Gardens',   icon: Trees },
  { to: '/plants',      label: 'Plants',    icon: Leaf },
  { to: '/inventory',   label: 'Inventory', icon: Package },
  { to: '/water-bills', label: 'Water',     icon: Droplets },
  { to: '/reports',     label: 'Reports',   icon: LineChart },
]

export default function AppShell() {
  const { signOut } = useAuth()
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const [searchParams] = useSearchParams()
  const [theme, setTheme] = useState(getCurrentTheme)
  
  const [expandedNav, setExpandedNav] = useState<Record<string, boolean>>({})
  
  const toggleNav = (path: string) => {
    setExpandedNav(prev => ({ ...prev, [path]: !prev[path] }))
  }

  const [expandedCategories, setExpandedCategories] = useState<string[]>([])
  const [expandedReportGardens, setExpandedReportGardens] = useState<number[]>([])

  const { data: plantTypes = [] } = useQuery({ queryKey: ['plant-types'], queryFn: getPlantTypes })
  const { data: gardens = [] } = useQuery({ queryKey: ['gardens'], queryFn: getGardens })
  const { data: inventory = [] } = useQuery({ queryKey: ['inventory'], queryFn: getInventory })
  const { data: waterBills = [] } = useQuery({ queryKey: ['water-bills'], queryFn: () => getWaterBills() })

  // Derived Data
  const typesByCategory = CATEGORIES.reduce((acc, cat) => {
    acc[cat] = plantTypes.filter(t => t.category === cat)
    return acc
  }, {} as Record<string, typeof plantTypes>)
  typesByCategory['Other'] = [
    ...(typesByCategory['Other'] || []),
    ...plantTypes.filter(t => !CATEGORIES.includes(t.category))
  ]

  const inventoryYears = Array.from(new Set(inventory.map(i => Number(i.purchaseDate.slice(0, 4))))).sort((a, b) => b - a)
  const waterBillYears = Array.from(new Set(waterBills.map(b => b.year))).sort((a, b) => b - a)

  const activeCategory = searchParams.get('category')
  const activeTypeId = Number(searchParams.get('typeId'))
  const activeYear = searchParams.get('year')
  const activeGardenId = Number(searchParams.get('gardenId')) || null

  function handleSignOut() {
    signOut()
    navigate('/login')
  }

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      {/* Global Left Sidebar */}
      <aside className="w-60 flex-shrink-0 flex flex-col bg-slate-900 text-slate-300 dark:bg-slate-950 border-r border-slate-800/50 shadow-xl z-50">
        
        {/* Brand */}
        <div className="h-16 flex items-center px-6 shrink-0 border-b border-slate-800/50">
          <Link to="/" className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-emerald-500/20 flex items-center justify-center">
              <Leaf className="w-5 h-5 text-emerald-400" />
            </div>
            <span className="text-lg font-semibold tracking-tight text-white">
              Garden Tracker
            </span>
          </Link>
        </div>

        {/* Main Navigation */}
        <nav className="flex-1 overflow-y-auto px-4 py-6 space-y-1">
          <div className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-4 px-2">
            Main Menu
          </div>
          {NAV_LINKS.map(({ to, label, icon: Icon }) => {
            const hasSubmenu = to !== '/'
            const isExpanded = expandedNav[to]
            const active = pathname === to || (to !== '/' && pathname.startsWith(to))
            
            return (
              <div key={to} className="space-y-1">
                <div className="flex items-center">
                  <Link
                    to={to}
                    className={cn(
                      "flex-1 flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200",
                      active && (!hasSubmenu || pathname === to) && !activeCategory && !activeTypeId && !activeYear && !activeGardenId
                        ? "bg-emerald-500/10 text-emerald-400"
                        : "text-slate-400 hover:text-white hover:bg-slate-800/50"
                    )}
                  >
                    <Icon className={cn("w-5 h-5", active ? "text-emerald-400" : "text-slate-500")} />
                    {label}
                  </Link>
                  {hasSubmenu && (
                    <button
                      onClick={(e) => { e.preventDefault(); toggleNav(to) }}
                      className={cn(
                        "p-2 rounded-lg ml-1 transition-colors",
                        active ? "text-emerald-400 hover:bg-emerald-500/10" : "text-slate-400 hover:bg-slate-800/50 hover:text-white"
                      )}
                    >
                      {isExpanded ? <ChevronDown className="w-4 h-4" /> : <ChevronRight className="w-4 h-4" />}
                    </button>
                  )}
                </div>

                {hasSubmenu && isExpanded && (
                  <div className="pl-9 pr-2 py-1 space-y-1">
                    {/* Gardens Hierarchy */}
                    {to === '/gardens' && gardens.map(g => (
                      <Link
                        key={g.id}
                        to={`/gardens/${g.id}`}
                        className={cn(
                          "block px-3 py-1.5 text-sm rounded-lg transition-colors truncate",
                          pathname === `/gardens/${g.id}`
                            ? "bg-emerald-500/10 text-emerald-400 font-medium"
                            : "text-slate-500 hover:text-slate-300 hover:bg-slate-800/30"
                        )}
                      >
                        {g.name}
                      </Link>
                    ))}

                    {/* Inventory Hierarchy */}
                    {to === '/inventory' && (
                      <>
                        <Link
                          to="/inventory?year=all"
                          className={cn(
                            "block px-3 py-1.5 text-sm rounded-lg transition-colors truncate",
                            (activeYear === 'all' || (!activeYear && pathname === '/inventory'))
                              ? "bg-emerald-500/10 text-emerald-400 font-medium"
                              : "text-slate-500 hover:text-slate-300 hover:bg-slate-800/30"
                          )}
                        >
                          All Years
                        </Link>
                        {inventoryYears.map(y => (
                          <Link
                            key={y}
                            to={`/inventory?year=${y}`}
                            className={cn(
                              "block px-3 py-1.5 text-sm rounded-lg transition-colors truncate",
                              activeYear === String(y) && pathname === '/inventory'
                                ? "bg-emerald-500/10 text-emerald-400 font-medium"
                                : "text-slate-500 hover:text-slate-300 hover:bg-slate-800/30"
                            )}
                          >
                            {y}
                          </Link>
                        ))}
                      </>
                    )}

                    {/* Water Bills Hierarchy */}
                    {to === '/water-bills' && waterBillYears.map(y => (
                      <Link
                        key={y}
                        to={`/water-bills?year=${y}`}
                        className={cn(
                          "block px-3 py-1.5 text-sm rounded-lg transition-colors truncate",
                          (activeYear === String(y) || (!activeYear && pathname === '/water-bills' && y === waterBillYears[0]))
                            ? "bg-emerald-500/10 text-emerald-400 font-medium"
                            : "text-slate-500 hover:text-slate-300 hover:bg-slate-800/30"
                        )}
                      >
                        {y}
                      </Link>
                    ))}

                    {/* Reports Hierarchy */}
                    {to === '/reports' && gardens.map(g => {
                      const isGardenExpanded = expandedReportGardens.includes(g.id)
                      const isGardenActive = activeGardenId === g.id && pathname === '/reports'
                      return (
                        <div key={g.id} className="space-y-1">
                          <div className="flex items-center">
                            <button
                              onClick={() => setExpandedReportGardens(prev => prev.includes(g.id) ? prev.filter(id => id !== g.id) : [...prev, g.id])}
                              className="p-1 rounded text-slate-500 hover:text-slate-300"
                            >
                              {isGardenExpanded ? <ChevronDown className="w-3.5 h-3.5" /> : <ChevronRight className="w-3.5 h-3.5" />}
                            </button>
                            <Link
                              to={`/reports?gardenId=${g.id}`}
                              className={cn(
                                "flex-1 px-2 py-1.5 text-sm rounded-lg transition-colors truncate",
                                isGardenActive && !activeYear
                                  ? "bg-emerald-500/10 text-emerald-400 font-medium"
                                  : "text-slate-500 hover:text-slate-300 hover:bg-slate-800/30"
                              )}
                            >
                              {g.name}
                            </Link>
                          </div>
                          {isGardenExpanded && (
                            <div className="pl-6 space-y-0.5 pb-1">
                              {waterBillYears.map(y => (
                                <Link
                                  key={y}
                                  to={`/reports?gardenId=${g.id}&year=${y}`}
                                  className={cn(
                                    "block px-3 py-1.5 text-sm rounded-lg transition-colors truncate",
                                    isGardenActive && activeYear === String(y)
                                      ? "bg-emerald-500 text-white font-medium shadow-sm"
                                      : "text-slate-500 hover:text-slate-300 hover:bg-slate-800/30"
                                  )}
                                >
                                  {y}
                                </Link>
                              ))}
                            </div>
                          )}
                        </div>
                      )
                    })}

                    {/* Plants Hierarchy */}
                    {to === '/plants' && (
                      <>
                        <Link
                          to="/plants"
                          className={cn(
                            "block px-3 py-1.5 text-sm font-medium rounded-lg transition-colors",
                            pathname === '/plants' && !activeCategory && !activeTypeId
                              ? "bg-emerald-500/10 text-emerald-400"
                              : "text-slate-400 hover:text-white hover:bg-slate-800/50"
                          )}
                        >
                          All Plants
                        </Link>

                    {CATEGORIES.map(cat => {
                      const types = typesByCategory[cat]
                      if (!types || types.length === 0) return null
                      
                      const isCatExpanded = expandedCategories.includes(cat)
                      const isCatActive = activeCategory === cat && !activeTypeId
                      
                      return (
                        <div key={cat} className="space-y-1">
                          <div className="flex items-center">
                            <button
                              onClick={() => setExpandedCategories(prev => prev.includes(cat) ? prev.filter(c => c !== cat) : [...prev, cat])}
                              className="p-1 rounded text-slate-500 hover:text-slate-300"
                            >
                              {isCatExpanded ? <ChevronDown className="w-3.5 h-3.5" /> : <ChevronRight className="w-3.5 h-3.5" />}
                            </button>
                            <Link
                              to={`/plants?category=${cat}`}
                              className={cn(
                                "flex-1 px-2 py-1.5 text-sm font-medium rounded-lg transition-colors",
                                isCatActive
                                  ? "bg-emerald-500/10 text-emerald-400"
                                  : "text-slate-400 hover:text-white hover:bg-slate-800/50"
                              )}
                            >
                              {cat}
                            </Link>
                          </div>
                          
                          {isCatExpanded && (
                            <div className="pl-6 space-y-0.5 pb-1">
                              {types.map(t => {
                                const isTypeActive = activeTypeId === t.id
                                return (
                                  <Link
                                    key={t.id}
                                    to={`/plants?typeId=${t.id}`}
                                    className={cn(
                                      "block px-3 py-1.5 text-sm rounded-lg transition-colors truncate",
                                      isTypeActive
                                        ? "bg-emerald-500 text-white font-medium shadow-sm"
                                        : "text-slate-500 hover:text-slate-300 hover:bg-slate-800/30"
                                    )}
                                  >
                                    {t.name}
                                  </Link>
                                )
                              })}
                            </div>
                          )}
                        </div>
                      )
                    })}
                      </>
                    )}
                  </div>
                )}
              </div>
            )
          })}
        </nav>

        {/* Bottom Actions */}
        <div className="p-4 border-t border-slate-800/50 space-y-2 shrink-0">
          <button
            onClick={() => setTheme(toggleTheme())}
            className="w-full flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium text-slate-400 hover:text-white hover:bg-slate-800/50 transition-colors"
          >
            {theme === 'dark' ? <Sun className="w-5 h-5 text-slate-500" /> : <Moon className="w-5 h-5 text-slate-500" />}
            {theme === 'dark' ? 'Light Mode' : 'Dark Mode'}
          </button>
          
          <button
            onClick={handleSignOut}
            className="w-full flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition-colors"
          >
            <LogOut className="w-5 h-5 text-slate-500 group-hover:text-rose-400" />
            Sign Out
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="flex-1 flex flex-col overflow-hidden relative">
        <Outlet />
      </main>
    </div>
  )
}
