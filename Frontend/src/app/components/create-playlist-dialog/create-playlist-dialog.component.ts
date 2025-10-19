import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PlaylistService, PlaylistCreateDto, PlaylistVisibility } from '../../services/playlist.service';

export interface CreatePlaylistDialogData {
  videoId?: number;
}

@Component({
  selector: 'app-create-playlist-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule
  ],
  template: `
    <div class="create-playlist-dialog">
      <h2 mat-dialog-title>Create Playlist</h2>
      
      <mat-dialog-content>
        <form class="playlist-form">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Playlist Name</mat-label>
            <input matInput [(ngModel)]="playlistData.name" name="name" required>
          </mat-form-field>
          
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Description (Optional)</mat-label>
            <textarea matInput [(ngModel)]="playlistData.description" name="description" rows="3"></textarea>
          </mat-form-field>
          
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Visibility</mat-label>
            <mat-select [(ngModel)]="playlistData.visibility" name="visibility">
              <mat-option [value]="PlaylistVisibility.Public">Public</mat-option>
              <mat-option [value]="PlaylistVisibility.Unlisted">Unlisted</mat-option>
              <mat-option [value]="PlaylistVisibility.Private">Private</mat-option>
            </mat-select>
          </mat-form-field>
        </form>
      </mat-dialog-content>
      
      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">Cancel</button>
        <button mat-raised-button color="primary" (click)="onCreate()" [disabled]="!playlistData.name.trim()">
          Create Playlist
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .create-playlist-dialog {
      min-width: 400px;
    }
    
    .playlist-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    
    .full-width {
      width: 100%;
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
export class CreatePlaylistDialogComponent {
  playlistData: PlaylistCreateDto = {
    name: '',
    description: '',
    visibility: PlaylistVisibility.Public
  };
  
  PlaylistVisibility = PlaylistVisibility;
  isCreating = false;

  constructor(
    public dialogRef: MatDialogRef<CreatePlaylistDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreatePlaylistDialogData,
    private playlistService: PlaylistService,
    private snackBar: MatSnackBar
  ) {}

  onCancel(): void {
    this.dialogRef.close();
  }

  onCreate(): void {
    if (!this.playlistData.name.trim()) {
      return;
    }

    this.isCreating = true;
    
    this.playlistService.createPlaylist(this.playlistData).subscribe({
      next: (playlist) => {
        this.snackBar.open('Playlist created successfully!', 'Close', { duration: 3000 });
        this.dialogRef.close(playlist);
      },
      error: (error) => {
        console.error('Error creating playlist:', error);
        this.snackBar.open('Failed to create playlist', 'Close', { duration: 3000 });
        this.isCreating = false;
      }
    });
  }
}
