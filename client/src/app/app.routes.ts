
import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';
import { UserRole } from './core/services/api-service';
import { LoginComponent } from './core/components/login.component/login.component';
import { ContentComponent } from './shared/layout/content.component/content.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
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
    component: ContentComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: [UserRole.User] },
    children: [
      {
        path: '',
        component: ContentComponent
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

