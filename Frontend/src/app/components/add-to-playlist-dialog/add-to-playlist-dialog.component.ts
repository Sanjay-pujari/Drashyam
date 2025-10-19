import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { PlaylistService, Playlist } from '../../services/playlist.service';
import { CreatePlaylistDialogComponent } from '../create-playlist-dialog/create-playlist-dialog.component';

export interface AddToPlaylistDialogData {
  videoId: number;
  videoPlaylistStatus?: { [playlistId: number]: boolean };
}

@Component({
  selector: 'app-add-to-playlist-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatListModule,
    MatIconModule
  ],
  template: `
    <div class="add-to-playlist-dialog">
      <h2 mat-dialog-title>Add to Playlist</h2>
      
      <mat-dialog-content>
        <div class="playlists-list" *ngIf="playlists.length > 0; else noPlaylists">
          <mat-list>
            <mat-list-item 
              *ngFor="let playlist of playlists" 
              (click)="togglePlaylist(playlist.id)"
              class="playlist-item"
              [class.in-playlist]="isVideoInPlaylist(playlist.id)">
              <mat-icon matListItemIcon>
                {{ isVideoInPlaylist(playlist.id) ? 'check_circle' : 'playlist_play' }}
              </mat-icon>
              <div matListItemTitle>{{ playlist.name }}</div>
              <div matListItemLine>{{ playlist.videoCount }} videos</div>
              <mat-icon class="toggle-icon">
                {{ isVideoInPlaylist(playlist.id) ? 'remove' : 'add' }}
              </mat-icon>
            </mat-list-item>
          </mat-list>
        </div>
        
        <ng-template #noPlaylists>
          <div class="no-playlists">
            <mat-icon>playlist_play</mat-icon>
            <p>No playlists yet</p>
            <button mat-raised-button color="primary" (click)="createNewPlaylist()">
              <mat-icon>add</mat-icon>
              Create Playlist
            </button>
          </div>
        </ng-template>
      </mat-dialog-content>
      
      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">Cancel</button>
        <button mat-raised-button color="primary" (click)="createNewPlaylist()">
          <mat-icon>add</mat-icon>
          Create New Playlist
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .add-to-playlist-dialog {
      min-width: 400px;
    }
    
    .playlists-list {
      max-height: 300px;
      overflow-y: auto;
    }
    
    .playlist-item {
      cursor: pointer;
      transition: all 0.2s;
      position: relative;
    }
    
    .playlist-item:hover {
      background-color: #f5f5f5;
    }
    
    .playlist-item.in-playlist {
      background-color: #e8f5e8;
      border-left: 4px solid #4caf50;
    }
    
    .playlist-item.in-playlist:hover {
      background-color: #d4edda;
    }
    
    .toggle-icon {
      position: absolute;
      right: 16px;
      top: 50%;
      transform: translateY(-50%);
      color: #666;
    }
    
    .playlist-item.in-playlist .toggle-icon {
      color: #f44336;
    }
    
    .no-playlists {
      text-align: center;
      padding: 40px 20px;
    }
    
    .no-playlists mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #ccc;
      margin-bottom: 16px;
    }
    
    .no-playlists p {
      color: #666;
      margin-bottom: 20px;
    }
    
    mat-dialog-content {
      padding: 20px 24px;
    }
    
    mat-dialog-actions {
      padding: 16px 24px;
      margin: 0;
    }
  `]
})
export class AddToPlaylistDialogComponent implements OnInit {
  playlists: Playlist[] = [];
  isLoading = false;
  videoPlaylistStatus: { [playlistId: number]: boolean } = {};

  constructor(
    public dialogRef: MatDialogRef<AddToPlaylistDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AddToPlaylistDialogData,
    private playlistService: PlaylistService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.videoPlaylistStatus = data.videoPlaylistStatus || {};
  }

  ngOnInit() {
    this.loadPlaylists();
  }

  loadPlaylists() {
    this.isLoading = true;
    this.playlistService.getPlaylists(1, 50).subscribe({
      next: (result) => {
        this.playlists = result.items || [];
        this.isLoading = false;
      },
      error: (error) => {
        this.playlists = [];
        this.isLoading = false;
      }
    });
  }

  isVideoInPlaylist(playlistId: number): boolean {
    return this.videoPlaylistStatus[playlistId] || false;
  }

  togglePlaylist(playlistId: number) {
    if (this.isVideoInPlaylist(playlistId)) {
      this.removeFromPlaylist(playlistId);
    } else {
      this.addToPlaylist(playlistId);
    }
  }

  addToPlaylist(playlistId: number) {
    this.playlistService.addVideoToPlaylist(playlistId, { videoId: this.data.videoId }).subscribe({
      next: () => {
        this.videoPlaylistStatus[playlistId] = true;
        this.snackBar.open('Added to playlist!', 'Close', { duration: 2000 });
      },
      error: (error) => {
        this.snackBar.open('Failed to add to playlist', 'Close', { duration: 3000 });
      }
    });
  }

  removeFromPlaylist(playlistId: number) {
    this.playlistService.removeVideoFromPlaylist(playlistId, this.data.videoId).subscribe({
      next: () => {
        this.videoPlaylistStatus[playlistId] = false;
        this.snackBar.open('Removed from playlist!', 'Close', { duration: 2000 });
      },
      error: (error) => {
        this.snackBar.open('Failed to remove from playlist', 'Close', { duration: 3000 });
      }
    });
  }

  createNewPlaylist() {
    const dialogRef = this.dialog.open(CreatePlaylistDialogComponent, {
      width: '500px',
      data: { videoId: this.data.videoId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Add the video to the newly created playlist
        this.addToPlaylist(result.id);
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
