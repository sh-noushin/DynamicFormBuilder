import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/services/auth.service';
import { DeleteDialogComponent, DeleteDialogData } from '../../shared/delete-dialog.component/delete-dialog.component';
import { HeaderComponent } from '../../shared/layout/header.component/header.component';
import { CreateTenantDialogComponent } from './create-tenant-dialog.component/create-tenant-dialog.component';
import { RenameTenantDialogComponent } from './rename-tenant-dialog.component/rename-tenant-dialog.component';

// Tenant shape returned by the backend. Kept as an interface here rather
// than an imported type because the generated API client hasn't been
// regenerated to include this endpoint yet.
interface TenantRow {
  id: string;
  name: string;
  slug: string;
  createdAt: string;
  userCount: number;
  formCount: number;
  // Billing snapshot. Read-only here — tenants change their own plan
  // through /admin/billing (Stripe Checkout / customer portal).
  plan: 'Free' | 'Pro';
  subscriptionStatus: string | null;
  subscriptionCurrentPeriodEnd: string | null;
  monthlyPriceUsd: number;
}

@Component({
  selector: 'app-super-admin',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatPaginatorModule,
    MatDialogModule,
    MatSnackBarModule,
    MatToolbarModule,
    MatTooltipModule,
    HeaderComponent,
  ],
  templateUrl: './super-admin.component.html',
  styleUrl: './super-admin.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class SuperAdminComponent implements OnInit {
  tenants = signal<TenantRow[]>([]);
  isLoading = signal(false);
  displayedColumns = ['name', 'slug', 'plan', 'price', 'users', 'forms', 'createdAt', 'actions'];

  pageIndex = signal(0);
  pageSize = signal(10);
  pagedTenants = computed<TenantRow[]>(() => {
    const start = this.pageIndex() * this.pageSize();
    return this.tenants().slice(start, start + this.pageSize());
  });

  // Monthly recurring revenue across all tenants, at list price. Only
  // subscriptions Stripe reports as active or trialing count — a
  // past_due or canceled Pro tenant isn't revenue.
  mrr = computed(() =>
    this.tenants().reduce(
      (sum, t) => sum + (this.isBillable(t) ? (t.monthlyPriceUsd ?? 0) : 0),
      0
    )
  );
  payingCount = computed(() => this.tenants().filter((t) => this.isBillable(t)).length);

  isBillable(t: TenantRow): boolean {
    return t.plan !== 'Free'
      && (t.subscriptionStatus === 'active' || t.subscriptionStatus === 'trialing');
  }

  // Anything Stripe flags as not-good-standing gets a warning chip so a
  // failed payment is visible without opening the Stripe dashboard.
  isDelinquent(t: TenantRow): boolean {
    return t.plan !== 'Free' && !!t.subscriptionStatus && !this.isBillable(t);
  }

  private http = inject(HttpClient);
  private router = inject(Router);
  private snack = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private auth = inject(AuthService);

  private get authHeaders(): HttpHeaders {
    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    return token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.http.get<TenantRow[]>(`${environment.apiBaseUrl}/api/organizations`, { headers: this.authHeaders })
      .subscribe({
        next: (rows) => {
          this.tenants.set(rows ?? []);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.snack.open('Failed to load tenants', 'Close', { duration: 3000 });
        },
      });
  }

  openCreate(): void {
    const ref = this.dialog.open(CreateTenantDialogComponent, {
      width: '520px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true,
    });
    ref.afterClosed().subscribe((payload) => {
      if (!payload) return;
      this.http.post<TenantRow>(`${environment.apiBaseUrl}/api/organizations`, payload, { headers: this.authHeaders })
        .subscribe({
          next: () => {
            this.snack.open('Tenant created', 'Close', { duration: 2500 });
            this.load();
          },
          error: (err) => {
            const msg = err?.error?.message ?? 'Failed to create tenant';
            this.snack.open(msg, 'Close', { duration: 4000 });
          },
        });
    });
  }

  openRename(row: TenantRow): void {
    const ref = this.dialog.open(RenameTenantDialogComponent, {
      width: '480px',
      panelClass: 'elevated-dialog-panel',
      disableClose: true,
      data: { name: row.name },
    });
    ref.afterClosed().subscribe((newName?: string) => {
      if (!newName || newName === row.name) return;
      this.http.put<TenantRow>(`${environment.apiBaseUrl}/api/organizations/${row.id}`, { name: newName }, { headers: this.authHeaders })
        .subscribe({
          next: () => {
            this.snack.open('Tenant renamed', 'Close', { duration: 2500 });
            this.load();
          },
          error: () => this.snack.open('Failed to rename tenant', 'Close', { duration: 3000 }),
        });
    });
  }

  confirmDelete(row: TenantRow): void {
    const ref = this.dialog.open(DeleteDialogComponent, {
      width: '440px',
      data: { itemType: 'tenant', itemName: row.name } as DeleteDialogData,
      disableClose: true,
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.http.delete(`${environment.apiBaseUrl}/api/organizations/${row.id}`, { headers: this.authHeaders })
        .subscribe({
          next: () => {
            this.snack.open('Tenant deleted', 'Close', { duration: 2500 });
            this.load();
          },
          error: () => this.snack.open('Failed to delete tenant', 'Close', { duration: 3000 }),
        });
    });
  }

  impersonate(row: TenantRow): void {
    this.http.post<{ token: string; organizationName: string; organizationId: string }>(
      `${environment.apiBaseUrl}/api/organizations/${row.id}/impersonate`,
      {},
      { headers: this.authHeaders }
    ).subscribe({
      next: (result) => {
        // Save the original super-admin token under a separate key so
        // the app has a "return to super admin" affordance later. Then
        // swap the active token with the impersonation JWT and jump
        // straight into the tenant's admin dashboard.
        if (typeof localStorage !== 'undefined') {
          const currentToken = localStorage.getItem('auth_token');
          if (currentToken) localStorage.setItem('super_token', currentToken);
          localStorage.setItem('auth_token', result.token);
        }
        this.auth.setUser({
          username: `super@${row.slug}`,
          email: '',
          roles: ['Admin'],
          organizationName: `[Impersonating] ${result.organizationName}`,
        });
        this.snack.open(`Now viewing as admin of ${result.organizationName}`, 'Close', { duration: 3000 });
        this.router.navigate(['/admin']);
      },
      error: () => this.snack.open('Failed to impersonate tenant', 'Close', { duration: 3000 }),
    });
  }

  copySignupUrl(row: TenantRow): void {
    const url = `${window.location.origin}/register/${row.slug}`;
    navigator.clipboard.writeText(url).then(
      () => this.snack.open('Sign-up URL copied', 'Close', { duration: 2000 }),
      () => this.snack.open('Failed to copy URL', 'Close', { duration: 2000 })
    );
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
  }
}
