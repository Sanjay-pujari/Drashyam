import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  profilePictureUrl?: string;
  subscriptionType: string;
  isActive: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresIn: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeAuth();
  }

  private initializeAuth() {
    const token = this.getToken();
    if (token && !this.isTokenExpired(token)) {
      const user = this.getUserFromToken(token);
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
    } else {
      this.logout();
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        tap(response => {
          this.setAuthData(response);
        }),
        catchError(this.handleError)
      );
  }

  register(userData: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/register`, userData)
      .pipe(
        tap(response => {
          this.setAuthData(response);
        }),
        catchError(this.handleError)
      );
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/refresh`, {})
      .pipe(
        tap(response => {
          this.setAuthData(response);
        }),
        catchError(error => {
          this.logout();
          return throwError(error);
        })
      );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.API_URL}/auth/forgot-password`, { email })
      .pipe(catchError(this.handleError));
  }

  resetPassword(token: string, password: string): Observable<any> {
    return this.http.post(`${this.API_URL}/auth/reset-password`, { token, password })
      .pipe(catchError(this.handleError));
  }

  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.API_URL}/auth/change-password`, {
      currentPassword,
      newPassword
    }).pipe(catchError(this.handleError));
  }

  updateProfile(userData: Partial<User>): Observable<User> {
    return this.http.put<User>(`${this.API_URL}/auth/profile`, userData)
      .pipe(
        tap(user => {
          this.currentUserSubject.next(user);
          localStorage.setItem('user', JSON.stringify(user));
        }),
        catchError(this.handleError)
      );
  }

  uploadProfilePicture(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ url: string }>(`${this.API_URL}/auth/profile-picture`, formData)
      .pipe(
        map(response => response.url),
        tap(url => {
          const currentUser = this.currentUserSubject.value;
          if (currentUser) {
            currentUser.profilePictureUrl = url;
            this.currentUserSubject.next(currentUser);
            localStorage.setItem('user', JSON.stringify(currentUser));
          }
        }),
        catchError(this.handleError)
      );
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.subscriptionType === role;
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  private setAuthData(response: AuthResponse) {
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response.user));
    this.currentUserSubject.next(response.user);
    this.isAuthenticatedSubject.next(true);
  }

  private getUserFromToken(token: string): User {
    try {
      const decoded: any = jwtDecode(token);
      return {
        id: decoded.sub,
        firstName: decoded.firstName,
        lastName: decoded.lastName,
        email: decoded.email,
        profilePictureUrl: decoded.profilePictureUrl,
        subscriptionType: decoded.subscriptionType,
        isActive: decoded.isActive
      };
    } catch (error) {
      console.error('Error decoding token:', error);
      throw new Error('Invalid token');
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp < currentTime;
    } catch (error) {
      return true;
    }
  }

  private handleError = (error: any) => {
    let errorMessage = 'An error occurred';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = error.error.message;
    } else if (error.error && error.error.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    }

    console.error('Auth Service Error:', error);
    return throwError(() => new Error(errorMessage));
  };
}
