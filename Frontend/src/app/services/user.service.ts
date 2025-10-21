import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../models/user.model';
import { PagedResult } from '../models/paged-result.model';

// PagedResult interface moved to models/paged-result.model.ts

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = `${environment.apiUrl}/api/user`;

  constructor(private http: HttpClient) {}

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/me`);
  }

  getById(userId: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${userId}`);
  }

  search(query: string, page = 1, pageSize = 20): Observable<PagedResult<User>> {
    let params = new HttpParams().set('query', query).set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<User>>(`${this.apiUrl}/search`, { params });
  }

  follow(targetUserId: string): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/${targetUserId}/follow`, {});
  }

  unfollow(targetUserId: string): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/${targetUserId}/unfollow`, {});
  }

  getFollowers(userId: string, page = 1, pageSize = 20): Observable<PagedResult<User>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<User>>(`${this.apiUrl}/${userId}/followers`, { params });
  }

  getFollowing(userId: string, page = 1, pageSize = 20): Observable<PagedResult<User>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<User>>(`${this.apiUrl}/${userId}/following`, { params });
  }

  isFollowing(targetUserId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/${targetUserId}/is-following`);
  }
}


