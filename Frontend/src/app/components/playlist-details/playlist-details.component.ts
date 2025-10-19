import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PlaylistService, Playlist, PlaylistVideo } from '../../services/playlist.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-playlist-details',
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
    <div class="playlist-details-container">
      <div class="playlist-header" *ngIf="playlist">
        <div class="playlist-info">
          <h1 class="playlist-title">{{ playlist.name }}</h1>
          <p class="playlist-description" *ngIf="playlist.description">{{ playlist.description }}</p>
          <div class="playlist-meta">
            <span class="video-count">{{ playlist.videoCount }} videos</span>
            <span class="created-at">Created {{ playlist.createdAt | date:'medium' }}</span>
          </div>
        </div>
        <div class="playlist-actions">
          <button mat-raised-button color="primary" (click)="playAll()">
            <mat-icon>play_arrow</mat-icon>
            Play All
          </button>
          <button mat-button (click)="editPlaylist()">
            <mat-icon>edit</mat-icon>
            Edit
          </button>
          <button mat-button (click)="deletePlaylist()">
            <mat-icon>delete</mat-icon>
            Delete
          </button>
        </div>
      </div>

      <div class="playlist-content" *ngIf="!isLoading; else loading">
        <div class="empty-state" *ngIf="playlistVideos.length === 0">
          <mat-icon>playlist_play</mat-icon>
          <h2>No videos in this playlist</h2>
          <p>Add videos to this playlist to get started</p>
          <button mat-raised-button color="primary" routerLink="/">
            <mat-icon>add</mat-icon>
            Browse Videos
          </button>
        </div>
        
        <div class="videos-list" *ngIf="playlistVideos.length > 0">
          <div class="video-item" *ngFor="let video of playlistVideos; let i = index" (click)="playVideo(video.videoId)">
            <div class="video-thumbnail">
              <img [src]="video.videoThumbnailUrl || '/assets/default-thumbnail.jpg'" [alt]="video.videoTitle">
              <div class="duration">{{ formatDuration(video.videoDuration) }}</div>
              <div class="play-overlay">
                <mat-icon>play_arrow</mat-icon>
              </div>
            </div>
            <div class="video-info">
              <h3 class="video-title">{{ video.videoTitle }}</h3>
              <p class="channel-name">{{ video.channelName }}</p>
              <p class="video-meta">
                <span class="views">{{ video.videoViewCount | number }} views</span>
                <span class="added-at">Added {{ video.addedAt | date:'medium' }}</span>
              </p>
            </div>
            <div class="video-actions" (click)="$event.stopPropagation()">
              <button mat-icon-button (click)="removeFromPlaylist(video.videoId)">
                <mat-icon>remove_circle</mat-icon>
              </button>
            </div>
          </div>
        </div>
      </div>
      
      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading playlist...</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .playlist-details-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .playlist-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 30px;
      padding: 20px;
      background: #f8f9fa;
      border-radius: 8px;
    }
    
    .playlist-info {
      flex: 1;
    }
    
    .playlist-title {
      font-size: 2rem;
      font-weight: 600;
      margin: 0 0 8px 0;
      color: #333;
    }
    
    .playlist-description {
      color: #666;
      margin: 0 0 16px 0;
      line-height: 1.5;
    }
    
    .playlist-meta {
      display: flex;
      gap: 20px;
      color: #666;
      font-size: 0.9rem;
    }
    
    .playlist-actions {
      display: flex;
      gap: 12px;
      align-items: center;
    }
    
    .videos-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    
    .video-item {
      display: flex;
      align-items: center;
      padding: 16px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      cursor: pointer;
      transition: all 0.2s;
    }
    
    .video-item:hover {
      box-shadow: 0 4px 8px rgba(0,0,0,0.15);
      transform: translateY(-2px);
    }
    
    .video-thumbnail {
      position: relative;
      width: 200px;
      height: 112px;
      margin-right: 16px;
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
      bottom: 4px;
      right: 4px;
      background: rgba(0,0,0,0.8);
      color: white;
      padding: 2px 6px;
      border-radius: 3px;
      font-size: 0.8rem;
    }
    
    .play-overlay {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: rgba(0,0,0,0.7);
      border-radius: 50%;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      opacity: 0;
      transition: opacity 0.2s;
    }
    
    .video-item:hover .play-overlay {
      opacity: 1;
    }
    
    .play-overlay mat-icon {
      color: white;
      font-size: 24px;
    }
    
    .video-info {
      flex: 1;
    }
    
    .video-title {
      font-size: 1.1rem;
      font-weight: 500;
      margin: 0 0 8px 0;
      color: #333;
      line-height: 1.4;
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
    
    .video-actions {
      display: flex;
      align-items: center;
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
export class PlaylistDetailsComponent implements OnInit, OnDestroy {
  playlist: Playlist | null = null;
  playlistVideos: PlaylistVideo[] = [];
  isLoading = false;
  private subscriptions: Subscription[] = [];

  constructor(
    private playlistService: PlaylistService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      const playlistId = +params['id'];
      if (playlistId) {
        this.loadPlaylist(playlistId);
        this.loadPlaylistVideos(playlistId);
      }
    });
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadPlaylist(playlistId: number) {
    this.isLoading = true;
    const sub = this.playlistService.getPlaylistById(playlistId).subscribe({
      next: (playlist: Playlist) => {
        this.playlist = playlist;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading playlist:', error);
        this.snackBar.open('Error loading playlist', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
    this.subscriptions.push(sub);
  }

  loadPlaylistVideos(playlistId: number) {
    const sub = this.playlistService.getPlaylistVideos(playlistId, 1, 100).subscribe({
      next: (result: any) => {
        this.playlistVideos = result.items || [];
      },
      error: (error: any) => {
        console.error('Error loading playlist videos:', error);
        this.snackBar.open('Error loading playlist videos', 'Close', { duration: 3000 });
      }
    });
    this.subscriptions.push(sub);
  }

  playVideo(videoId: number) {
    this.router.navigate(['/videos', videoId]);
  }

  playAll() {
    if (this.playlistVideos.length > 0) {
      this.playVideo(this.playlistVideos[0].videoId);
    }
  }

  editPlaylist() {
    // TODO: Implement edit playlist functionality
    this.snackBar.open('Edit playlist functionality coming soon', 'Close', { duration: 3000 });
  }

  deletePlaylist() {
    if (!this.playlist) return;
    
    if (confirm('Are you sure you want to delete this playlist?')) {
      const sub = this.playlistService.deletePlaylist(this.playlist.id).subscribe({
        next: () => {
          this.snackBar.open('Playlist deleted', 'Close', { duration: 3000 });
          this.router.navigate(['/playlists']);
        },
        error: (error: any) => {
          console.error('Error deleting playlist:', error);
          this.snackBar.open('Error deleting playlist', 'Close', { duration: 3000 });
        }
      });
      this.subscriptions.push(sub);
    }
  }

  removeFromPlaylist(videoId: number) {
    if (!this.playlist) return;
    
    const sub = this.playlistService.removeVideoFromPlaylist(this.playlist.id, videoId).subscribe({
      next: () => {
        this.playlistVideos = this.playlistVideos.filter(v => v.videoId !== videoId);
        if (this.playlist) {
          this.playlist.videoCount--;
        }
        this.snackBar.open('Removed from playlist', 'Close', { duration: 3000 });
      },
      error: (error: any) => {
        console.error('Error removing from playlist:', error);
        this.snackBar.open('Error removing from playlist', 'Close', { duration: 3000 });
      }
    });
    this.subscriptions.push(sub);
  }

  formatDuration(duration: string): string {
    // Handle duration formatting if needed
    return duration;
  }
}
