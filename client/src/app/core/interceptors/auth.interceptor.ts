import { HttpInterceptorFn } from "@angular/common/http";

const getAuthToken = (): string | null => {
  if (typeof window === 'undefined') {
    return null;
  }
  try {
    return window.localStorage?.getItem('auth_token') ?? null;
  } catch {
    return null;
  }
};

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = getAuthToken();
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  return next(req);
};
