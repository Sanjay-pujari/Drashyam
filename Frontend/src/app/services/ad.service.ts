import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Ad {
  id: number;
  type: AdType;
  content?: string;
  url?: string;
  thumbnailUrl?: string;
  costPerClick: number;
  costPerView: number;
}

export interface AdCampaign {
  id: number;
  name: string;
  description?: string;
  advertiserId: string;
  type: AdType;
  budget: number;
  costPerClick: number;
  costPerView: number;
  startDate: string;
  endDate: string;
  status: AdStatus | string;
  targetAudience?: string;
  adContent?: string;
  adUrl?: string;
  thumbnailUrl?: string;
  createdAt: string;
  updatedAt?: string;
  advertiser?: any;
  impressions?: AdImpression[];
}

export interface AdCampaignCreate {
  name: string;
  description?: string;
  type: AdType;
  budget: number;
  costPerClick: number;
  costPerView: number;
  startDate: string;
  endDate: string;
  targetAudience?: string;
  adContent?: string;
  adUrl?: string;
  thumbnailUrl?: string;
}

export interface AdCampaignUpdate {
  name?: string;
  description?: string;
  type?: AdType;
  budget?: number;
  costPerClick?: number;
  costPerView?: number;
  startDate?: string;
  endDate?: string;
  status?: AdStatus;
  targetAudience?: string;
  adContent?: string;
  adUrl?: string;
  thumbnailUrl?: string;
}

export interface AdImpression {
  id: number;
  adCampaignId: number;
  videoId?: number;
  userId?: string;
  viewedAt: string;
  wasClicked: boolean;
  clickedAt?: string;
  revenue: number;
  user?: any;
  video?: any;
}

export interface AdRevenue {
  totalRevenue: number;
  totalImpressions: number;
  totalClicks: number;
  clickThroughRate: number;
  revenuePerImpression: number;
  revenuePerClick: number;
}

export interface AdAnalytics {
  campaignId: number;
  totalImpressions: number;
  totalClicks: number;
  totalRevenue: number;
  clickThroughRate: number;
  costPerClick: number;
  costPerImpression: number;
}

export enum AdType {
  Banner = 0,
  Video = 1,
  Overlay = 2,
  Sponsored = 3
}

export enum AdStatus {
  Draft = 0,
  Active = 1,
  Paused = 2,
  Completed = 3,
  Cancelled = 4
}

@Injectable({
  providedIn: 'root'
})
export class AdService {
  private apiUrl = `${environment.apiUrl}/api/ad`;

  constructor(private http: HttpClient) {}

  // Campaign Management
  createCampaign(campaign: AdCampaignCreate): Observable<AdCampaign> {
    return this.http.post<AdCampaign>(`${this.apiUrl}/campaigns`, campaign);
  }

  getCampaign(id: number): Observable<AdCampaign> {
    return this.http.get<AdCampaign>(`${this.apiUrl}/campaigns/${id}`);
  }

  getCampaigns(page: number = 1, pageSize: number = 20): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/campaigns?page=${page}&pageSize=${pageSize}`);
  }

  updateCampaign(id: number, campaign: AdCampaignUpdate): Observable<AdCampaign> {
    return this.http.put<AdCampaign>(`${this.apiUrl}/campaigns/${id}`, campaign);
  }

  deleteCampaign(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/campaigns/${id}`);
  }

  activateCampaign(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/campaigns/${id}/activate`, {});
  }

  pauseCampaign(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/campaigns/${id}/pause`, {});
  }

  // Ad Serving
  getAd(videoId?: number, type?: AdType): Observable<Ad | null> {
    let params = '';
    if (videoId) params += `videoId=${videoId}`;
    if (type !== undefined) params += (params ? '&' : '') + `type=${type}`;
    
    return this.http.get<Ad | null>(`${this.apiUrl}/serve?${params}`);
  }

  recordImpression(campaignId: number, videoId?: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/impressions`, {
      campaignId,
      videoId
    });
  }

  recordClick(campaignId: number, videoId?: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/clicks`, {
      campaignId,
      videoId
    });
  }

  // Analytics
  getAdRevenue(startDate?: string, endDate?: string): Observable<AdRevenue> {
    let params = '';
    if (startDate) params += `startDate=${startDate}`;
    if (endDate) params += (params ? '&' : '') + `endDate=${endDate}`;
    
    return this.http.get<AdRevenue>(`${this.apiUrl}/revenue?${params}`);
  }

  getCampaignAnalytics(campaignId: number, startDate?: string, endDate?: string): Observable<AdAnalytics> {
    let params = '';
    if (startDate) params += `startDate=${startDate}`;
    if (endDate) params += (params ? '&' : '') + `endDate=${endDate}`;
    
    return this.http.get<AdAnalytics>(`${this.apiUrl}/campaigns/${campaignId}/analytics?${params}`);
  }

  getAdvertiserAnalytics(startDate?: string, endDate?: string): Observable<AdAnalytics> {
    let params = '';
    if (startDate) params += `startDate=${startDate}`;
    if (endDate) params += (params ? '&' : '') + `endDate=${endDate}`;
    
    return this.http.get<AdAnalytics>(`${this.apiUrl}/analytics?${params}`);
  }
}