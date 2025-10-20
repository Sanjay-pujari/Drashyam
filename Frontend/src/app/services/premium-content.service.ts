import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PremiumVideo {
  id: number;
  videoId: number;
  videoTitle: string;
  videoThumbnailUrl: string;
  creatorName: string;
  price: number;
  currency: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface PremiumVideoCreate {
  videoId: number;
  price: number;
  currency?: string;
}

export interface PremiumVideoUpdate {
  price?: number;
  currency?: string;
  isActive?: boolean;
}

export interface PremiumPurchase {
  id: number;
  premiumVideoId: number;
  userId: string;
  videoTitle: string;
  amount: number;
  currency: string;
  paymentIntentId: string;
  status: string;
  purchasedAt: string;
  completedAt?: string;
  refundedAt?: string;
}

export interface PremiumPurchaseCreate {
  premiumVideoId: number;
  paymentIntentId: string;
}

export interface PremiumContentAnalytics {
  premiumVideoId: number;
  totalPurchases: number;
  totalRevenue: number;
  averagePrice: number;
  startDate?: string;
  endDate?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class PremiumContentService {
  private apiUrl = `${environment.apiUrl}/api/premiumcontent`;

  constructor(private http: HttpClient) {}

  createPremiumVideo(premiumVideo: PremiumVideoCreate): Observable<PremiumVideo> {
    return this.http.post<PremiumVideo>(`${this.apiUrl}/videos`, premiumVideo);
  }

  updatePremiumVideo(premiumVideoId: number, premiumVideo: PremiumVideoUpdate): Observable<PremiumVideo> {
    return this.http.put<PremiumVideo>(`${this.apiUrl}/videos/${premiumVideoId}`, premiumVideo);
  }

  deletePremiumVideo(premiumVideoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/videos/${premiumVideoId}`);
  }

  getPremiumVideo(premiumVideoId: number): Observable<PremiumVideo> {
    return this.http.get<PremiumVideo>(`${this.apiUrl}/videos/${premiumVideoId}`);
  }

  getPremiumVideos(page: number = 1, pageSize: number = 20): Observable<PagedResult<PremiumVideo>> {
    return this.http.get<PagedResult<PremiumVideo>>(`${this.apiUrl}/videos?page=${page}&pageSize=${pageSize}`);
  }

  isVideoPremium(videoId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/videos/${videoId}/is-premium`);
  }

  hasUserPurchased(premiumVideoId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/videos/${premiumVideoId}/has-purchased`);
  }

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
    return this.http.get<PagedResult<PremiumPurchase>>(`${this.apiUrl}/purchases?page=${page}&pageSize=${pageSize}`);
  }

  getPremiumContentAnalytics(premiumVideoId: number, startDate?: string, endDate?: string): Observable<PremiumContentAnalytics> {
    let url = `${this.apiUrl}/videos/${premiumVideoId}/analytics`;
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (params.toString()) url += `?${params.toString()}`;
    
    return this.http.get<PremiumContentAnalytics>(url);
  }

  getPremiumRevenue(startDate?: string, endDate?: string): Observable<number> {
    let url = `${this.apiUrl}/revenue`;
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (params.toString()) url += `?${params.toString()}`;
    
    return this.http.get<number>(url);
  }

  // Additional methods for video management
  getPremiumVideoByVideoId(videoId: number): Observable<PremiumVideo | null> {
    return this.http.get<PremiumVideo | null>(`${this.apiUrl}/videos/by-video/${videoId}`);
  }

  updatePremiumVideo(videoId: number, premiumData: { price: number; currency: string }): Observable<PremiumVideo> {
    return this.http.put<PremiumVideo>(`${this.apiUrl}/videos/by-video/${videoId}`, premiumData);
  }

  deletePremiumVideo(videoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/videos/by-video/${videoId}`);
  }
}
