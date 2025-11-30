import { Component, inject, signal, Input, computed } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { ChangePasswordDialogComponent, ChangePasswordPayload } from '../../change-password/change-password-dialog.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [MatIconModule, MatButtonModule, MatToolbarModule, MatMenuModule, MatDialogModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  @Input() title: string = '';
  private router = inject(Router);
  private auth = inject(AuthService);
  private dialog = inject(MatDialog);
  user = this.auth.user;
  readonly initials = computed(() => {
    const current = this.user();
    if (!current) {
      return '?';
    }
    const source = (current.username || current.email || '').trim();
    if (!source) {
      return '?';
    }
    const tokens = source.split(/\s+/).filter(Boolean);
    if (tokens.length >= 2) {
      return (tokens[0][0] + tokens[1][0]).toUpperCase();
    }
    return source.substring(0, 2).toUpperCase();
  });

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

  changePassword() {
    const dialogRef = this.dialog.open(ChangePasswordDialogComponent, {
      width: '520px',
      maxWidth: '92vw',
      panelClass: 'change-password-dialog-panel'
    });

    const instance = dialogRef.componentInstance;
    if (!instance) {
      return;
    }

    const subscriptions: Subscription[] = [];
    subscriptions.push(instance.cancelled.subscribe(() => dialogRef.close()));
    subscriptions.push(instance.submitted.subscribe((payload: ChangePasswordPayload) => dialogRef.close(payload)));

    dialogRef.afterClosed().subscribe((payload?: ChangePasswordPayload) => {
      subscriptions.forEach(sub => sub.unsubscribe());
      if (payload) {
        this.handlePasswordChange(payload);
      }
    });
  }

  private handlePasswordChange(payload: ChangePasswordPayload) {
    console.info('Change password payload submitted', payload);
    // TODO: wire to API endpoint and provide user feedback.
  }
}
