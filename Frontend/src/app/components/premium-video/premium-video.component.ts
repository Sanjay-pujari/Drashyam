import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

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

@Component({
  selector: 'app-premium-video',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="premium-video-card" *ngIf="premiumVideo">
      <div class="video-thumbnail">
        <img [src]="premiumVideo.videoThumbnailUrl" [alt]="premiumVideo.videoTitle">
        <div class="premium-badge">Premium</div>
        <div class="price-badge">{{ premiumVideo.currency }} {{ premiumVideo.price }}</div>
      </div>
      
      <div class="video-info">
        <h3>{{ premiumVideo.videoTitle }}</h3>
        <p class="creator">by {{ premiumVideo.creatorName }}</p>
        
        <div class="purchase-section" *ngIf="!hasPurchased">
          <button class="purchase-btn" (click)="purchaseVideo()" [disabled]="isPurchasing">
            {{ isPurchasing ? 'Processing...' : 'Purchase for ' + premiumVideo.currency + ' ' + premiumVideo.price }}
          </button>
        </div>
        
        <div class="purchased-section" *ngIf="hasPurchased">
          <div class="purchased-badge">✓ Purchased</div>
          <button class="watch-btn" (click)="watchVideo()">Watch Now</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .premium-video-card {
      border: 2px solid #ffd700;
      border-radius: 12px;
      overflow: hidden;
      background: linear-gradient(135deg, #fff8e1 0%, #ffffff 100%);
      box-shadow: 0 4px 12px rgba(255, 215, 0, 0.2);
      transition: transform 0.2s ease;
    }

    .premium-video-card:hover {
      transform: translateY(-2px);
    }

    .video-thumbnail {
      position: relative;
      width: 100%;
      height: 200px;
      overflow: hidden;
    }

    .video-thumbnail img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .premium-badge {
      position: absolute;
      top: 8px;
      left: 8px;
      background: linear-gradient(45deg, #ffd700, #ffed4e);
      color: #333;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: bold;
      text-transform: uppercase;
    }

    .price-badge {
      position: absolute;
      top: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.8);
      color: white;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 14px;
      font-weight: bold;
    }

    .video-info {
      padding: 16px;
    }

    .video-info h3 {
      margin: 0 0 8px 0;
      font-size: 16px;
      font-weight: 600;
      color: #333;
      line-height: 1.3;
    }

    .creator {
      margin: 0 0 16px 0;
      color: #666;
      font-size: 14px;
    }

    .purchase-section {
      text-align: center;
    }

    .purchase-btn {
      background: linear-gradient(45deg, #ffd700, #ffed4e);
      color: #333;
      border: none;
      padding: 12px 24px;
      border-radius: 6px;
      font-weight: bold;
      cursor: pointer;
      transition: all 0.2s ease;
      width: 100%;
    }

    .purchase-btn:hover:not(:disabled) {
      background: linear-gradient(45deg, #ffed4e, #ffd700);
      transform: translateY(-1px);
    }

    .purchase-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .purchased-section {
      text-align: center;
    }

    .purchased-badge {
      background: #4caf50;
      color: white;
      padding: 8px 16px;
      border-radius: 20px;
      font-size: 14px;
      font-weight: bold;
      margin-bottom: 12px;
      display: inline-block;
    }

    .watch-btn {
      background: #2196f3;
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 6px;
      font-weight: bold;
      cursor: pointer;
      transition: background 0.2s ease;
      width: 100%;
    }

    .watch-btn:hover {
      background: #1976d2;
    }
  `]
})
export class PremiumVideoComponent implements OnInit {
  @Input() videoId!: number;
  
  premiumVideo: PremiumVideo | null = null;
  hasPurchased = false;
  isPurchasing = false;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadPremiumVideo();
    this.checkPurchaseStatus();
  }

  private loadPremiumVideo() {
    this.http.get<PremiumVideo>(`${environment.apiUrl}/api/premiumcontent/videos/${this.videoId}`).subscribe({
      next: (video) => {
        this.premiumVideo = video;
      },
      error: (error) => {
        console.error('Error loading premium video:', error);
      }
    });
  }

  private checkPurchaseStatus() {
    this.http.get<boolean>(`${environment.apiUrl}/api/premiumcontent/videos/${this.videoId}/has-purchased`).subscribe({
      next: (purchased) => {
        this.hasPurchased = purchased;
      },
      error: (error) => {
        console.error('Error checking purchase status:', error);
      }
    });
  }

  purchaseVideo() {
    if (!this.premiumVideo || this.isPurchasing) return;

    this.isPurchasing = true;
    
    // In a real implementation, you would integrate with Stripe or another payment processor
    // For now, we'll simulate the purchase process
    const purchaseData = {
      premiumVideoId: this.premiumVideo.id,
      paymentIntentId: 'simulated_payment_intent_' + Date.now()
    };

    this.http.post<PremiumPurchase>(`${environment.apiUrl}/api/premiumcontent/purchases`, purchaseData).subscribe({
      next: (purchase) => {
        // Simulate payment completion
        setTimeout(() => {
          this.completePurchase(purchase.paymentIntentId);
        }, 2000);
      },
      error: (error) => {
        console.error('Error creating purchase:', error);
        this.isPurchasing = false;
        alert('Purchase failed. Please try again.');
      }
    });
  }

  private completePurchase(paymentIntentId: string) {
    this.http.post(`${environment.apiUrl}/api/premiumcontent/purchases/${paymentIntentId}/complete`, {}).subscribe({
      next: () => {
        this.hasPurchased = true;
        this.isPurchasing = false;
        alert('Purchase completed successfully!');
      },
      error: (error) => {
        console.error('Error completing purchase:', error);
        this.isPurchasing = false;
        alert('Purchase completion failed. Please contact support.');
      }
    });
  }

  watchVideo() {
    // Navigate to video player
    window.location.href = `/videos/${this.premiumVideo?.videoId}`;
  }
}
