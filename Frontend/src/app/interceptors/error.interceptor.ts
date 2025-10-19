import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError(error => {
      console.log('Error interceptor caught error:', {
        status: error.status,
        url: req.url,
        method: req.method,
        currentUrl: router.url
      });

      if (error.status === 401) {
        // Unauthorized - clear auth and redirect to login
        // Only redirect to login if we're not already on a public page
        const currentUrl = router.url;
        const publicRoutes = ['/', '/videos', '/channels', '/login', '/register'];
        
        console.log('401 error - current URL:', currentUrl, 'is public:', publicRoutes.includes(currentUrl));
        
        // Don't redirect if we're on a public route or if the request is to a public endpoint
        if (!publicRoutes.includes(currentUrl) && 
            !currentUrl.startsWith('/videos/') && 
            !currentUrl.startsWith('/channels/') &&
            !req.url.includes('/home-feed') &&
            !req.url.includes('/api/video') &&
            !req.url.includes('/api/channel')) {
          
          console.log('Redirecting to login due to 401 error');
          localStorage.removeItem('token');
          router.navigate(['/login']);
        } else {
          console.log('Not redirecting to login - on public route or public endpoint');
        }
      } else if (error.status === 403) {
        // Forbidden - redirect to home
        console.log('403 error - redirecting to home');
        router.navigate(['/']);
      }
      return throwError(() => error);
    })
  );
};

