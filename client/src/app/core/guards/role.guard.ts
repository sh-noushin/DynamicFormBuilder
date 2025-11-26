import { Inject, Injectable, PLATFORM_ID } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from "@angular/router";
import { Client, UserInfoDto, UserRole } from "../services/api-service";
import { catchError, map, Observable, of } from "rxjs";
import { isPlatformBrowser } from "@angular/common";

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {

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

    const requiredRoles: UserRole[] = route.data['roles'] || [];
    const token = localStorage.getItem('auth_token');
    
    if (!token) {
      this.router.navigate(['/login']);
      return of(false);
    }

    return this.apiClient.user().pipe(
      map((userInfo: UserInfoDto) => {
        if (requiredRoles.length === 0) {
          return true; 
        }
        
        const hasRequiredRole = userInfo?.roles?.some(role => 
          requiredRoles.includes(role)
        );
        
        if (!hasRequiredRole) {
          console.warn('[RoleGuard] User does not have required role');
          this.redirectToUnauthorized();
          return false;
        }
        
        return true;
      }),
      catchError(err => {
        console.warn('[RoleGuard] Token validation failed:', err);
        localStorage.removeItem('auth_token');
        this.router.navigate(['/login']);
        return of(false);
      })
    );
  }

  private redirectToUnauthorized(): void {
    this.apiClient.user().pipe(
      catchError(() => {
        this.router.navigate(['/login']);
        return of(null);
      })
    ).subscribe(userInfo => {
      if (userInfo?.roles?.includes(UserRole.Admin)) {
        this.router.navigate(['/admin']);
      } else if (userInfo?.roles?.includes(UserRole.User)) {
        this.router.navigate(['/user']);
      } else {
        this.router.navigate(['/login']);
      }
    });
  }
}