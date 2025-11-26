import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';
import { UserRole } from './core/services/api-service';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./shared/layout/content.component/content.component').then(m => m.ContentComponent)
  },
  {
    path: 'admin',
    loadComponent: () => import('./dashbosrds/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: [UserRole.Admin] },
    children: [
      {
        path: '',
        redirectTo: 'forms',
        pathMatch: 'full'
      },
      {
        path: 'forms',
        loadComponent: () => import('./dashbosrds/admin/form-management/forms-list.component/forms-list.component').then(m => m.FormsListComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./dashbosrds/admin/user-management/users-list.component/users-list.component').then(m => m.UsersListComponent)
      }
    ]
  },
  {
    path: 'user',
    loadComponent: () => import('./shared/layout/content.component/content.component').then(m => m.ContentComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: [UserRole.User] },
    children: [
      {
        path: '',
        loadComponent: () => import('./shared/layout/content.component/content.component').then(m => m.ContentComponent)
      }
    ]
  },
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];

