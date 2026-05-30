import { Routes } from '@angular/router';

export const techniciansRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./technician-list/technician-list.component').then(m => m.TechnicianListComponent)
  }
];
