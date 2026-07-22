
import { Component, signal, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { filter } from 'rxjs/operators';
import { HeaderComponent } from '../../shared/layout/header.component/header.component';
import { ImpersonationBannerComponent } from '../../shared/impersonation-banner/impersonation-banner.component';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    RouterModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatToolbarModule,
    MatButtonModule,
    MatTabsModule,
    HeaderComponent,
    ImpersonationBannerComponent,
],
  templateUrl: './admin-dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent {
  selectedMenuItem = signal<string>('overview');
  selectedIndex = signal<number>(0);

  private tabs = ['overview', 'forms', 'users', 'api-keys', 'billing', 'settings'] as const;

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
    const target = this.tabs[index] ?? 'overview';
    if (this.selectedMenuItem() !== target) {
      this.selectMenuItem(target);
    }
  }

  private syncTabWithRoute(url: string) {
    if (url.includes('/admin/settings')) {
      this.selectedMenuItem.set('settings');
      this.selectedIndex.set(5);
    } else if (url.includes('/admin/billing')) {
      this.selectedMenuItem.set('billing');
      this.selectedIndex.set(4);
    } else if (url.includes('/admin/api-keys')) {
      this.selectedMenuItem.set('api-keys');
      this.selectedIndex.set(3);
    } else if (url.includes('/admin/users')) {
      this.selectedMenuItem.set('users');
      this.selectedIndex.set(2);
    } else if (url.includes('/admin/forms')) {
      this.selectedMenuItem.set('forms');
      this.selectedIndex.set(1);
    } else {
      this.selectedMenuItem.set('overview');
      this.selectedIndex.set(0);
    }
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
