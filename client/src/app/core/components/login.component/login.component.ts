import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Client, LoginDto, LoginResultDto } from '../../services/api-service';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    RouterModule,
  ],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  private auth = inject(AuthService);
  username = signal('');
  password = signal('');
  isLoading = signal(false);
  error = signal('');
  private client = inject(Client);
  private router = inject(Router);

  login() {
    this.isLoading.set(true);
    this.error.set('');
    if (!this.username() || !this.password()) {
      this.error.set('Username and password are required');
      this.isLoading.set(false);
      return;
    }
    const dto = LoginDto.fromJS({
      username: this.username(),
      password: this.password()
    });

    this.client.login(dto).subscribe({
      next: (result: LoginResultDto) => {
        if (result?.token) {
          if (typeof window !== 'undefined' && window.localStorage) {
            localStorage.setItem('auth_token', result.token);
          }
          const userInfo = {
            username: result.username || this.username(),
            email: result.email || '',
            roles: (result.roles ?? []).map(r => String(r)),
            organizationName: (result as any).organizationName ?? '',
          };
          this.auth.setUser(userInfo);
          // Route by highest-privilege role in the token. Super admin
          // manages tenants; tenant admin manages their workspace;
          // regular users fill forms.
          const roleStrings = userInfo.roles;
          const route = roleStrings.includes('SuperAdmin') ? '/superadmin'
            : roleStrings.includes('Admin') ? '/admin'
            : '/user';
          this.router.navigate([route]);
        } else {
          this.error.set('Invalid credentials');
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set('Login failed');
        this.isLoading.set(false);
      }
    });
  }
}
