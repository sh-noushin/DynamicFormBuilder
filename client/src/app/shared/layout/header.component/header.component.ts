import { Component, inject, signal } from '@angular/core';
import { Client } from '../../../core/services/api-service';
import { Router } from 'express';

@Component({
  selector: 'app-header',
  standalone: true,
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  private client = inject(Client);
  private router = inject(Router);
  username = signal('');

  constructor() {
    this.client.user().subscribe({
      next: (user) => {
        this.username.set(user?.email || user?.username || '');
      }
    });
  }

  logout() {
    localStorage.removeItem('auth_token');
    this.router.navigate(['/login']);
  }
}
