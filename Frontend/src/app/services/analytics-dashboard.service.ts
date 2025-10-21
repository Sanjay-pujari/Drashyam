import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AnalyticsSummary {
  totalViews?: number;
  totalLikes?: number;
  totalComments?: number;
  totalShares?: number;
  totalSubscribers?: number;
  totalRevenue?: number;
  averageWatchTime?: number;
  engagementRate?: number;
  revenueGrowth?: number;
  subscriberGrowth?: number;
}

export interface TimeSeriesData {
  date: string;
  views?: number;
  likes?: number;
  comments?: number;
  shares?: number;
  revenue?: number;
  engagementRate?: number;
}

export interface TopVideoAnalytics {
  videoId: number;
  title: string;
  thumbnailUrl: string;
  views?: number;
  likes?: number;
  comments?: number;
  revenue?: number;
  engagementRate?: number;
  createdAt: string;
}

export interface RevenueAnalytics {
  id: number;
  userId: string;
  channelId?: number;
  date: string;
  totalRevenue?: number;
  adRevenue?: number;
  subscriptionRevenue?: number;
  premiumContentRevenue?: number;
  merchandiseRevenue?: number;
  donationRevenue?: number;
  referralRevenue?: number;
  revenuePerView?: number;
  revenuePerSubscriber?: number;
  revenueGrowthRate?: number;
  createdAt: string;
}

export interface GeographicAnalytics {
  country: string;
  countryCode: string;
  views?: number;
  revenue?: number;
  percentage?: number;
  subscribers?: number;
}

export interface DeviceAnalytics {
  deviceType: string;
  views?: number;
  percentage?: number;
  averageWatchTime?: number;
  engagementRate?: number;
}

export interface ReferrerAnalytics {
  referrer: string;
  views?: number;
  percentage?: number;
  conversionRate?: number;
}

export interface AudienceAnalytics {
  id: number;
  userId: string;
  channelId?: number;
  date: string;
  ageGroup?: string;
  gender?: string;
  country?: string;
  deviceType?: string;
  referrer?: string;
  viewCount?: number;
  watchTime?: number;
  engagementScore?: number;
  revenue?: number;
  createdAt: string;
}

export interface EngagementAnalytics {
  id: number;
  userId: string;
  channelId?: number;
  date: string;
  likeRate?: number;
  commentRate?: number;
  shareRate?: number;
  watchTimeRate?: number;
  clickThroughRate?: number;
  retentionRate?: number;
  totalLikes?: number;
  totalComments?: number;
  totalShares?: number;
  totalViews?: number;
  createdAt: string;
}

export interface VideoAnalytics {
  id: number;
  videoId: number;
  videoTitle: string;
  videoThumbnailUrl: string;
  userId: string;
  date: string;
  views?: number;
  uniqueViews?: number;
  likes?: number;
  dislikes?: number;
  comments?: number;
  shares?: number;
  revenue?: number;
  averageWatchTime?: number;
  engagementRate?: number;
  country?: string;
  deviceType?: string;
  referrer?: string;
  createdAt: string;
}

export interface ChannelComparison {
  channelId: number;
  channelName: string;
  views?: number;
  subscribers?: number;
  revenue?: number;
  engagementRate?: number;
  growthRate?: number;
}

export interface AnalyticsFilter {
  startDate?: string;
  endDate?: string;
  channelId?: number;
  country?: string;
  deviceType?: string;
  referrer?: string;
  videoCategory?: string;
}

export interface TrackViewRequest {
  videoId: number;
  country?: string;
  deviceType?: string;
  referrer?: string;
}

export interface TrackEngagementRequest {
  videoId: number;
  engagementType: string;
  value?: number;
}

export interface TrackRevenueRequest {
  amount: number;
  revenueType: string;
  videoId?: number;
  channelId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsDashboardService {
  private apiUrl = `${environment.apiUrl}/api/analytics`;

  constructor(private http: HttpClient) { }

  // Dashboard Overview
  getSummary(startDate?: string, endDate?: string): Observable<AnalyticsSummary> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<AnalyticsSummary>(`${this.apiUrl}/summary`, { params });
  }

  getTimeSeriesData(startDate?: string, endDate?: string): Observable<TimeSeriesData[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<TimeSeriesData[]>(`${this.apiUrl}/time-series`, { params });
  }

  getTopVideos(count: number = 10, startDate?: string, endDate?: string): Observable<TopVideoAnalytics[]> {
    let params = new HttpParams().set('count', count.toString());
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<TopVideoAnalytics[]>(`${this.apiUrl}/top-videos`, { params });
  }

  // Revenue Analytics
  getRevenueAnalytics(startDate?: string, endDate?: string): Observable<RevenueAnalytics> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<RevenueAnalytics>(`${this.apiUrl}/revenue`, { params });
  }

  getRevenueTimeSeries(startDate?: string, endDate?: string): Observable<TimeSeriesData[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<TimeSeriesData[]>(`${this.apiUrl}/revenue/time-series`, { params });
  }

  getTotalRevenue(startDate?: string, endDate?: string): Observable<number> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<number>(`${this.apiUrl}/revenue/total`, { params });
  }

  // Audience Analytics
  getGeographicData(startDate?: string, endDate?: string): Observable<GeographicAnalytics[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<GeographicAnalytics[]>(`${this.apiUrl}/geographic`, { params });
  }

  getDeviceData(startDate?: string, endDate?: string): Observable<DeviceAnalytics[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<DeviceAnalytics[]>(`${this.apiUrl}/devices`, { params });
  }

  getReferrerData(startDate?: string, endDate?: string): Observable<ReferrerAnalytics[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<ReferrerAnalytics[]>(`${this.apiUrl}/referrers`, { params });
  }

  getAudienceInsights(startDate?: string, endDate?: string): Observable<AudienceAnalytics[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<AudienceAnalytics[]>(`${this.apiUrl}/audience`, { params });
  }

  // Engagement Analytics
  getEngagementMetrics(startDate?: string, endDate?: string): Observable<EngagementAnalytics> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<EngagementAnalytics>(`${this.apiUrl}/engagement`, { params });
  }

  getEngagementTimeSeries(startDate?: string, endDate?: string): Observable<TimeSeriesData[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<TimeSeriesData[]>(`${this.apiUrl}/engagement/time-series`, { params });
  }

  // Video Analytics
  getVideoAnalytics(videoId: number, startDate?: string, endDate?: string): Observable<VideoAnalytics> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<VideoAnalytics>(`${this.apiUrl}/videos/${videoId}`, { params });
  }

  getVideoAnalyticsList(startDate?: string, endDate?: string): Observable<VideoAnalytics[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<VideoAnalytics[]>(`${this.apiUrl}/videos`, { params });
  }

  // Channel Analytics
  getChannelComparison(startDate?: string, endDate?: string): Observable<ChannelComparison[]> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<ChannelComparison[]>(`${this.apiUrl}/channels/comparison`, { params });
  }

  getChannelAnalytics(channelId: number, startDate?: string, endDate?: string): Observable<AnalyticsSummary> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<AnalyticsSummary>(`${this.apiUrl}/channels/${channelId}`, { params });
  }

  // Real-time Analytics
  getRealTimeAnalytics(): Observable<AnalyticsSummary> {
    return this.http.get<AnalyticsSummary>(`${this.apiUrl}/real-time`);
  }

  getRealTimeVideoAnalytics(): Observable<VideoAnalytics[]> {
    return this.http.get<VideoAnalytics[]>(`${this.apiUrl}/real-time/videos`);
  }

  // Data Tracking
  trackView(request: TrackViewRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/track/view`, request);
  }

  trackEngagement(request: TrackEngagementRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/track/engagement`, request);
  }

  trackRevenue(request: TrackRevenueRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/track/revenue`, request);
  }

  // Export and Reporting
  exportAnalyticsReport(startDate: string, endDate: string, format: string = 'csv'): Observable<Blob> {
    const params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate)
      .set('format', format);
    
    return this.http.get(`${this.apiUrl}/export`, { 
      params, 
      responseType: 'blob' 
    });
  }

  getDashboardReport(startDate?: string, endDate?: string): Observable<any> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get(`${this.apiUrl}/dashboard`, { params });
  }

  // Helper methods
  formatNumber(value: number | undefined | null): string {
    if (value === undefined || value === null || isNaN(value)) {
      return '0';
    }
    if (value >= 1000000) {
      return (value / 1000000).toFixed(1) + 'M';
    } else if (value >= 1000) {
      return (value / 1000).toFixed(1) + 'K';
    }
    return value.toString();
  }

  formatCurrency(value: number | undefined | null, currency: string = 'USD'): string {
    if (value === undefined || value === null || isNaN(value)) {
      return '$0.00';
    }
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency
    }).format(value);
  }

  formatPercentage(value: number | undefined | null): string {
    if (value === undefined || value === null || isNaN(value)) {
      return '0.0%';
    }
    return (value * 100).toFixed(1) + '%';
  }

  formatDuration(seconds: number | undefined | null): string {
    if (seconds === undefined || seconds === null || isNaN(seconds)) {
      return '0:00';
    }
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    } else {
      return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }
  }
}
