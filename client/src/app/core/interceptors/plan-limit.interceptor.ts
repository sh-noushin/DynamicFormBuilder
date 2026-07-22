import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

// Catches HTTP 402 Payment Required responses (thrown by the backend
// when a tenant tries to exceed their plan) and redirects to the
// billing page with a snackbar. The API call still errors so the
// caller can also react — we just make sure the user sees a helpful
// prompt regardless of the caller's error handling.
export const planLimitInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const snack = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 402) {
        const detail = err.error?.detail ?? err.error?.message ?? 'Plan limit reached — upgrade to continue.';
        snack.open(detail, 'Upgrade', { duration: 5000 })
          .onAction()
          .subscribe(() => router.navigate(['/admin/billing']));
        // Also navigate immediately so users who miss the toast still
        // land on the billing page.
        router.navigate(['/admin/billing']);
      }
      return throwError(() => err);
    })
  );
};
