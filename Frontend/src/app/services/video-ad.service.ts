import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VideoAd {
  id: number;
  campaignId: number;
  type: 'pre-roll' | 'mid-roll' | 'post-roll';
  content: string;
  url?: string;
  thumbnailUrl?: string;
  duration: number; // in seconds
  skipAfter: number; // seconds before skip button appears
  position?: number; // for mid-roll ads, video timestamp
}

export interface AdRequest {
  videoId: number;
  userId?: string;
  category?: string;
  deviceType?: string;
}

export interface AdResponse {
  hasAd: boolean;
  ad?: VideoAd;
  adType?: string;
}

@Injectable({
  providedIn: 'root'
})
export class VideoAdService {
  private apiUrl = `${environment.apiUrl}/api/ad`;
  private currentAd$ = new BehaviorSubject<VideoAd | null>(null);
  private adHistory: VideoAd[] = [];

  constructor(private http: HttpClient) {}

  // Get ad for video player
  getVideoAd(request: AdRequest): Observable<AdResponse> {
    return this.http.post<AdResponse>(`${this.apiUrl}/video-ad`, request);
  }

  // Record ad impression
  recordAdImpression(campaignId: number, videoId: number, userId?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/impressions`, {
      campaignId,
      videoId,
      userId
    });
  }

  // Record ad click
  recordAdClick(campaignId: number, videoId: number, userId?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/clicks`, {
      campaignId,
      videoId,
      userId
    });
  }

  // Record ad completion
  recordAdCompletion(campaignId: number, videoId: number, userId?: string, watchedDuration?: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/completion`, {
      campaignId,
      videoId,
      userId,
      watchedDuration
    });
  }

  // Get current ad
  getCurrentAd(): Observable<VideoAd | null> {
    return this.currentAd$.asObservable();
  }

  // Set current ad
  setCurrentAd(ad: VideoAd | null): void {
    this.currentAd$.next(ad);
    if (ad) {
      this.adHistory.push(ad);
    }
  }

  // Clear current ad
  clearCurrentAd(): void {
    this.currentAd$.next(null);
  }

  // Get ad history
  getAdHistory(): VideoAd[] {
    return [...this.adHistory];
  }

  // Check if user should see ads (based on subscription)
  shouldShowAds(userSubscription?: string): boolean {
    // Premium and Pro users don't see ads
    return !userSubscription || userSubscription === 'Free';
  }

  // Get ad placement positions for mid-roll ads
  getMidRollPositions(videoDuration: number): number[] {
    const positions: number[] = [];
    
    // Add mid-roll ads at 25%, 50%, and 75% of video duration
    if (videoDuration > 60) { // Only for videos longer than 1 minute
      positions.push(Math.floor(videoDuration * 0.25));
      if (videoDuration > 300) { // Only for videos longer than 5 minutes
        positions.push(Math.floor(videoDuration * 0.50));
      }
      if (videoDuration > 600) { // Only for videos longer than 10 minutes
        positions.push(Math.floor(videoDuration * 0.75));
      }
    }
    
    return positions;
  }
}
