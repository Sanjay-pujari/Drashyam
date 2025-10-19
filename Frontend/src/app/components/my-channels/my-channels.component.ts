import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ChannelService } from '../../services/channel.service';
import { Channel } from '../../models/channel.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-my-channels',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="my-channels-container">
      <div class="header">
        <h1>My Channels</h1>
        <button mat-raised-button color="primary" routerLink="/channel/create">
          <mat-icon>add</mat-icon>
          Create Channel
        </button>
      </div>

      <div class="content" *ngIf="!isLoading; else loading">
        <div class="empty-state" *ngIf="channels.length === 0">
          <mat-icon>account_circle</mat-icon>
          <h2>No channels yet</h2>
          <p>Create your first channel to start sharing content</p>
          <button mat-raised-button color="primary" routerLink="/channel/create">
            <mat-icon>add</mat-icon>
            Create Channel
          </button>
        </div>
        
        <div class="channels-grid" *ngIf="channels.length > 0">
          <mat-card class="channel-card" *ngFor="let channel of channels" (click)="viewChannel(channel.id)" style="cursor: pointer;">
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
              <div class="channel-meta">
                <span class="created-at">Created {{ channel.createdAt | date:'medium' }}</span>
              </div>
            </div>
            <div class="channel-actions" (click)="$event.stopPropagation()">
              <button mat-icon-button (click)="editChannel(channel.id)">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button (click)="deleteChannel(channel.id)">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </mat-card>
        </div>
      </div>
      
      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading channels...</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .my-channels-container {
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
      margin-bottom: 8px;
    }
    
    .channel-stats span {
      color: #666;
      font-size: 0.9rem;
    }
    
    .channel-meta {
      color: #999;
      font-size: 0.8rem;
    }
    
    .channel-actions {
      display: flex;
      align-items: center;
      gap: 8px;
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
export class MyChannelsComponent implements OnInit, OnDestroy {
  channels: Channel[] = [];
  isLoading = false;
  private subscriptions: Subscription[] = [];

  constructor(
    private channelService: ChannelService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadMyChannels();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadMyChannels() {
    this.isLoading = true;
    this.channels = [];

    // Get channels created by the current user
    const sub = this.channelService.getMyChannels().subscribe({
      next: (result) => {
        this.channels = result.items || [];
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading my channels:', error);
        this.channels = [];
        this.isLoading = false;
        this.snackBar.open('Error loading channels', 'Close', { duration: 3000 });
      }
    });

    this.subscriptions.push(sub);
  }

  viewChannel(channelId: number) {
    this.router.navigate(['/channels', channelId]);
  }

  editChannel(channelId: number) {
    this.router.navigate(['/channel/edit', channelId]);
  }

  deleteChannel(channelId: number) {
    if (confirm('Are you sure you want to delete this channel?')) {
      const sub = this.channelService.deleteChannel(channelId).subscribe({
        next: () => {
          this.channels = this.channels.filter(channel => channel.id !== channelId);
          this.snackBar.open('Channel deleted', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error deleting channel:', error);
          this.snackBar.open('Error deleting channel', 'Close', { duration: 3000 });
        }
      });

      this.subscriptions.push(sub);
    }
  }
}
