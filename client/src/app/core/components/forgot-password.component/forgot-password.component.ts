import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterModule } from '@angular/router';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, RouterModule, MatFormFieldModule, MatInputModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class ForgotPasswordComponent {
  private http = inject(HttpClient);

  email = signal('');
  isLoading = signal(false);
  isSent = signal(false);
  error = signal('');

  submit(): void {
    this.error.set('');
    const email = this.email().trim();
    if (!email) {
      this.error.set('Please enter your email.');
      return;
    }
    this.isLoading.set(true);
    this.http.post(`${environment.apiBaseUrl}/api/auth/request-reset`, { email }).subscribe({
      next: () => {
        this.isLoading.set(false);
        // Server always returns 200 whether the email is registered or
        // not — showing a success screen for both cases so we don't leak
        // account-existence info here either.
        this.isSent.set(true);
      },
      error: () => {
        this.isLoading.set(false);
        this.error.set('Something went wrong. Please try again.');
      },
    });
  }
}
