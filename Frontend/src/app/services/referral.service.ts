import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  Referral, 
  CreateReferral, 
  ReferralStats, 
  ReferralReward, 
  ClaimReward, 
  ReferralCode, 
  CreateReferralCode 
} from '../models/referral.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReferralService {
  private apiUrl = `${environment.apiUrl}/api/referral`;

  constructor(private http: HttpClient) {}

  createReferral(referral: CreateReferral): Observable<Referral> {
    return this.http.post<Referral>(this.apiUrl, referral);
  }

  getReferralById(referralId: number): Observable<Referral> {
    return this.http.get<Referral>(`${this.apiUrl}/${referralId}`);
  }

  getMyReferrals(page: number = 1, pageSize: number = 20): Observable<PagedResult<Referral>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<Referral>>(`${this.apiUrl}/my-referrals`, { params });
  }

  getReferralsByUser(userId: string, page: number = 1, pageSize: number = 20): Observable<PagedResult<Referral>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<Referral>>(`${this.apiUrl}/referrals-by-user/${userId}`, { params });
  }

  getReferralStats(): Observable<ReferralStats> {
    return this.http.get<ReferralStats>(`${this.apiUrl}/stats`);
  }

  createReferralCode(createCode: CreateReferralCode): Observable<ReferralCode> {
    return this.http.post<ReferralCode>(`${this.apiUrl}/code`, createCode);
  }

  getReferralCode(): Observable<ReferralCode> {
    return this.http.get<ReferralCode>(`${this.apiUrl}/code`);
  }

  validateReferralCode(code: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/validate-code/${code}`);
  }

  claimReward(claimReward: ClaimReward): Observable<ReferralReward> {
    return this.http.post<ReferralReward>(`${this.apiUrl}/claim-reward`, claimReward);
  }

  getUserRewards(page: number = 1, pageSize: number = 20): Observable<PagedResult<ReferralReward>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<ReferralReward>>(`${this.apiUrl}/rewards`, { params });
  }

  processReferralReward(referralId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${referralId}/process-reward`, {});
  }

  updateReferralStatus(referralId: number, status: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${referralId}/status`, status);
  }
}
