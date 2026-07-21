import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../services/auth.service';
import { Client, LoginDto, LoginResultDto } from '../../services/api-service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class RegisterComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private client = inject(Client);

  tenantSlug = this.route.snapshot.paramMap.get('tenantSlug') ?? '';

  username = signal('');
  email = signal('');
  password = signal('');
  confirmPassword = signal('');
  isLoading = signal(false);
  error = signal('');

  submit(): void {
    this.error.set('');
    if (!this.tenantSlug) {
      this.error.set('Workspace URL is invalid.');
      return;
    }
    if (!this.username() || !this.email() || !this.password() || !this.confirmPassword()) {
      this.error.set('All fields are required');
      return;
    }
    if (this.password() !== this.confirmPassword()) {
      this.error.set('Passwords do not match');
      return;
    }
    if (this.password().length < 6) {
      this.error.set('Password must be at least 6 characters');
      return;
    }
    this.isLoading.set(true);

    // Public /register/{tenantSlug} endpoint — role in the body is
    // ignored server-side; the new user always lands as User in the
    // named tenant.
    this.http.post(`${environment.apiBaseUrl}/api/user/register/${encodeURIComponent(this.tenantSlug)}`, {
      username: this.username(),
      email: this.email(),
      password: this.password(),
      role: 'User',
    }).subscribe({
      next: () => this.autoLogin(),
      error: (err) => {
        this.isLoading.set(false);
        const msg = err?.error?.message ?? 'Registration failed';
        this.error.set(msg);
      },
    });
  }

  // After registering, log the user in and drop them onto the user
  // dashboard directly — no extra step needed.
  private autoLogin(): void {
    const dto = LoginDto.fromJS({ username: this.username(), password: this.password() });
    this.client.login(dto).subscribe({
      next: (result: LoginResultDto) => {
        this.isLoading.set(false);
        if (!result?.token) {
          this.error.set('Login after registration failed. Please log in manually.');
          this.router.navigate(['/login']);
          return;
        }
        if (typeof localStorage !== 'undefined') {
          localStorage.setItem('auth_token', result.token);
        }
        this.auth.setUser({
          username: result.username || this.username(),
          email: result.email || this.email(),
          roles: (result.roles ?? []).map(r => String(r)),
          organizationName: (result as any).organizationName ?? '',
        });
        this.router.navigate(['/user']);
      },
      error: () => {
        this.isLoading.set(false);
        this.error.set('Registered, but auto-login failed. Please log in manually.');
        this.router.navigate(['/login']);
      },
    });
  }
}
