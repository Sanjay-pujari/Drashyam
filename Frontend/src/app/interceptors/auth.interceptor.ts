import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  if (token) {
    // Check if token is expired
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Date.now() / 1000;
      if (payload.exp <= currentTime) {
        console.warn('Token is expired, removing from localStorage');
        localStorage.removeItem('token');
        return next(req);
      }
    } catch (error) {
      console.warn('Invalid token format, removing from localStorage');
      localStorage.removeItem('token');
      return next(req);
    }

    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(cloned);
  }
  return next(req);
};
