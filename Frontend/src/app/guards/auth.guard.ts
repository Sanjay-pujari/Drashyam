import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, catchError, of } from 'rxjs';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  console.log('Auth guard checking access to history page');
  
  return authService.currentUser$.pipe(
    map(user => {
      console.log('Auth guard - current user:', user);
      if (user) {
        console.log('Auth guard - access granted');
        return true;
      } else {
        console.log('Auth guard - redirecting to login');
        router.navigate(['/login']);
        return false;
      }
    }),
    catchError((error) => {
      console.error('Auth guard error:', error);
      router.navigate(['/login']);
      return of(false);
    })
  );
};

