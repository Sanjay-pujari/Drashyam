import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { environment } from '../../environments/environment';
import { User, UserLogin, UserRegistration } from '../models/user.model';
import { AppState } from '../store/app.state';
import * as UserActions from '../store/user/user.actions';

export interface AuthResponse {
  user: User;
  token: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/api/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private store: Store<AppState>
  ) {
    // Don't automatically load user in constructor to avoid circular dependency
    // User will be loaded when initializeAuth() is called from app.component
  }

  login(credentials: UserLogin): Observable<AuthResponse> {
    const url = `${this.apiUrl}/login`;
    
    return this.http.post<AuthResponse>(url, credentials).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
        this.currentUserSubject.next(response.user);
        
        // Dispatch action to NgRx store
        this.store.dispatch(UserActions.loginSuccess({ 
          user: response.user, 
          token: response.token 
        }));
      })
    );
  }

  register(userData: UserRegistration): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, userData).pipe(
      tap(response => {
        // Align behavior with login: persist token and set current user if provided
        if (response.token) {
          localStorage.setItem('token', response.token);
        }
        if (response.user) {
          this.currentUserSubject.next(response.user);
        }
      })
    );
  }

  // Convenience observable for auth state
  public isAuthenticated$ = this.currentUser$.pipe(
    map(user => !!user)
  );

  // Check if user is loaded and authenticated
  isUserLoaded(): boolean {
    return this.currentUserSubject.value !== null;
  }

  logout(): void {
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
    
    // Dispatch logout action to NgRx store
    this.store.dispatch(UserActions.logout());
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${environment.apiUrl}/api/user/me`).pipe(
      tap(user => this.currentUserSubject.next(user))
    );
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('token');
    if (!token) return false;
    
    // Check if token is expired (basic check)
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Date.now() / 1000;
      const isValid = payload.exp > currentTime;
      
      if (!isValid) {
        // Token is expired, remove it
        localStorage.removeItem('token');
        this.currentUserSubject.next(null);
      }
      
      return isValid;
    } catch {
      // Invalid token format, remove it
      localStorage.removeItem('token');
      this.currentUserSubject.next(null);
      return false;
    }
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  refreshToken(): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`${this.apiUrl}/refresh-token`, {} as any).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
      }),
      catchError(error => {
        this.logout();
        throw error;
      })
    );
  }

  // Check if token needs refresh and refresh if necessary
  ensureValidToken(): Observable<boolean> {
    const token = localStorage.getItem('token');
    if (!token) {
      return of(false);
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Date.now() / 1000;
      const timeUntilExpiry = payload.exp - currentTime;
      
      // If token expires in less than 5 minutes, try to refresh
      if (timeUntilExpiry < 300) {
        return this.refreshToken().pipe(
          map(() => true),
          catchError(() => of(false))
        );
      }
      
      return of(true);
    } catch {
      return of(false);
    }
  }

  forgotPassword(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/reset-password`, { 
      token, 
      newPassword 
    });
  }

  changePassword(currentPassword: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/change-password`, {
      currentPassword,
      newPassword
    });
  }

  updateProfile(userData: Partial<User>): Observable<User> {
    return this.http.put<User>(`${environment.apiUrl}/api/user/me`, userData).pipe(
      tap(user => this.currentUserSubject.next(user))
    );
  }

  uploadProfilePicture(file: File): Observable<User> {
    const formData = new FormData();
    formData.append('profilePicture', file);
    
    return this.http.post<User>(`${environment.apiUrl}/api/user/me/profile-picture`, formData).pipe(
      tap(user => this.currentUserSubject.next(user))
    );
  }

  // Initialize authentication state on app startup
  initializeAuth(): Observable<User | null> {
    const token = localStorage.getItem('token');
    
    if (!token || !this.isAuthenticated()) {
      this.currentUserSubject.next(null);
      return of(null);
    }
    

    return this.getCurrentUser().pipe(
      tap(user => {
        this.currentUserSubject.next(user);
        
        // Dispatch action to NgRx store
        this.store.dispatch(UserActions.loadCurrentUserSuccess({ user }));
      }),
      map(user => user),
      catchError(error => {
        
        // If it's a 401 or 403, the token is invalid
        if (error.status === 401 || error.status === 403) {
          this.logout();
        }
        
        // Dispatch failure action to NgRx store
        this.store.dispatch(UserActions.loadCurrentUserFailure({ error: error.message }));
        
        return of(null);
      })
    );
  }
}