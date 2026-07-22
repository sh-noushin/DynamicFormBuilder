import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { environment } from '../../../../environments/environment';

// Shape returned by GET /api/billing/plan. Not in the generated client
// yet — this endpoint is new. Keeping it inline avoids a regen.
interface BillingStatus {
  plan: 'Free' | 'Pro';
  status?: string;
  currentPeriodEnd?: string;
  formCount: number;
  maxForms: number;
  submissionsThisMonth: number;
  maxSubmissionsPerMonth: number;
}

@Component({
  selector: 'app-billing',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatSnackBarModule],
  templateUrl: './billing.component.html',
  styleUrl: './billing.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class BillingComponent implements OnInit {
  status = signal<BillingStatus | null>(null);
  isLoading = signal(false);
  isRedirecting = signal(false);

  // Convert usage counts into 0–100 percentages for the progress bars.
  // Clamped so a tenant briefly over their cap (edge cases like a race
  // on Free) doesn't blow past 100 and confuse the bar.
  formsPercent = computed(() => {
    const s = this.status();
    if (!s || !s.maxForms) return 0;
    return Math.min(100, Math.round((s.formCount / s.maxForms) * 100));
  });
  submissionsPercent = computed(() => {
    const s = this.status();
    if (!s || !s.maxSubmissionsPerMonth) return 0;
    return Math.min(100, Math.round((s.submissionsThisMonth / s.maxSubmissionsPerMonth) * 100));
  });

  private http = inject(HttpClient);
  private snack = inject(MatSnackBar);

  private get authHeaders(): HttpHeaders {
    const token = typeof localStorage !== 'undefined' ? localStorage.getItem('auth_token') : null;
    return token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : new HttpHeaders();
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.http.get<BillingStatus>(`${environment.apiBaseUrl}/api/billing/plan`, { headers: this.authHeaders })
      .subscribe({
        next: (s) => {
          this.status.set(s);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.snack.open('Failed to load billing status', 'Close', { duration: 3000 });
        },
      });
  }

  upgrade(): void {
    this.isRedirecting.set(true);
    this.http.post<{ url: string }>(`${environment.apiBaseUrl}/api/billing/checkout`, {}, { headers: this.authHeaders })
      .subscribe({
        next: (r) => {
          if (r?.url) {
            window.location.href = r.url;
          } else {
            this.isRedirecting.set(false);
            this.snack.open('Checkout URL missing from response', 'Close', { duration: 3000 });
          }
        },
        error: () => {
          this.isRedirecting.set(false);
          this.snack.open('Failed to start checkout. Check Stripe config.', 'Close', { duration: 4000 });
        },
      });
  }

  managePortal(): void {
    this.isRedirecting.set(true);
    this.http.post<{ url: string }>(`${environment.apiBaseUrl}/api/billing/portal`, {}, { headers: this.authHeaders })
      .subscribe({
        next: (r) => {
          if (r?.url) window.location.href = r.url;
          else {
            this.isRedirecting.set(false);
            this.snack.open('Portal URL missing', 'Close', { duration: 3000 });
          }
        },
        error: () => {
          this.isRedirecting.set(false);
          this.snack.open('Failed to open billing portal', 'Close', { duration: 3000 });
        },
      });
  }
}
