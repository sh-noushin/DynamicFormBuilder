import { Component, inject, signal, Input } from '@angular/core';
import { Client } from '../../../core/services/api-service';
import { NavigationEnd, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [MatIconModule, MatButtonModule, MatToolbarModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  @Input() title: string = '';
  private router = inject(Router);
  private auth = inject(AuthService);
  user = this.auth.user;

  constructor() {
    this.auth.loadUserFromStorage();
    this.router.events.pipe(filter(ev => ev instanceof NavigationEnd)).subscribe(() => {
      this.auth.loadUserFromStorage();
    });
  }

  logout() {
    localStorage.removeItem('auth_token');
    this.auth.clearUser();
    this.router.navigate(['/login']);
  }
}
