import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface QuotaStatus {
  userId: string;
  subscriptionType: string;
  storageUsed: number;
  storageLimit: number;
  videosUploaded: number;
  videoLimit: number;
  channelsCreated: number;
  channelLimit: number;
  hasAds: boolean;
  hasAnalytics: boolean;
  hasMonetization: boolean;
  hasLiveStreaming: boolean;
  storageUsagePercentage: number;
  videoUsagePercentage: number;
  channelUsagePercentage: number;
}

export interface QuotaWarning {
  userId: string;
  warnings: string[];
  hasWarnings: boolean;
  recommendedAction: string;
}

export interface SubscriptionBenefits {
  currentPlan: SubscriptionPlan;
  nextUpgradePlan?: SubscriptionPlan;
  benefits: string[];
  upgradeBenefits: string[];
  quotaStatus: QuotaStatus;
}

export interface SubscriptionPlan {
  id: number;
  name: string;
  description?: string;
  price: number;
  billingCycle: string;
  maxChannels: number;
  maxVideosPerChannel: number;
  maxStorageGB: number;
  hasAds: boolean;
  hasAnalytics: boolean;
  hasMonetization: boolean;
  hasLiveStreaming: boolean;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class QuotaService {
  private apiUrl = `${environment.apiUrl}/api/quota`;

  constructor(private http: HttpClient) {}

  getQuotaStatus(): Observable<QuotaStatus> {
    return this.http.get<QuotaStatus>(`${this.apiUrl}/status`);
  }

  getQuotaWarnings(): Observable<QuotaWarning> {
    return this.http.get<QuotaWarning>(`${this.apiUrl}/warnings`);
  }

  getSubscriptionBenefits(): Observable<SubscriptionBenefits> {
    return this.http.get<SubscriptionBenefits>(`${this.apiUrl}/benefits`);
  }

  canUploadVideo(fileSize: number): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/can-upload`, { fileSize });
  }

  canCreateChannel(): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/can-create-channel`);
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getUsageColor(percentage: number): string {
    if (percentage >= 90) return 'warn';
    if (percentage >= 75) return 'accent';
    return 'primary';
  }
}