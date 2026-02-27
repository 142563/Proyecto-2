import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login.page').then((m) => m.LoginPage)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.page').then((m) => m.DashboardPage)
  },
  {
    path: 'transfers',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Student'] },
    loadComponent: () => import('./features/transfers/transfers.page').then((m) => m.TransfersPage)
  },
  {
    path: 'courses',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Student'] },
    loadComponent: () => import('./features/courses/courses.page').then((m) => m.CoursesPage)
  },
  {
    path: 'payments',
    canActivate: [authGuard],
    loadComponent: () => import('./features/payments/payments.page').then((m) => m.PaymentsPage)
  },
  {
    path: 'certificates',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Student'] },
    loadComponent: () => import('./features/certificates/certificates.page').then((m) => m.CertificatesPage)
  },
  {
    path: 'admin-reports',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    loadComponent: () => import('./features/admin-reports/admin-reports.page').then((m) => m.AdminReportsPage)
  },
  { path: '**', redirectTo: 'dashboard' }
];
