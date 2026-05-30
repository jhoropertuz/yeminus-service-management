import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./shared/components/layout/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'orders', pathMatch: 'full' },
      {
        path: 'orders',
        loadChildren: () => import('./features/orders/orders.routes').then(m => m.ordersRoutes)
      },
      {
        path: 'clients',
        loadChildren: () => import('./features/clients/clients.routes').then(m => m.clientsRoutes)
      },
      {
        path: 'technicians',
        loadChildren: () => import('./features/technicians/technicians.routes').then(m => m.techniciansRoutes)
      }
    ]
  },
  { path: '**', redirectTo: 'orders' }
];
