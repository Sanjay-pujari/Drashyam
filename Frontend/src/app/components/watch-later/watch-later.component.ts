import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-watch-later',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  template: `
    <div class="watch-later-container">
      <div class="header">
        <h1>Watch Later</h1>
        <button mat-button color="primary" (click)="clearWatchLater()">
          <mat-icon>clear_all</mat-icon>
          Clear All
        </button>
      </div>
      
      <div class="content" *ngIf="!isLoading; else loading">
        <div class="empty-state" *ngIf="watchLaterItems.length === 0">
          <mat-icon>watch_later</mat-icon>
          <h2>No videos in watch later</h2>
          <p>Videos you save to watch later will appear here</p>
          <button mat-raised-button color="primary" routerLink="/">
            <mat-icon>home</mat-icon>
            Browse Videos
          </button>
        </div>
        
        <div class="watch-later-list" *ngIf="watchLaterItems.length > 0">
          <mat-card class="watch-later-item" *ngFor="let item of watchLaterItems">
            <div class="video-thumbnail">
              <img [src]="item.thumbnailUrl" [alt]="item.title">
              <div class="duration">{{ item.duration }}</div>
              <button mat-icon-button class="remove-button" (click)="removeFromWatchLater(item.id)">
                <mat-icon>close</mat-icon>
              </button>
            </div>
            <div class="video-info">
              <h3 class="video-title">{{ item.title }}</h3>
              <p class="channel-name">{{ item.channelName }}</p>
              <p class="views">{{ item.views | number }} views</p>
              <p class="added-at">Added {{ item.addedAt | date:'medium' }}</p>
            </div>
            <div class="actions">
              <button mat-raised-button color="primary" (click)="watchVideo(item.id)">
                <mat-icon>play_arrow</mat-icon>
                Watch Now
              </button>
            </div>
          </mat-card>
        </div>
      </div>
      
      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading watch later...</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .watch-later-container {
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
    
    .watch-later-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    
    .watch-later-item {
      display: flex;
      align-items: center;
      padding: 16px;
      gap: 16px;
    }
    
    .video-thumbnail {
      position: relative;
      width: 200px;
      height: 112px;
      flex-shrink: 0;
    }
    
    .video-thumbnail img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      border-radius: 8px;
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
    
    .remove-button {
      position: absolute;
      top: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.6);
      color: white;
    }
    
    .video-info {
      flex: 1;
    }
    
    .video-title {
      font-size: 16px;
      font-weight: 500;
      margin: 0 0 8px 0;
      color: #333;
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
    
    .added-at {
      color: #999;
      margin: 0;
      font-size: 12px;
    }
    
    .actions {
      display: flex;
      gap: 8px;
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
export class WatchLaterComponent implements OnInit {
  isLoading = false;
  watchLaterItems: any[] = [];

  ngOnInit() {
    this.loadWatchLater();
  }

  loadWatchLater() {
    this.isLoading = true;
    // TODO: Implement actual watch later loading from API
    setTimeout(() => {
      this.watchLaterItems = [];
      this.isLoading = false;
    }, 1000);
  }

  clearWatchLater() {
    // TODO: Implement clear watch later functionality
    console.log('Clear watch later clicked');
  }

  removeFromWatchLater(id: string) {
    // TODO: Implement remove from watch later functionality
    console.log('Remove from watch later:', id);
  }

  watchVideo(id: string) {
    // TODO: Implement watch video functionality
    console.log('Watch video:', id);
  }
}
