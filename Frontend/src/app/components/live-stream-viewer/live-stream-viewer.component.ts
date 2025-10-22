import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { SuperChatButtonComponent } from '../super-chat-button/super-chat-button.component';
import { SuperChatDisplayComponent } from '../super-chat-display/super-chat-display.component';
import { LiveStreamService } from '../../services/live-stream.service';
import { LiveStream } from '../../models/live-stream.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-live-stream-viewer',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    SuperChatButtonComponent,
    SuperChatDisplayComponent
  ],
  template: `
    <div class="live-stream-viewer" *ngIf="liveStream; else loading">
      <!-- Stream Header -->
      <div class="stream-header">
        <div class="stream-info">
          <h1 class="stream-title">{{ liveStream.title }}</h1>
          <div class="stream-meta">
            <span class="viewer-count">
              <mat-icon>visibility</mat-icon>
              {{ liveStream.viewerCount | number }} watching
            </span>
            <span class="stream-status" [class.live]="liveStream.status === 'Live'">
              <mat-icon>fiber_manual_record</mat-icon>
              {{ liveStream.status }}
            </span>
          </div>
        </div>
        <div class="stream-actions">
          <app-super-chat-button 
            [liveStreamId]="liveStream.id"
            [liveStreamTitle]="liveStream.title"
            (superChatSent)="onSuperChatSent($event)">
          </app-super-chat-button>
        </div>
      </div>

      <!-- Video Player Area -->
      <div class="video-container">
        <div class="video-player">
          <video 
            #videoPlayer
            [src]="liveStream.streamUrl"
            controls
            autoplay
            muted
            class="stream-video">
            Your browser does not support the video tag.
          </video>
        </div>
        
        <!-- Super Chat Display -->
        <div class="super-chat-sidebar" *ngIf="liveStream.isMonetized">
          <app-super-chat-display [liveStreamId]="liveStream.id"></app-super-chat-display>
        </div>
      </div>

      <!-- Stream Description -->
      <div class="stream-description" *ngIf="liveStream.description">
        <h3>About this stream</h3>
        <p>{{ liveStream.description }}</p>
      </div>

      <!-- Stream Stats -->
      <div class="stream-stats">
        <div class="stat-item">
          <mat-icon>visibility</mat-icon>
          <span>{{ liveStream.viewerCount | number }} viewers</span>
        </div>
        <div class="stat-item">
          <mat-icon>schedule</mat-icon>
          <span>Started {{ formatStartTime(liveStream.actualStartTime) }}</span>
        </div>
        <div class="stat-item" *ngIf="liveStream.isMonetized">
          <mat-icon>monetization_on</mat-icon>
          <span>Monetized</span>
        </div>
      </div>
    </div>

    <ng-template #loading>
      <div class="loading-container">
        <mat-spinner></mat-spinner>
        <p>Loading live stream...</p>
      </div>
    </ng-template>
  `,
  styles: [`
    .live-stream-viewer {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }

    .stream-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 20px;
      padding: 20px;
      background: #f8f9fa;
      border-radius: 8px;
    }

    .stream-info {
      flex: 1;
    }

    .stream-title {
      font-size: 24px;
      font-weight: 600;
      margin: 0 0 12px 0;
      color: #333;
    }

    .stream-meta {
      display: flex;
      gap: 20px;
      align-items: center;
    }

    .viewer-count,
    .stream-status {
      display: flex;
      align-items: center;
      gap: 6px;
      color: #666;
      font-size: 14px;
    }

    .stream-status.live {
      color: #f44336;
      font-weight: 500;
    }

    .stream-status.live mat-icon {
      animation: pulse 2s infinite;
    }

    @keyframes pulse {
      0% { opacity: 1; }
      50% { opacity: 0.5; }
      100% { opacity: 1; }
    }

    .video-container {
      display: grid;
      grid-template-columns: 1fr 300px;
      gap: 20px;
      margin-bottom: 20px;
    }

    .video-player {
      position: relative;
      background: #000;
      border-radius: 8px;
      overflow: hidden;
    }

    .stream-video {
      width: 100%;
      height: 400px;
      object-fit: cover;
    }

    .super-chat-sidebar {
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .stream-description {
      background: #fff;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 20px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .stream-description h3 {
      margin: 0 0 12px 0;
      color: #333;
    }

    .stream-description p {
      margin: 0;
      color: #666;
      line-height: 1.6;
    }

    .stream-stats {
      display: flex;
      gap: 24px;
      padding: 16px;
      background: #f8f9fa;
      border-radius: 8px;
    }

    .stat-item {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #666;
      font-size: 14px;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
    }

    .loading-container p {
      margin-top: 16px;
      color: #666;
    }

    @media (max-width: 768px) {
      .video-container {
        grid-template-columns: 1fr;
      }
      
      .stream-header {
        flex-direction: column;
        gap: 16px;
      }
      
      .stream-meta {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }
    }
  `]
})
export class LiveStreamViewerComponent implements OnInit, OnDestroy {
  @Input() liveStreamId!: number;
  
  liveStream: LiveStream | null = null;
  private subscriptions: Subscription[] = [];

  constructor(
    private liveStreamService: LiveStreamService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.loadLiveStream();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadLiveStream() {
    const sub = this.liveStreamService.getLiveStreamById(this.liveStreamId).subscribe({
      next: (liveStream) => {
        this.liveStream = liveStream;
      },
      error: (error) => {
        console.error('Error loading live stream:', error);
        this.snackBar.open('Failed to load live stream', 'Close', { duration: 3000 });
      }
    });
    this.subscriptions.push(sub);
  }

  onSuperChatSent(superChat: any) {
    this.snackBar.open('Super Chat sent successfully!', 'Close', { duration: 3000 });
    // The SuperChatDisplayComponent will automatically refresh to show the new super chat
  }

  formatStartTime(startTime?: string): string {
    if (!startTime) return 'Unknown';
    
    const start = new Date(startTime);
    const now = new Date();
    const diffMs = now.getTime() - start.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 60) return `${diffMins} minutes ago`;
    
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;
    
    return start.toLocaleDateString();
  }
}
