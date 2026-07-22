import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, RouterModule, MatFormFieldModule, MatInputModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class ResetPasswordComponent {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // Both come from the reset email's URL as ?email=...&token=... query
  // params. Missing either one = someone visited the page directly.
  email = this.route.snapshot.queryParamMap.get('email') ?? '';
  token = this.route.snapshot.queryParamMap.get('token') ?? '';

  newPassword = signal('');
  confirmPassword = signal('');
  isLoading = signal(false);
  isDone = signal(false);
  error = signal('');

  submit(): void {
    this.error.set('');
    if (!this.email || !this.token) {
      this.error.set('This reset link is missing information. Request a new one.');
      return;
    }
    if (this.newPassword() !== this.confirmPassword()) {
      this.error.set('Passwords do not match.');
      return;
    }
    if (this.newPassword().length < 6) {
      this.error.set('Password must be at least 6 characters.');
      return;
    }
    this.isLoading.set(true);
    this.http.post(`${environment.apiBaseUrl}/api/auth/reset-password`, {
      email: this.email,
      token: this.token,
      newPassword: this.newPassword(),
    }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isDone.set(true);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err?.error?.detail ?? err?.error?.message ?? 'Reset link is invalid or expired.';
        this.error.set(msg);
      },
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
