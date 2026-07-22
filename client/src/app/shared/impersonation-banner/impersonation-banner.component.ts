import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { Client } from '../../core/services/api-service';
import { AuthService } from '../../core/services/auth.service';

// Red banner shown whenever the super admin is impersonating a tenant.
// Presence is detected by the `super_token` key in localStorage (set by
// super-admin.component.impersonate). Clicking Exit restores that token
// as the active auth_token and routes back to /superadmin.
@Component({
  selector: 'app-impersonation-banner',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './impersonation-banner.component.html',
  styleUrl: './impersonation-banner.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class ImpersonationBannerComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private api = inject(Client);

  // The header already sets user.organizationName to "[Impersonating] X"
  // during an impersonation session — reuse that as our signal instead
  // of hitting localStorage on every change detection.
  isImpersonating = computed(() => !!this.auth.user()?.organizationName?.startsWith('[Impersonating]'));

  workspaceName = computed(() => {
    const name = this.auth.user()?.organizationName ?? '';
    return name.replace(/^\[Impersonating\]\s*/, '');
  });

  exit(): void {
    if (typeof localStorage === 'undefined') return;
    const superToken = localStorage.getItem('super_token');
    if (!superToken) {
      // Fallback: no saved super admin token, so we can't restore.
      // Force a re-login instead.
      localStorage.removeItem('auth_token');
      this.auth.clearUser();
      this.router.navigate(['/login']);
      return;
    }
    // Swap tokens: the saved super_token becomes the active session
    // again, and we discard the impersonation JWT.
    localStorage.setItem('auth_token', superToken);
    localStorage.removeItem('super_token');
    // Refresh user identity from the server so the header chip shows
    // the super admin's context (no [Impersonating] prefix, etc.).
    this.api.user().subscribe({
      next: (info) => {
        this.auth.setUser({
          username: info.username || 'super',
          email: info.email || '',
          roles: (info.roles ?? []).map(r => String(r)),
          organizationName: (info as any).organizationName ?? '',
        });
        this.router.navigate(['/superadmin']);
      },
      error: () => {
        // If /user rejects the restored token (expired / rotated), send
        // the super admin back to login rather than leaving them in a
        // half-broken impersonation state.
        localStorage.removeItem('auth_token');
        this.auth.clearUser();
        this.router.navigate(['/login']);
      },
    });
  }
}
