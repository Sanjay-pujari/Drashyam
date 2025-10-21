import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  PremiumVideo, 
  PremiumVideoCreate, 
  PremiumVideoUpdate, 
  PremiumPurchase, 
  PremiumPurchaseCreate, 
  PremiumContentAnalytics,
  PagedResult 
} from '../models/premium-content.model';

@Injectable({
  providedIn: 'root'
})
export class PremiumContentService {
  private apiUrl = `${environment.apiUrl}/api/premiumcontent`;

  constructor(private http: HttpClient) {}

  // Premium Video Management
  createPremiumVideo(premiumVideo: PremiumVideoCreate): Observable<PremiumVideo> {
    return this.http.post<PremiumVideo>(`${this.apiUrl}/videos`, premiumVideo);
  }

  updatePremiumVideo(id: number, premiumVideo: PremiumVideoUpdate): Observable<PremiumVideo> {
    return this.http.put<PremiumVideo>(`${this.apiUrl}/videos/${id}`, premiumVideo);
  }

  deletePremiumVideo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/videos/${id}`);
  }

  getPremiumVideo(id: number): Observable<PremiumVideo> {
    return this.http.get<PremiumVideo>(`${this.apiUrl}/videos/${id}`);
  }

  getPremiumVideos(page: number = 1, pageSize: number = 20): Observable<PagedResult<PremiumVideo>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<PremiumVideo>>(`${this.apiUrl}/videos`, { params });
  }

  isVideoPremium(videoId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/videos/${videoId}/is-premium`);
  }

  getPremiumVideoByVideoId(videoId: number): Observable<PremiumVideo> {
    return this.http.get<PremiumVideo>(`${this.apiUrl}/videos/by-video/${videoId}`);
  }

  updatePremiumVideoByVideoId(videoId: number, premiumVideo: PremiumVideoUpdate): Observable<PremiumVideo> {
    return this.http.put<PremiumVideo>(`${this.apiUrl}/videos/by-video/${videoId}`, premiumVideo);
  }

  deletePremiumVideoByVideoId(videoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/videos/by-video/${videoId}`);
  }

  hasUserPurchased(premiumVideoId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/videos/${premiumVideoId}/has-purchased`);
  }

  // Purchase Management
  createPurchase(purchase: PremiumPurchaseCreate): Observable<PremiumPurchase> {
    return this.http.post<PremiumPurchase>(`${this.apiUrl}/purchases`, purchase);
  }

  completePurchase(paymentIntentId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/purchases/${paymentIntentId}/complete`, {});
  }

  refundPurchase(purchaseId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/purchases/${purchaseId}/refund`, {});
  }

  getUserPurchases(page: number = 1, pageSize: number = 20): Observable<PagedResult<PremiumPurchase>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<PremiumPurchase>>(`${this.apiUrl}/purchases`, { params });
  }

  // Analytics
  getPremiumContentAnalytics(
    premiumVideoId: number, 
    startDate?: string, 
    endDate?: string
  ): Observable<PremiumContentAnalytics> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<PremiumContentAnalytics>(
      `${this.apiUrl}/videos/${premiumVideoId}/analytics`, 
      { params }
    );
  }

  getPremiumRevenue(startDate?: string, endDate?: string): Observable<number> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<number>(`${this.apiUrl}/revenue`, { params });
  }
}