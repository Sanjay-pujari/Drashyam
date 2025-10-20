import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface AdServe {
  hasAd: boolean;
  campaignId?: number;
  adType?: string;
  adContent?: string;
  adUrl?: string;
  thumbnailUrl?: string;
  costPerClick?: number;
  costPerView?: number;
}

@Component({
  selector: 'app-ad-banner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="ad-banner" *ngIf="ad && ad.hasAd">
      <div class="ad-content" (click)="onAdClick()">
        <img *ngIf="ad.thumbnailUrl" [src]="ad.thumbnailUrl" [alt]="'Advertisement'" class="ad-image">
        <div class="ad-text" *ngIf="ad.adContent" [innerHTML]="ad.adContent"></div>
        <div class="ad-label">Advertisement</div>
      </div>
    </div>
  `,
  styles: [`
    .ad-banner {
      margin: 16px 0;
      border: 1px solid #ddd;
      border-radius: 8px;
      overflow: hidden;
      background: #f9f9f9;
    }

    .ad-content {
      position: relative;
      cursor: pointer;
      display: block;
    }

    .ad-image {
      width: 100%;
      height: auto;
      display: block;
    }

    .ad-text {
      padding: 12px;
      font-size: 14px;
      line-height: 1.4;
    }

    .ad-label {
      position: absolute;
      top: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.7);
      color: white;
      padding: 2px 6px;
      border-radius: 3px;
      font-size: 10px;
      font-weight: bold;
    }
  `]
})
export class AdBannerComponent implements OnInit, OnDestroy {
  @Input() videoId?: number;
  @Input() category?: string;
  @Input() location?: string;
  @Input() deviceType?: string;

  ad: AdServe | null = null;
  private userId?: string;

  constructor(private http: HttpClient) {
    // Get user ID from localStorage or auth service
    this.userId = localStorage.getItem('userId') || undefined;
  }

  ngOnInit() {
    this.loadAd();
  }

  ngOnDestroy() {
    // Record impression when component is destroyed
    if (this.ad?.hasAd && this.ad.campaignId) {
      this.recordImpression();
    }
  }

  private loadAd() {
    const request = {
      userId: this.userId,
      videoId: this.videoId,
      category: this.category,
      location: this.location,
      deviceType: this.deviceType || 'desktop'
    };

    this.http.post<AdServe>(`${environment.apiUrl}/api/ad/serve`, request).subscribe({
      next: (ad) => {
        this.ad = ad;
        if (ad.hasAd) {
          this.recordImpression();
        }
      },
      error: (error) => {
        console.error('Error loading ad:', error);
      }
    });
  }

  onAdClick() {
    if (this.ad?.hasAd && this.ad.campaignId) {
      this.recordClick();
      if (this.ad.adUrl) {
        window.open(this.ad.adUrl, '_blank');
      }
    }
  }

  private recordImpression() {
    if (!this.ad?.campaignId) return;

    this.http.post(`${environment.apiUrl}/api/ad/impressions`, {
      campaignId: this.ad.campaignId,
      userId: this.userId,
      videoId: this.videoId
    }).subscribe({
      error: (error) => console.error('Error recording impression:', error)
    });
  }

  private recordClick() {
    if (!this.ad?.campaignId) return;

    this.http.post(`${environment.apiUrl}/api/ad/clicks`, {
      campaignId: this.ad.campaignId,
      userId: this.userId,
      videoId: this.videoId
    }).subscribe({
      error: (error) => console.error('Error recording click:', error)
    });
  }
}
