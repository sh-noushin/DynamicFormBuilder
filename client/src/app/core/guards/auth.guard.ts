import { Inject, Injectable, PLATFORM_ID } from "@angular/core";
import { Client } from "../services/api-service";
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router, CanActivate } from "@angular/router";
import { Observable, of } from "rxjs";
import { isPlatformBrowser } from "@angular/common";

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private apiClient: Client,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> {
    if (!isPlatformBrowser(this.platformId)) {
      return of(true);
    }

    const token = localStorage.getItem('auth_token');
    if (token) {
      return new Observable<boolean>(observer => {
        this.apiClient.user().subscribe({
          next: () => {
            observer.next(true);
            observer.complete();
          },
          error: err => {
            console.warn('[AuthGuard] Token validation failed:', err);
            this.removeTokenAndRedirect();
            observer.next(false);
            observer.complete();
          }
        });
      });
    } else {
      this.router.navigate(['/login']);
      return of(false);
    }
  }

  private removeTokenAndRedirect(): void {
    localStorage.removeItem('auth_token');
    this.router.navigate(['/login']);
  }
}