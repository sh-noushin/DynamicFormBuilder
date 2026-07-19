
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
    path: 'f/:slug',
    loadComponent: () => import('./public-form/public-form.component').then(m => m.PublicFormComponent)
  },
  {
    path: 'admin',
    loadComponent: () => import('./dashboards/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent),
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
        loadComponent: () => import('./dashboards/admin/form-management/forms-list.component/forms-list.component').then(m => m.FormsListComponent)
      },
      {
        path: 'forms/:id/submissions',
        loadComponent: () => import('./dashboards/admin/form-management/admin-submissions.component/admin-submissions.component').then(m => m.AdminSubmissionsComponent)
      },
      {
        path: 'forms/:formId/preview/:versionNumber',
        loadComponent: () => import('./dashboards/admin/form-management/admin-preview.component/admin-preview.component').then(m => m.AdminPreviewComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./dashboards/admin/user-management/users-list.component/users-list.component').then(m => m.UsersListComponent)
      }
    ]
  },
  {
    path: 'user',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: [UserRole.User] },
    loadComponent: () => import('./dashboards/user/user-dashboard.component/user-dashboard.component').then(m => m.UserDashboardComponent)
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

