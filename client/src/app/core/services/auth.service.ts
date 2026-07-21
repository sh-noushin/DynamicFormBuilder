import { Injectable, signal } from '@angular/core';

export interface AuthUser {
  username: string;
  email: string;
  roles: string[];
  // Workspace label rendered in the app header. Populated from the
  // login response's OrganizationName. Older sessions saved to
  // localStorage before this field existed will show empty.
  organizationName?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _user = signal<AuthUser | null>(null);

  get user() {
    return this._user;
  }

  setUser(user: AuthUser | null) {
    this._user.set(user);
    if (user && typeof window !== 'undefined' && window.localStorage) {
      localStorage.setItem('user_info', JSON.stringify(user));
    }
  }

  loadUserFromStorage() {
    if (typeof window !== 'undefined' && window.localStorage) {
      const userStr = localStorage.getItem('user_info');
      if (userStr) {
        try {
          const user = JSON.parse(userStr);
          this._user.set(user);
        } catch {}
      }
    }
  }

  clearUser() {
    this._user.set(null);
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.removeItem('user_info');
    }
  }
}
