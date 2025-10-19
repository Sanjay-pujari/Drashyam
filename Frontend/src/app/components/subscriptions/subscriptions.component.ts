import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SubscriptionService } from '../../services/subscription.service';
import { ChannelService } from '../../services/channel.service';
import { VideoService } from '../../services/video.service';
import { Channel } from '../../models/channel.model';
import { Video } from '../../models/video.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTabsModule
  ],
  template: `
    <div class="subscriptions-container">
      <div class="header">
        <h1>Subscriptions</h1>
      </div>

      <div class="content" *ngIf="!isLoading; else loading">
        <mat-tab-group>
          <!-- Channels Tab -->
          <mat-tab label="Channels">
            <div class="tab-content">
              <div class="empty-state" *ngIf="subscribedChannels.length === 0">
                <mat-icon>subscriptions</mat-icon>
                <h2>No channel subscriptions</h2>
                <p>Subscribe to channels to see their latest videos here</p>
                <button mat-raised-button color="primary" routerLink="/channels">
                  <mat-icon>explore</mat-icon>
                  Browse Channels
                </button>
              </div>
              
              <div class="channels-grid" *ngIf="subscribedChannels.length > 0">
                <mat-card class="channel-card" *ngFor="let channel of subscribedChannels" (click)="viewChannel(channel.id)" style="cursor: pointer;">
                  <div class="channel-avatar">
                    <img [src]="channel.profilePictureUrl || '/assets/default-avatar.svg'" [alt]="channel.name">
                  </div>
                  <div class="channel-info">
                    <h3 class="channel-name">{{ channel.name }}</h3>
                    <p class="channel-description" *ngIf="channel.description">{{ channel.description }}</p>
                    <div class="channel-stats">
                      <span class="subscriber-count">{{ channel.subscriberCount | number }} subscribers</span>
                      <span class="video-count">{{ channel.videoCount || 0 }} videos</span>
                    </div>
                  </div>
                  <div class="channel-actions" (click)="$event.stopPropagation()">
                    <button mat-icon-button (click)="unsubscribeFromChannel(channel.id)">
                      <mat-icon>unsubscribe</mat-icon>
                    </button>
                  </div>
                </mat-card>
              </div>
            </div>
          </mat-tab>

          <!-- Latest Videos Tab -->
          <mat-tab label="Latest Videos">
            <div class="tab-content">
              <div class="empty-state" *ngIf="latestVideos.length === 0">
                <mat-icon>video_library</mat-icon>
                <h2>No videos from subscriptions</h2>
                <p>Subscribe to channels to see their latest videos here</p>
                <button mat-raised-button color="primary" routerLink="/channels">
                  <mat-icon>explore</mat-icon>
                  Browse Channels
                </button>
              </div>
              
              <div class="videos-grid" *ngIf="latestVideos.length > 0">
                <mat-card class="video-card" *ngFor="let video of latestVideos" (click)="playVideo(video.id)" style="cursor: pointer;">
                  <div class="video-thumbnail">
                    <img [src]="video.thumbnailUrl || '/assets/default-thumbnail.jpg'" [alt]="video.title">
                    <div class="duration">{{ formatDuration(video.duration) }}</div>
                    <div class="play-overlay">
                      <mat-icon>play_arrow</mat-icon>
                    </div>
                  </div>
                  <div class="video-info">
                    <h3 class="video-title">{{ video.title }}</h3>
                    <p class="channel-name">{{ video.channel?.name || (video.user?.firstName && video.user?.lastName ? (video.user?.firstName + ' ' + video.user?.lastName) : 'Unknown User') }}</p>
                    <p class="video-meta">
                      <span class="views">{{ video.viewCount | number }} views</span>
                      <span class="uploaded">{{ video.createdAt | date:'medium' }}</span>
                    </p>
                  </div>
                </mat-card>
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>
      </div>
      
      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading subscriptions...</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .subscriptions-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .header {
      margin-bottom: 30px;
    }
    
    .header h1 {
      margin: 0;
      color: #333;
    }
    
    .tab-content {
      padding: 20px 0;
    }
    
    .channels-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 20px;
    }
    
    .channel-card {
      display: flex;
      align-items: center;
      padding: 20px;
      transition: all 0.2s;
      cursor: pointer;
    }
    
    .channel-card:hover {
      box-shadow: 0 4px 8px rgba(0,0,0,0.15);
      transform: translateY(-2px);
    }
    
    .channel-avatar {
      width: 60px;
      height: 60px;
      margin-right: 16px;
      border-radius: 50%;
      overflow: hidden;
      flex-shrink: 0;
    }
    
    .channel-avatar img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    
    .channel-info {
      flex: 1;
      min-width: 0;
    }
    
    .channel-name {
      font-size: 1.2rem;
      font-weight: 600;
      margin: 0 0 8px 0;
      color: #333;
    }
    
    .channel-description {
      color: #666;
      margin: 0 0 12px 0;
      line-height: 1.4;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    
    .channel-stats {
      display: flex;
      gap: 16px;
    }
    
    .channel-stats span {
      color: #666;
      font-size: 0.9rem;
    }
    
    .channel-actions {
      display: flex;
      align-items: center;
    }
    
    .videos-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }
    
    .video-card {
      transition: all 0.2s;
      cursor: pointer;
    }
    
    .video-card:hover {
      box-shadow: 0 4px 8px rgba(0,0,0,0.15);
      transform: translateY(-2px);
    }
    
    .video-thumbnail {
      position: relative;
      width: 100%;
      height: 200px;
      border-radius: 4px;
      overflow: hidden;
    }
    
    .video-thumbnail img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    
    .duration {
      position: absolute;
      bottom: 8px;
      right: 8px;
      background: rgba(0,0,0,0.8);
      color: white;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.8rem;
    }
    
    .play-overlay {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: rgba(0,0,0,0.7);
      border-radius: 50%;
      width: 50px;
      height: 50px;
      display: flex;
      align-items: center;
      justify-content: center;
      opacity: 0;
      transition: opacity 0.2s;
    }
    
    .video-card:hover .play-overlay {
      opacity: 1;
    }
    
    .play-overlay mat-icon {
      color: white;
      font-size: 30px;
    }
    
    .video-info {
      padding: 16px;
    }
    
    .video-title {
      font-size: 1.1rem;
      font-weight: 500;
      margin: 0 0 8px 0;
      color: #333;
      line-height: 1.4;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    
    .channel-name {
      color: #666;
      margin: 0 0 8px 0;
      font-size: 0.9rem;
    }
    
    .video-meta {
      display: flex;
      gap: 16px;
      color: #666;
      font-size: 0.8rem;
      margin: 0;
    }
    
    .empty-state {
      text-align: center;
      padding: 60px 20px;
    }
    
    .empty-state mat-icon {
      font-size: 4rem;
      width: 4rem;
      height: 4rem;
      color: #ccc;
      margin-bottom: 16px;
    }
    
    .empty-state h2 {
      color: #333;
      margin-bottom: 8px;
    }
    
    .empty-state p {
      color: #666;
      margin-bottom: 24px;
    }
    
    .loading {
      text-align: center;
      padding: 40px;
    }
    
    .loading p {
      color: #666;
      margin-top: 16px;
    }
  `]
})
export class SubscriptionsComponent implements OnInit, OnDestroy {
  subscribedChannels: Channel[] = [];
  latestVideos: Video[] = [];
  isLoading = false;
  private subscriptions: Subscription[] = [];

  constructor(
    private subscriptionService: SubscriptionService,
    private channelService: ChannelService,
    private videoService: VideoService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadSubscriptions();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadSubscriptions() {
    this.isLoading = true;
    this.subscribedChannels = [];
    this.latestVideos = [];

    // Load subscribed channels
    const channelsSub = this.subscriptionService.getSubscribedChannels().subscribe({
      next: (result) => {
        this.subscribedChannels = result.items || [];
      },
      error: (error) => {
        console.error('Error loading subscribed channels:', error);
        this.snackBar.open('Error loading subscriptions', 'Close', { duration: 3000 });
      }
    });

    // Load latest videos from subscriptions
    const videosSub = this.videoService.getLatestFromSubscriptions().subscribe({
      next: (result) => {
        this.latestVideos = result.items || [];
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading latest videos:', error);
        this.latestVideos = [];
        this.isLoading = false;
      }
    });

    this.subscriptions.push(channelsSub, videosSub);
  }

  viewChannel(channelId: number) {
    this.router.navigate(['/channels', channelId]);
  }

  playVideo(videoId: number) {
    this.router.navigate(['/videos', videoId]);
  }

  unsubscribeFromChannel(channelId: number) {
    if (confirm('Are you sure you want to unsubscribe from this channel?')) {
      const sub = this.subscriptionService.unsubscribeFromChannel(channelId).subscribe({
        next: () => {
          this.subscribedChannels = this.subscribedChannels.filter(channel => channel.id !== channelId);
          this.snackBar.open('Unsubscribed from channel', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error unsubscribing from channel:', error);
          this.snackBar.open('Error unsubscribing from channel', 'Close', { duration: 3000 });
        }
      });

      this.subscriptions.push(sub);
    }
  }

  formatDuration(duration: string): string {
    // Handle duration formatting if needed
    return duration;
  }
}
