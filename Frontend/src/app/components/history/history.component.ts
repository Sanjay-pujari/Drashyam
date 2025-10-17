import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  template: `
    <div class="history-container">
      <div class="header">
        <h1>Watch History</h1>
        <button mat-button color="primary" (click)="clearHistory()">
          <mat-icon>clear_all</mat-icon>
          Clear All
        </button>
      </div>
      
      <div class="content" *ngIf="!isLoading; else loading">
        <div class="empty-state" *ngIf="historyItems.length === 0">
          <mat-icon>history</mat-icon>
          <h2>No watch history yet</h2>
          <p>Videos you watch will appear here</p>
          <button mat-raised-button color="primary" routerLink="/">
            <mat-icon>home</mat-icon>
            Browse Videos
          </button>
        </div>
        
        <div class="history-list" *ngIf="historyItems.length > 0">
          <mat-card class="history-item" *ngFor="let item of historyItems">
            <div class="video-thumbnail">
              <img [src]="item.thumbnailUrl" [alt]="item.title">
              <div class="duration">{{ item.duration }}</div>
            </div>
            <div class="video-info">
              <h3 class="video-title">{{ item.title }}</h3>
              <p class="channel-name">{{ item.channelName }}</p>
              <p class="views">{{ item.views | number }} views</p>
              <p class="watched-at">Watched {{ item.watchedAt | date:'medium' }}</p>
            </div>
            <div class="actions">
              <button mat-icon-button (click)="removeFromHistory(item.id)">
                <mat-icon>close</mat-icon>
              </button>
            </div>
          </mat-card>
        </div>
      </div>
      
      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading history...</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .history-container {
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
    
    .history-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    
    .history-item {
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
    
    .watched-at {
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
export class HistoryComponent implements OnInit {
  isLoading = false;
  historyItems: any[] = [];

  ngOnInit() {
    this.loadHistory();
  }

  loadHistory() {
    this.isLoading = true;
    // TODO: Implement actual history loading from API
    setTimeout(() => {
      this.historyItems = [];
      this.isLoading = false;
    }, 1000);
  }

  clearHistory() {
    // TODO: Implement clear history functionality
    console.log('Clear history clicked');
  }

  removeFromHistory(id: string) {
    // TODO: Implement remove from history functionality
    console.log('Remove from history:', id);
  }
}
