import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

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
          <mat-card class="video-card" *ngFor="let video of likedVideos">
            <div class="video-thumbnail">
              <img [src]="video.thumbnailUrl" [alt]="video.title">
              <div class="duration">{{ video.duration }}</div>
              <button mat-icon-button class="like-button liked" (click)="unlikeVideo(video.id)">
                <mat-icon>thumb_up</mat-icon>
              </button>
            </div>
            <div class="video-info">
              <h3 class="video-title">{{ video.title }}</h3>
              <p class="channel-name">{{ video.channelName }}</p>
              <p class="views">{{ video.views | number }} views</p>
              <p class="liked-at">Liked {{ video.likedAt | date:'medium' }}</p>
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
export class LikedVideosComponent implements OnInit {
  isLoading = false;
  likedVideos: any[] = [];

  ngOnInit() {
    this.loadLikedVideos();
  }

  loadLikedVideos() {
    this.isLoading = true;
    // TODO: Implement actual liked videos loading from API
    setTimeout(() => {
      this.likedVideos = [];
      this.isLoading = false;
    }, 1000);
  }

  clearLikedVideos() {
    // TODO: Implement clear liked videos functionality
    console.log('Clear liked videos clicked');
  }

  unlikeVideo(id: string) {
    // TODO: Implement unlike video functionality
    console.log('Unlike video:', id);
  }
}
