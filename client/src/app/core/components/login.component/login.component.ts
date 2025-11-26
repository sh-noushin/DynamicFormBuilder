import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { Client, LoginDto, LoginResultDto, RegisterUserDto } from '../../services/api-service';
import { Router } from 'express';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTabsModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  regUsername = signal('');
  regEmail = signal('');
  regPassword = signal('');
  regConfirmPassword = signal('');
  register() {
    this.isLoading.set(true);
    this.error.set('');
    if (!this.regUsername() || !this.regEmail() || !this.regPassword() || !this.regConfirmPassword()) {
      this.error.set('All fields are required');
      this.isLoading.set(false);
      return;
    }
    if (this.regPassword() !== this.regConfirmPassword()) {
      this.error.set('Passwords do not match');
      this.isLoading.set(false);
      return;
    }
    const dto = RegisterUserDto.fromJS({
      username: this.regUsername(),
      email: this.regEmail(),
      password: this.regPassword(),
      role: 'User'
    });
    this.client.register(dto).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.error.set('');
        this.regUsername.set('');
        this.regEmail.set('');
        this.regPassword.set('');
        this.regConfirmPassword.set('');  
      },
      error: () => {
        this.isLoading.set(false);
        this.error.set('Registration failed');
      }
    });
  }
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
        if (result?.token && result?.roles) {
          if (typeof window !== 'undefined' && window.localStorage) {
            localStorage.setItem('auth_token', result.token);
          }
          const route = String(result.roles[0]) === 'Admin' ? '/admin' : '/user';
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
