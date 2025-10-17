import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  UserInvite, 
  CreateInvite, 
  AcceptInvite, 
  InviteStats, 
  BulkInvite, 
  InviteLink 
} from '../models/invite.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class InviteService {
  private apiUrl = `${environment.apiUrl}/api/invite`;

  constructor(private http: HttpClient) {}

  createInvite(invite: CreateInvite): Observable<UserInvite> {
    return this.http.post<UserInvite>(this.apiUrl, invite);
  }

  bulkCreateInvites(bulkInvite: BulkInvite): Observable<UserInvite[]> {
    return this.http.post<UserInvite[]>(`${this.apiUrl}/bulk`, bulkInvite);
  }

  getInviteByToken(token: string): Observable<UserInvite> {
    return this.http.get<UserInvite>(`${this.apiUrl}/token/${token}`);
  }

  acceptInvite(token: string, acceptData: AcceptInvite): Observable<UserInvite> {
    return this.http.post<UserInvite>(`${this.apiUrl}/accept/${token}`, acceptData);
  }

  getMyInvites(page: number = 1, pageSize: number = 20): Observable<PagedResult<UserInvite>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<UserInvite>>(`${this.apiUrl}/my-invites`, { params });
  }

  getInvitesByEmail(email: string, page: number = 1, pageSize: number = 20): Observable<PagedResult<UserInvite>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<UserInvite>>(`${this.apiUrl}/by-email/${email}`, { params });
  }

  cancelInvite(inviteId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${inviteId}`);
  }

  resendInvite(inviteId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${inviteId}/resend`, {});
  }

  getInviteStats(): Observable<InviteStats> {
    return this.http.get<InviteStats>(`${this.apiUrl}/stats`);
  }

  createInviteLink(maxUsage?: number, expiresAt?: string): Observable<InviteLink> {
    const params = new HttpParams()
      .set('maxUsage', maxUsage?.toString() || '')
      .set('expiresAt', expiresAt || '');
    
    return this.http.post<InviteLink>(`${this.apiUrl}/link`, {}, { params });
  }

  validateInviteToken(token: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/validate/${token}`);
  }

  expireInvite(inviteId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${inviteId}/expire`, {});
  }
}
