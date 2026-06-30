import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react'
import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import Layout from './components/Layout'
import LoginPage from './pages/LoginPage'
import Dashboard from './pages/Dashboard'
import RepositoriesPage from './pages/RepositoriesPage'
import RepositoryDetailPage from './pages/RepositoryDetailPage'
import PromptsPage from './pages/PromptsPage'
import FilterGroupsPage from './pages/FilterGroupsPage'
import CategoryGroupsPage from './pages/CategoryGroupsPage'
import UsersPage from './pages/UsersPage'

const queryClient = new QueryClient()

const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      { index: true, element: <Dashboard /> },
      { path: 'repositories', element: <RepositoriesPage /> },
      { path: 'repositories/:id', element: <RepositoryDetailPage /> },
      { path: 'prompts', element: <PromptsPage /> },
      { path: 'filter-groups', element: <FilterGroupsPage /> },
      { path: 'category-groups', element: <CategoryGroupsPage /> },
      { path: 'admin/users', element: <UsersPage /> },
      { path: '*', element: <Navigate to="/" replace /> },
    ],
  },
])

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <UnauthenticatedTemplate>
        <LoginPage />
      </UnauthenticatedTemplate>

      <AuthenticatedTemplate>
        <RouterProvider router={router} />
      </AuthenticatedTemplate>
    </QueryClientProvider>
  )
}
