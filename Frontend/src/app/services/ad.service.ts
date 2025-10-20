import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AdCampaign {
  id: number;
  name: string;
  description?: string;
  advertiserId: string;
  advertiserName: string;
  type: string;
  budget: number;
  costPerClick: number;
  costPerView: number;
  startDate: string;
  endDate: string;
  status: string;
  targetAudience?: string;
  adContent?: string;
  adUrl?: string;
  thumbnailUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface AdCampaignCreate {
  name: string;
  description?: string;
  type: string;
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
  type?: string;
  budget?: number;
  costPerClick?: number;
  costPerView?: number;
  startDate?: string;
  endDate?: string;
  status?: string;
  targetAudience?: string;
  adContent?: string;
  adUrl?: string;
  thumbnailUrl?: string;
}

export interface AdAnalytics {
  campaignId: number;
  totalImpressions: number;
  totalClicks: number;
  clickThroughRate: number;
  totalRevenue: number;
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
export class AdService {
  private apiUrl = `${environment.apiUrl}/api/ad`;

  constructor(private http: HttpClient) {}

  createCampaign(campaign: AdCampaignCreate): Observable<AdCampaign> {
    return this.http.post<AdCampaign>(`${this.apiUrl}/campaigns`, campaign);
  }

  updateCampaign(campaignId: number, campaign: AdCampaignUpdate): Observable<AdCampaign> {
    return this.http.put<AdCampaign>(`${this.apiUrl}/campaigns/${campaignId}`, campaign);
  }

  deleteCampaign(campaignId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/campaigns/${campaignId}`);
  }

  getCampaign(campaignId: number): Observable<AdCampaign> {
    return this.http.get<AdCampaign>(`${this.apiUrl}/campaigns/${campaignId}`);
  }

  getCampaigns(page: number = 1, pageSize: number = 20): Observable<PagedResult<AdCampaign>> {
    return this.http.get<PagedResult<AdCampaign>>(`${this.apiUrl}/campaigns?page=${page}&pageSize=${pageSize}`);
  }

  activateCampaign(campaignId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/campaigns/${campaignId}/activate`, {});
  }

  pauseCampaign(campaignId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/campaigns/${campaignId}/pause`, {});
  }

  getCampaignAnalytics(campaignId: number, startDate?: string, endDate?: string): Observable<AdAnalytics> {
    let url = `${this.apiUrl}/campaigns/${campaignId}/analytics`;
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (params.toString()) url += `?${params.toString()}`;
    
    return this.http.get<AdAnalytics>(url);
  }

  getRevenue(startDate?: string, endDate?: string): Observable<number> {
    let url = `${this.apiUrl}/revenue`;
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (params.toString()) url += `?${params.toString()}`;
    
    return this.http.get<number>(url);
  }
}
