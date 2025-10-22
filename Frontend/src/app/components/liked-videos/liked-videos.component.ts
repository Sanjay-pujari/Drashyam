import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { VideoService } from '../../services/video.service';
import { Video } from '../../models/video.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-liked-videos',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  template: `
    <div class="liked-videos-container">
      <div class="header">
        <h1>Liked Videos</h1>
        <button mat-button color="primary" (click)="clearLikedVideos()">
          <mat-icon>clear_all</mat-icon>
          Clear All
        </button>
      </div>
      
      <div class="content" *ngIf="!isLoading; else loading">
        <div class="empty-state" *ngIf="likedVideos.length === 0">
          <mat-icon>thumb_up</mat-icon>
          <h2>No liked videos yet</h2>
          <p>Videos you like will appear here</p>
          <button mat-raised-button color="primary" routerLink="/">
            <mat-icon>home</mat-icon>
            Browse Videos
          </button>
        </div>
        
        <div class="videos-grid" *ngIf="likedVideos.length > 0">
          <mat-card class="video-card" *ngFor="let video of likedVideos" (click)="onVideoClick(video)">
            <div class="video-thumbnail">
              <img [src]="video.thumbnailUrl || '/assets/default-video-thumbnail.svg'" [alt]="video.title">
              <div class="duration">{{ formatDuration(video.duration) }}</div>
              <button mat-icon-button class="like-button liked" (click)="$event.stopPropagation(); unlikeVideo(video.id)">
                <mat-icon>thumb_up</mat-icon>
              </button>
            </div>
            <div class="video-info">
              <h3 class="video-title">{{ video.title }}</h3>
              <p class="channel-name" (click)="$event.stopPropagation(); onChannelClick(video.channelId!)">
                {{ video.channel?.name || (video.user?.firstName && video.user?.lastName ? (video.user?.firstName + ' ' + video.user?.lastName) : 'Unknown User') }}
              </p>
              <p class="views">{{ video.viewCount | number }} views</p>
              <p class="liked-at">Liked {{ video.createdAt | date:'medium' }}</p>
            </div>
          </mat-card>
        </div>
      </div>
      
      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading liked videos...</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .liked-videos-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
    }
    
    .header h1 {
      margin: 0;
      color: #333;
    }
    
    .empty-state {
      text-align: center;
      padding: 60px 20px;
    }
    
    .empty-state mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #ccc;
      margin-bottom: 20px;
    }
    
    .empty-state h2 {
      color: #666;
      margin-bottom: 10px;
    }
    
    .empty-state p {
      color: #999;
      margin-bottom: 30px;
    }
    
    .videos-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }
    
    .video-card {
      overflow: hidden;
    }
    
    .video-thumbnail {
      position: relative;
      width: 100%;
      height: 200px;
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
      background: rgba(0, 0, 0, 0.8);
      color: white;
      padding: 2px 6px;
      border-radius: 4px;
      font-size: 12px;
    }
    
    .like-button {
      position: absolute;
      top: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.6);
      color: white;
    }
    
    .like-button.liked {
      background: #1976d2;
    }
    
    .video-info {
      padding: 16px;
    }
    
    .video-title {
      font-size: 16px;
      font-weight: 500;
      margin: 0 0 8px 0;
      color: #333;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    
    .channel-name {
      color: #666;
      margin: 0 0 4px 0;
      font-size: 14px;
    }
    
    .views {
      color: #999;
      margin: 0 0 4px 0;
      font-size: 14px;
    }
    
    .liked-at {
      color: #999;
      margin: 0;
      font-size: 12px;
    }
    
    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
    }
    
    .loading p {
      margin-top: 20px;
      color: #666;
    }
  `]
})
export class LikedVideosComponent implements OnInit, OnDestroy {
  isLoading = false;
  likedVideos: Video[] = [];
  private subscriptions: Subscription[] = [];

  constructor(
    private videoService: VideoService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadLikedVideos();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadLikedVideos() {
    this.isLoading = true;
    this.likedVideos = [];

    console.log('Loading liked videos...');
    const sub = this.videoService.getLikedVideos({ page: 1, pageSize: 50 }).subscribe({
      next: (result) => {
        console.log('Liked videos API response:', result);
        this.likedVideos = result.items || [];
        console.log('Liked videos loaded:', this.likedVideos.length, 'videos');
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading liked videos:', error);
        this.likedVideos = [];
        this.isLoading = false;
      }
    });

    this.subscriptions.push(sub);
  }

  clearLikedVideos() {
    // TODO: Implement clear all liked videos functionality
    console.log('Clear liked videos clicked');
    // This would require a backend endpoint to clear all liked videos
  }

  unlikeVideo(videoId: number) {
    const sub = this.videoService.likeVideo(videoId, 'dislike').subscribe({
      next: (video) => {
        // Remove the video from the liked videos list
        this.likedVideos = this.likedVideos.filter(v => v.id !== videoId);
        console.log('Video unliked successfully');
      },
      error: (error) => {
        console.error('Error unliking video:', error);
      }
    });

    this.subscriptions.push(sub);
  }

  onVideoClick(video: Video) {
    this.router.navigate(['/videos', video.id]);
  }

  onChannelClick(channelId: number) {
    this.router.navigate(['/channels', channelId]);
  }

  formatDuration(duration: string | any): string {
    // Handle both string and TimeSpan object from backend
    let seconds: number;
    
    if (typeof duration === 'string') {
      seconds = parseInt(duration);
    } else if (duration && typeof duration === 'object') {
      seconds = Math.floor(duration.ticks / 10000000);
    } else {
      seconds = 0;
    }
    
    if (isNaN(seconds) || seconds === 0) {
      return '0:00';
    }

    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    } else {
      return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }
  }
}
