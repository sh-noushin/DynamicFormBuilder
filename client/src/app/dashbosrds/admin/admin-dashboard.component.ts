import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { filter } from 'rxjs/operators';
import { HeaderComponent } from '../../shared/layout/header.component/header.component';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatToolbarModule,
    MatButtonModule,
    MatTabsModule,
    HeaderComponent
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent {
  selectedMenuItem = signal<string>('forms');
  selectedIndex = signal<number>(0);

  constructor(private router: Router) {
    this.syncTabWithRoute(this.router.url);
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe((e: any) => {
      const url = e.urlAfterRedirects || e.url || '';
      this.syncTabWithRoute(url);
    });
  }

  selectMenuItem(item: string) {
    this.selectedMenuItem.set(item);
    this.router.navigate(['/admin', item]);
  }

  onTabChange(index: number) {
    this.selectedIndex.set(index);
    const target = index === 0 ? 'forms' : 'users';
    if (this.selectedMenuItem() !== target) {
      this.selectMenuItem(target);
    }
  }

  private syncTabWithRoute(url: string) {
    if (url.includes('/admin/users')) {
      this.selectedMenuItem.set('users');
      this.selectedIndex.set(1);
    } else {
      this.selectedMenuItem.set('forms');
      this.selectedIndex.set(0);
    }
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
