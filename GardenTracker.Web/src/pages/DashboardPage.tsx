import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getGardens } from '@/api/gardens'
import { getPlantTypes, getAllVarieties } from '@/api/plants'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Trees, Leaf, Package, ChevronRight } from 'lucide-react'

export default function DashboardPage() {
  const { data: gardens = [] } = useQuery({ queryKey: ['gardens'], queryFn: getGardens })
  const { data: plantTypes = [] } = useQuery({ queryKey: ['plant-types'], queryFn: getPlantTypes })
  const { data: varieties = [] } = useQuery({ queryKey: ['varieties'], queryFn: getAllVarieties })

  const stats = [
    { 
      title: 'Active Gardens', 
      value: gardens.length, 
      icon: Trees,
      color: 'text-emerald-500',
      bg: 'bg-emerald-500/10'
    },
    { 
      title: 'Plant Types', 
      value: plantTypes.length, 
      icon: Leaf,
      color: 'text-amber-500',
      bg: 'bg-amber-500/10'
    },
    { 
      title: 'Varieties', 
      value: varieties.length, 
      icon: Package,
      color: 'text-blue-500',
      bg: 'bg-blue-500/10'
    },
  ]

  return (
    <div className="flex flex-col h-full p-6 lg:p-8 overflow-y-auto">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight text-foreground">Welcome back</h1>
        <p className="mt-1.5 text-muted-foreground">Here is an overview of your garden's status.</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        {stats.map((stat) => (
          <Card key={stat.title} className="border-border shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {stat.title}
              </CardTitle>
              <div className={`w-8 h-8 rounded-md flex items-center justify-center ${stat.bg}`}>
                <stat.icon className={`w-4 h-4 ${stat.color}`} />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">{stat.value}</div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Quick Actions & Recent */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        
        {/* Quick Actions */}
        <Card className="border-border shadow-sm">
          <CardHeader>
            <CardTitle className="text-lg font-semibold">Quick Actions</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4">
            <Link 
              to="/gardens" 
              className="flex items-center justify-between p-4 rounded-lg border hover:bg-muted/50 transition-colors group"
            >
              <div className="flex items-center gap-4">
                <div className="w-10 h-10 rounded-full bg-emerald-500/10 flex items-center justify-center">
                  <Trees className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                </div>
                <div>
                  <h4 className="font-medium">Manage Gardens</h4>
                  <p className="text-sm text-muted-foreground">Plan and track your active garden beds</p>
                </div>
              </div>
              <ChevronRight className="w-5 h-5 text-muted-foreground group-hover:text-foreground transition-colors" />
            </Link>
            
            <Link 
              to="/plants" 
              className="flex items-center justify-between p-4 rounded-lg border hover:bg-muted/50 transition-colors group"
            >
              <div className="flex items-center gap-4">
                <div className="w-10 h-10 rounded-full bg-amber-500/10 flex items-center justify-center">
                  <Leaf className="w-5 h-5 text-amber-600 dark:text-amber-400" />
                </div>
                <div>
                  <h4 className="font-medium">Plant Catalog</h4>
                  <p className="text-sm text-muted-foreground">Browse and add new plant varieties</p>
                </div>
              </div>
              <ChevronRight className="w-5 h-5 text-muted-foreground group-hover:text-foreground transition-colors" />
            </Link>
          </CardContent>
        </Card>

      </div>
    </div>
  )
}
