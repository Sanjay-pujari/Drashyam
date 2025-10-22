import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { AdService, Ad, AdType } from '../../services/ad.service';

@Component({
  selector: 'app-ad-overlay',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatCardModule],
  template: `
    @if (ad && showAd) {
      <div class="ad-overlay" [class]="getAdClass()">
        <div class="ad-content">
          @if (ad.type === AdType.Banner) {
            <div class="banner-ad">
              <img [src]="ad.thumbnailUrl" [alt]="'Advertisement'" class="ad-image" *ngIf="ad.thumbnailUrl">
              <div class="ad-text">
                <h4>{{ ad.content }}</h4>
                <button mat-button color="primary" (click)="onAdClick()">
                  Learn More
                </button>
              </div>
            </div>
          } @else if (ad.type === AdType.Video) {
            <div class="video-ad">
              <video [src]="ad.url" controls class="ad-video" *ngIf="ad.url">
                Your browser does not support the video tag.
              </video>
              <div class="ad-overlay-content">
                <h4>{{ ad.content }}</h4>
                <button mat-button color="primary" (click)="onAdClick()">
                  Learn More
                </button>
              </div>
            </div>
          } @else if (ad.type === AdType.Overlay) {
            <div class="overlay-ad">
              <div class="ad-background" [style.background-image]="'url(' + ad.thumbnailUrl + ')'" *ngIf="ad.thumbnailUrl">
                <div class="ad-overlay-content">
                  <h4>{{ ad.content }}</h4>
                  <button mat-button color="primary" (click)="onAdClick()">
                    Learn More
                  </button>
                </div>
              </div>
            </div>
          }
          
          <div class="ad-controls">
            <button mat-icon-button (click)="closeAd()" class="close-button">
              <mat-icon>close</mat-icon>
            </button>
            <span class="ad-label">Advertisement</span>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .ad-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0, 0, 0, 0.8);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .ad-content {
      background: white;
      border-radius: 8px;
      max-width: 90%;
      max-height: 90%;
      overflow: hidden;
      position: relative;
    }

    .banner-ad {
      display: flex;
      align-items: center;
      padding: 20px;
      min-width: 400px;
    }

    .ad-image {
      width: 120px;
      height: 80px;
      object-fit: cover;
      border-radius: 4px;
      margin-right: 20px;
    }

    .ad-text h4 {
      margin: 0 0 10px 0;
      color: #333;
    }

    .video-ad {
      position: relative;
      max-width: 800px;
    }

    .ad-video {
      width: 100%;
      max-height: 500px;
    }

    .ad-overlay-content {
      position: absolute;
      bottom: 0;
      left: 0;
      right: 0;
      background: linear-gradient(transparent, rgba(0, 0, 0, 0.8));
      color: white;
      padding: 20px;
    }

    .overlay-ad {
      position: relative;
      max-width: 600px;
      max-height: 400px;
    }

    .ad-background {
      width: 100%;
      height: 300px;
      background-size: cover;
      background-position: center;
      border-radius: 8px;
      position: relative;
    }

    .ad-controls {
      position: absolute;
      top: 10px;
      right: 10px;
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .close-button {
      background: rgba(0, 0, 0, 0.5);
      color: white;
    }

    .ad-label {
      background: rgba(0, 0, 0, 0.7);
      color: white;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
    }

    .banner-ad .ad-controls {
      position: absolute;
      top: 10px;
      right: 10px;
    }

    .video-ad .ad-controls {
      position: absolute;
      top: 10px;
      right: 10px;
    }
  `]
})
export class AdOverlayComponent implements OnInit, OnDestroy {
  @Input() videoId?: number;
  @Input() adType?: AdType;

  ad: Ad | null = null;
  showAd = false;
  AdType = AdType;

  private adTimer?: number;

  constructor(private adService: AdService) {}

  ngOnInit() {
    this.loadAd();
  }

  ngOnDestroy() {
    if (this.adTimer) {
      clearTimeout(this.adTimer);
    }
  }

  private loadAd() {
    this.adService.getAd(this.videoId, this.adType).subscribe({
      next: (ad) => {
        if (ad) {
          this.ad = ad;
          this.showAd = true;
          
          // Record impression
          this.adService.recordImpression(ad.id, this.videoId).subscribe();
          
          // Auto-hide after 10 seconds for banner ads
          if (ad.type === AdType.Banner) {
            this.adTimer = window.setTimeout(() => {
              this.closeAd();
            }, 10000);
          }
        }
      },
      error: (error) => {
        console.error('Error loading ad:', error);
      }
    });
  }

  onAdClick() {
    if (this.ad) {
      // Record click
      this.adService.recordClick(this.ad.id, this.videoId).subscribe();
      
      // Open ad URL if available
      if (this.ad.url) {
        window.open(this.ad.url, '_blank');
      }
    }
  }

  closeAd() {
    this.showAd = false;
    if (this.adTimer) {
      clearTimeout(this.adTimer);
      this.adTimer = undefined;
    }
  }

  getAdClass(): string {
    if (!this.ad) return '';
    
    switch (this.ad.type) {
      case AdType.Banner:
        return 'banner-overlay';
      case AdType.Video:
        return 'video-overlay';
      case AdType.Overlay:
        return 'overlay-ad';
      default:
        return '';
    }
  }
}
