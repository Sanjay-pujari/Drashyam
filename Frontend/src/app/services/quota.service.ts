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

export interface QuotaCheck {
  canUpload: boolean;
  canCreateChannel: boolean;
  canUseFeature: boolean;
  reason?: string;
  warnings?: QuotaWarning;
}

@Injectable({
  providedIn: 'root'
})
export class QuotaService {
  private apiUrl = `${environment.apiUrl}/quota`;

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

  checkVideoUpload(channelId: number, fileSize: number): Observable<QuotaCheck> {
    return this.http.post<QuotaCheck>(`${this.apiUrl}/check-upload`, {
      channelId,
      fileSize
    });
  }

  checkChannelCreation(): Observable<QuotaCheck> {
    return this.http.post<QuotaCheck>(`${this.apiUrl}/check-channel`, {});
  }

  checkFeatureAccess(feature: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/feature/${feature}`);
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getUsagePercentage(used: number, limit: number): number {
    if (limit === 0) return 0;
    return Math.min((used / limit) * 100, 100);
  }

  getUsageColor(percentage: number): string {
    if (percentage >= 90) return '#f44336'; // Red
    if (percentage >= 75) return '#ff9800'; // Orange
    if (percentage >= 50) return '#ffc107'; // Yellow
    return '#4caf50'; // Green
  }

  getSubscriptionTierName(subscriptionType: string): string {
    switch (subscriptionType) {
      case 'Free': return 'Free';
      case 'Premium': return 'Premium';
      case 'Pro': return 'Pro';
      default: return 'Free';
    }
  }

  getSubscriptionTierColor(subscriptionType: string): string {
    switch (subscriptionType) {
      case 'Free': return '#9e9e9e'; // Grey
      case 'Premium': return '#2196f3'; // Blue
      case 'Pro': return '#ff9800'; // Orange
      default: return '#9e9e9e';
    }
  }
}
