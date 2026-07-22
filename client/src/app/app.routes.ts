
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
        redirectTo: 'overview',
        pathMatch: 'full'
      },
      {
        path: 'overview',
        loadComponent: () => import('./dashboards/admin/admin-overview.component/admin-overview.component').then(m => m.AdminOverviewComponent)
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
        path: 'forms/:id/analytics',
        loadComponent: () => import('./dashboards/admin/form-management/form-analytics.component/form-analytics.component').then(m => m.FormAnalyticsComponent)
      },
      {
        path: 'forms/:formId/preview/:versionNumber',
        loadComponent: () => import('./dashboards/admin/form-management/admin-preview.component/admin-preview.component').then(m => m.AdminPreviewComponent)
      },
      {
        path: 'forms/:id/versions/:version/builder',
        loadComponent: () => import('./dashboards/admin/form-management/form-builder.component/form-builder.component').then(m => m.FormBuilderComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./dashboards/admin/user-management/users-list.component/users-list.component').then(m => m.UsersListComponent)
      },
      {
        path: 'api-keys',
        loadComponent: () => import('./dashboards/admin/api-keys/api-keys.component').then(m => m.ApiKeysComponent)
      },
      {
        path: 'billing',
        loadComponent: () => import('./dashboards/admin/billing.component/billing.component').then(m => m.BillingComponent)
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
    path: 'superadmin',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: [UserRole.SuperAdmin] },
    loadComponent: () => import('./dashboards/super-admin/super-admin.component').then(m => m.SuperAdminComponent)
  },
  {
    path: 'register/:tenantSlug',
    loadComponent: () => import('./core/components/register.component/register.component').then(m => m.RegisterComponent)
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

