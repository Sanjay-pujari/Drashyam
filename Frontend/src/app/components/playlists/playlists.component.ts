import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PlaylistService, Playlist, PlaylistVisibility } from '../../services/playlist.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-playlists',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatCardModule, MatProgressSpinnerModule, MatDialogModule],
  template: `
    <div class="playlists-container">
      <div class="header">
        <h1>Playlists</h1>
        <button mat-raised-button color="primary" (click)="createPlaylist()">
          <mat-icon>add</mat-icon>
          Create Playlist
        </button>
      </div>
      
      <div class="content" *ngIf="!isLoading; else loading">
        <div class="empty-state" *ngIf="playlists.length === 0">
          <mat-icon>playlist_play</mat-icon>
          <h2>No playlists yet</h2>
          <p>Create your first playlist to organize your favorite videos</p>
          <button mat-raised-button color="primary" (click)="createPlaylist()">
            <mat-icon>add</mat-icon>
            Create Playlist
          </button>
        </div>
        
        <div class="playlists-grid" *ngIf="playlists.length > 0">
          <mat-card class="playlist-card" *ngFor="let playlist of playlists" (click)="viewPlaylist(playlist.id)">
            <div class="playlist-thumbnail">
              <img [src]="playlist.thumbnailUrl" [alt]="playlist.name" *ngIf="playlist.thumbnailUrl">
              <div class="video-count" *ngIf="!playlist.thumbnailUrl">
                <mat-icon>playlist_play</mat-icon>
                <span>{{ playlist.videoCount }} videos</span>
              </div>
            </div>
            <div class="playlist-info">
              <h3 class="playlist-name">{{ playlist.name }}</h3>
              <p class="playlist-description" *ngIf="playlist.description">{{ playlist.description }}</p>
              <p class="video-count">{{ playlist.videoCount }} videos</p>
              <p class="created-at">Created {{ playlist.createdAt | date:'medium' }}</p>
            </div>
            <div class="actions" (click)="$event.stopPropagation()">
              <button mat-icon-button (click)="editPlaylist(playlist.id)">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button (click)="deletePlaylist(playlist.id)">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </mat-card>
        </div>
      </div>
      
      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading playlists...</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .playlists-container {
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
    
    .playlists-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }
    
    .playlist-card {
      overflow: hidden;
    }
    
    .playlist-thumbnail {
      position: relative;
      width: 100%;
      height: 200px;
      background: #f5f5f5;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .playlist-thumbnail img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    
    .video-count {
      display: flex;
      flex-direction: column;
      align-items: center;
      color: #666;
    }
    
    .video-count mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 8px;
    }
    
    .playlist-info {
      padding: 16px;
    }
    
    .playlist-name {
      font-size: 16px;
      font-weight: 500;
      margin: 0 0 8px 0;
      color: #333;
    }
    
    .playlist-description {
      color: #666;
      margin: 0 0 8px 0;
      font-size: 14px;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    
    .video-count {
      color: #999;
      margin: 0 0 4px 0;
      font-size: 14px;
    }
    
    .created-at {
      color: #999;
      margin: 0;
      font-size: 12px;
    }
    
    .actions {
      display: flex;
      gap: 8px;
      padding: 16px;
      border-top: 1px solid #eee;
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
export class PlaylistsComponent implements OnInit, OnDestroy {
  isLoading = false;
  playlists: Playlist[] = [];
  private subscriptions: Subscription[] = [];

  constructor(
    private playlistService: PlaylistService,
    private router: Router,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadPlaylists();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadPlaylists() {
    this.isLoading = true;
    this.playlists = [];

    const sub = this.playlistService.getPlaylists(1, 50).subscribe({
      next: (result) => {
        this.playlists = result.items || [];
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading playlists:', error);
        this.playlists = [];
        this.isLoading = false;
        this.snackBar.open('Error loading playlists', 'Close', { duration: 3000 });
      }
    });

    this.subscriptions.push(sub);
  }

  createPlaylist() {
    // TODO: Open create playlist dialog
    console.log('Create playlist clicked');
    this.snackBar.open('Create playlist functionality coming soon', 'Close', { duration: 3000 });
  }

  editPlaylist(id: number) {
    // TODO: Open edit playlist dialog
    console.log('Edit playlist:', id);
    this.snackBar.open('Edit playlist functionality coming soon', 'Close', { duration: 3000 });
  }

  deletePlaylist(id: number) {
    if (confirm('Are you sure you want to delete this playlist?')) {
      const sub = this.playlistService.deletePlaylist(id).subscribe({
        next: () => {
          this.playlists = this.playlists.filter(playlist => playlist.id !== id);
          this.snackBar.open('Playlist deleted', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error deleting playlist:', error);
          this.snackBar.open('Error deleting playlist', 'Close', { duration: 3000 });
        }
      });

      this.subscriptions.push(sub);
    }
  }

  viewPlaylist(id: number) {
    this.router.navigate(['/playlists', id]);
  }
}
