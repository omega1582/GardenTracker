import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from '@/lib/queryClient'
import { AuthProvider } from '@/features/auth/AuthContext'
import ProtectedRoute from '@/components/layout/ProtectedRoute'
import AppShell from '@/components/layout/AppShell'
import LoginPage from '@/features/auth/LoginPage'
import RegisterPage from '@/features/auth/RegisterPage'
import DashboardPage from '@/pages/DashboardPage'
import GardensPage from '@/features/gardens/GardensPage'
import GardenDetailPage from '@/features/gardens/GardenDetailPage'
import PlantsPage from '@/features/plants/PlantsPage'
import InventoryPage from '@/features/inventory/InventoryPage'
import WaterBillsPage from '@/features/waterBills/WaterBillsPage'
import './index.css'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Protected routes */}
            <Route element={<ProtectedRoute />}>
              <Route element={<AppShell />}>
                <Route index element={<DashboardPage />} />
                <Route path="gardens" element={<GardensPage />} />
                <Route path="gardens/:gardenId" element={<GardenDetailPage />} />
                <Route path="plants" element={<PlantsPage />} />
                <Route path="inventory" element={<InventoryPage />} />
                <Route path="water-bills" element={<WaterBillsPage />} />
                <Route path="reports" element={<div>Reports (coming soon)</div>} />
              </Route>
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  </StrictMode>,
)
