import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { VideoService } from '../../services/video.service';
import { PremiumContentService } from '../../services/premium-content.service';
import { Video, VideoStatus } from '../../models/video.model';
import { PremiumVideo } from '../../models/premium-video.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-video-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatDialogModule
  ],
  template: `
    <div class="video-management-container">
      <div class="header">
        <h1>Video Management</h1>
        <button mat-raised-button color="primary" routerLink="/upload">
          <mat-icon>add</mat-icon>
          Upload New Video
        </button>
      </div>

      <div class="content" *ngIf="!isLoading; else loading">
        <div class="empty-state" *ngIf="videos.length === 0">
          <mat-icon>video_library</mat-icon>
          <h2>No videos yet</h2>
          <p>Upload your first video to get started</p>
          <button mat-raised-button color="primary" routerLink="/upload">
            <mat-icon>upload</mat-icon>
            Upload Video
          </button>
        </div>

        <div class="videos-grid" *ngIf="videos.length > 0">
          <mat-card class="video-card" *ngFor="let video of videos">
            <div class="video-thumbnail">
              <img [src]="video.thumbnailUrl || '/assets/default-video-thumbnail.svg'" [alt]="video.title">
              <div class="video-duration">{{ formatDuration(video.duration) }}</div>
              <div class="video-status" [class]="getStatusClass(video.status)">
                {{ getStatusText(video.status) }}
              </div>
            </div>
            
            <div class="video-info">
              <h3 class="video-title">{{ video.title }}</h3>
              <p class="video-stats">
                <span>{{ video.viewCount | number }} views</span>
                <span>{{ video.likeCount | number }} likes</span>
                <span>{{ video.createdAt | date:'MMM d, y' }}</span>
              </p>
              <div class="video-actions">
                <button mat-button color="primary" (click)="viewVideo(video.id)">
                  <mat-icon>visibility</mat-icon>
                  View
                </button>
                <button mat-button (click)="editVideo(video)">
                  <mat-icon>edit</mat-icon>
                  Edit
                </button>
                <button mat-button color="warn" (click)="deleteVideo(video.id)">
                  <mat-icon>delete</mat-icon>
                  Delete
                </button>
              </div>
            </div>
          </mat-card>
        </div>
      </div>

      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading videos...</p>
        </div>
      </ng-template>
    </div>

    <!-- Edit Video Dialog -->
    <div class="edit-dialog" *ngIf="editingVideo">
      <div class="dialog-overlay" (click)="closeEditDialog()"></div>
      <div class="dialog-content">
        <div class="dialog-header">
          <h2>Edit Video: {{ editingVideo.title }}</h2>
          <button mat-icon-button (click)="closeEditDialog()">
            <mat-icon>close</mat-icon>
          </button>
        </div>
        
        <div class="dialog-body">
          <form [formGroup]="editForm" (ngSubmit)="updateVideo()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Title</mat-label>
              <input matInput formControlName="title">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Description</mat-label>
              <textarea matInput formControlName="description" rows="3"></textarea>
            </mat-form-field>

            <mat-form-field appearance="outline" class="half-width">
              <mat-label>Visibility</mat-label>
              <mat-select formControlName="visibility">
                <mat-option [value]="0">Public</mat-option>
                <mat-option [value]="1">Private</mat-option>
                <mat-option [value]="2">Unlisted</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" class="half-width">
              <mat-label>Category</mat-label>
              <mat-select formControlName="category">
                <mat-option value="">Select Category</mat-option>
                <mat-option value="Music">Music</mat-option>
                <mat-option value="Gaming">Gaming</mat-option>
                <mat-option value="Education">Education</mat-option>
                <mat-option value="Entertainment">Entertainment</mat-option>
                <mat-option value="Technology">Technology</mat-option>
                <mat-option value="Sports">Sports</mat-option>
                <mat-option value="News">News</mat-option>
                <mat-option value="Lifestyle">Lifestyle</mat-option>
                <mat-option value="Travel">Travel</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Tags</mat-label>
              <input matInput formControlName="tags" placeholder="Enter tags separated by commas">
            </mat-form-field>

            <!-- Premium Content Section -->
            <div class="premium-section">
              <h3>Premium Content Settings</h3>
              <div class="premium-option">
                <div class="option-info">
                  <h4>Make this video premium</h4>
                  <p>Set a price for viewers to purchase access to this video</p>
                </div>
                <mat-slide-toggle formControlName="isPremium">
                  {{ editForm.get('isPremium')?.value ? 'Premium' : 'Free' }}
                </mat-slide-toggle>
              </div>

              @if (editForm.get('isPremium')?.value) {
                <div class="premium-details">
                  <mat-form-field appearance="outline" class="half-width">
                    <mat-label>Price</mat-label>
                    <input matInput formControlName="premiumPrice" type="number" step="0.01" min="0.01">
                    <mat-error *ngIf="editForm.get('premiumPrice')?.hasError('required')">
                      Price is required for premium content
                    </mat-error>
                    <mat-error *ngIf="editForm.get('premiumPrice')?.hasError('min')">
                      Price must be at least $0.01
                    </mat-error>
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="half-width">
                    <mat-label>Currency</mat-label>
                    <mat-select formControlName="premiumCurrency">
                      <mat-option value="USD">USD ($)</mat-option>
                      <mat-option value="EUR">EUR (€)</mat-option>
                      <mat-option value="GBP">GBP (£)</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>
              }
            </div>

            <div class="form-actions">
              <button mat-raised-button color="primary" type="submit" [disabled]="editForm.invalid || isUpdating">
                <mat-icon *ngIf="isUpdating">hourglass_empty</mat-icon>
                <mat-icon *ngIf="!isUpdating">save</mat-icon>
                {{ isUpdating ? 'Updating...' : 'Update Video' }}
              </button>
              <button mat-button type="button" (click)="closeEditDialog()">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .video-management-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
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

    .videos-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }

    .video-card {
      overflow: hidden;
      transition: transform 0.2s ease;
    }

    .video-card:hover {
      transform: translateY(-2px);
    }

    .video-thumbnail {
      position: relative;
      width: 100%;
      height: 200px;
      background: #f5f5f5;
    }

    .video-thumbnail img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .video-duration {
      position: absolute;
      bottom: 8px;
      right: 8px;
      background: rgba(0,0,0,0.8);
      color: white;
      padding: 2px 6px;
      border-radius: 3px;
      font-size: 0.8rem;
    }

    .video-status {
      position: absolute;
      top: 8px;
      left: 8px;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.8rem;
      font-weight: 500;
    }

    .video-status.ready {
      background: #4caf50;
      color: white;
    }

    .video-status.processing {
      background: #ff9800;
      color: white;
    }

    .video-status.failed {
      background: #f44336;
      color: white;
    }

    .video-info {
      padding: 16px;
    }

    .video-title {
      font-size: 1.1rem;
      font-weight: 500;
      margin: 0 0 8px 0;
      color: #333;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .video-stats {
      display: flex;
      gap: 12px;
      color: #666;
      font-size: 0.9rem;
      margin: 0 0 16px 0;
    }

    .video-actions {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .video-actions button {
      flex: 1;
      min-width: 80px;
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

    /* Edit Dialog Styles */
    .edit-dialog {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      z-index: 1000;
    }

    .dialog-overlay {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0,0,0,0.5);
    }

    .dialog-content {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: white;
      border-radius: 8px;
      width: 90%;
      max-width: 600px;
      max-height: 90vh;
      overflow-y: auto;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px;
      border-bottom: 1px solid #eee;
    }

    .dialog-header h2 {
      margin: 0;
      color: #333;
    }

    .dialog-body {
      padding: 20px;
    }

    .premium-section {
      margin: 20px 0;
      padding: 20px;
      background: #f8f9fa;
      border-radius: 8px;
      border: 1px solid #e9ecef;
    }

    .premium-section h3 {
      margin-bottom: 20px;
      color: #333;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .premium-section h3::before {
      content: "💎";
      font-size: 1.2em;
    }

    .premium-option {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .option-info h4 {
      margin: 0 0 5px 0;
      color: #333;
      font-size: 1.1rem;
    }

    .option-info p {
      margin: 0;
      color: #666;
      font-size: 0.9rem;
    }

    .premium-details {
      display: flex;
      gap: 16px;
      margin-top: 20px;
      padding: 16px;
      background: white;
      border-radius: 6px;
      border: 1px solid #dee2e6;
    }

    .form-actions {
      display: flex;
      gap: 16px;
      justify-content: flex-end;
      margin-top: 20px;
      padding-top: 20px;
      border-top: 1px solid #eee;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .half-width {
      width: 48%;
      margin-bottom: 16px;
    }

    @media (max-width: 768px) {
      .videos-grid {
        grid-template-columns: 1fr;
      }

      .dialog-content {
        width: 95%;
        margin: 20px;
      }

      .premium-details {
        flex-direction: column;
      }

      .half-width {
        width: 100%;
      }

      .form-actions {
        flex-direction: column;
      }
    }
  `]
})
export class VideoManagementComponent implements OnInit, OnDestroy {
  videos: Video[] = [];
  isLoading = false;
  editingVideo: Video | null = null;
  isUpdating = false;
  editForm!: FormGroup;
  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private videoService: VideoService,
    private premiumContentService: PremiumContentService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.editForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      visibility: [0, Validators.required],
      category: [''],
      tags: [''],
      isPremium: [false],
      premiumPrice: [null],
      premiumCurrency: ['USD']
    });
  }

  ngOnInit() {
    this.loadVideos();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  loadVideos() {
    this.isLoading = true;
    this.subscriptions.add(
      this.videoService.getUserVideos('me', { page: 1, pageSize: 50 }).subscribe({
        next: (result) => {
          this.videos = result.items;
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.snackBar.open('Failed to load videos', 'Close', { duration: 3000 });
        }
      })
    );
  }

  viewVideo(videoId: number) {
    this.router.navigate(['/videos', videoId]);
  }

  editVideo(video: Video) {
    this.editingVideo = video;
    this.editForm.patchValue({
      title: video.title,
      description: video.description || '',
      visibility: video.visibility,
      category: video.category || '',
      tags: video.tags || '',
      isPremium: false, // Will be updated when we load premium info
      premiumPrice: null,
      premiumCurrency: 'USD'
    });

    // Load premium content info if exists
    this.subscriptions.add(
      this.premiumContentService.getPremiumVideoByVideoId(video.id).subscribe({
        next: (premiumVideo) => {
          if (premiumVideo) {
            this.editForm.patchValue({
              isPremium: true,
              premiumPrice: premiumVideo.price,
              premiumCurrency: premiumVideo.currency
            });
          }
        },
        error: (error) => {
          // No premium content, that's fine
        }
      })
    );
  }

  updateVideo() {
    if (!this.editingVideo || this.editForm.invalid) return;

    // Add conditional validation for premium content
    if (this.editForm.get('isPremium')?.value) {
      this.editForm.get('premiumPrice')?.setValidators([Validators.required, Validators.min(0.01)]);
    } else {
      this.editForm.get('premiumPrice')?.clearValidators();
    }
    this.editForm.get('premiumPrice')?.updateValueAndValidity();

    if (this.editForm.invalid) return;

    this.isUpdating = true;
    const updateData = new FormData();
    updateData.append('title', this.editForm.value.title);
    updateData.append('description', this.editForm.value.description || '');
    updateData.append('visibility', this.editForm.value.visibility.toString());
    updateData.append('category', this.editForm.value.category || '');
    updateData.append('tags', this.editForm.value.tags || '');

    this.subscriptions.add(
      this.videoService.updateVideo(this.editingVideo.id, updateData).subscribe({
        next: (updatedVideo) => {
          // Update premium content
          if (this.editForm.value.isPremium && this.editForm.value.premiumPrice) {
            this.updatePremiumContent(updatedVideo.id);
          } else {
            this.removePremiumContent(updatedVideo.id);
          }
        },
        error: (error) => {
          this.isUpdating = false;
          this.snackBar.open('Failed to update video', 'Close', { duration: 3000 });
        }
      })
    );
  }

  private updatePremiumContent(videoId: number) {
    const premiumData = {
      price: this.editForm.value.premiumPrice,
      currency: this.editForm.value.premiumCurrency
    };

    this.subscriptions.add(
      this.premiumContentService.updatePremiumVideoByVideoId(videoId, premiumData).subscribe({
        next: () => {
          this.isUpdating = false;
          this.snackBar.open('Video updated successfully', 'Close', { duration: 3000 });
          this.closeEditDialog();
          this.loadVideos();
        },
        error: (error) => {
          this.isUpdating = false;
          this.snackBar.open('Failed to update premium settings', 'Close', { duration: 3000 });
        }
      })
    );
  }

  private removePremiumContent(videoId: number) {
    this.subscriptions.add(
      this.premiumContentService.deletePremiumVideoByVideoId(videoId).subscribe({
        next: () => {
          this.isUpdating = false;
          this.snackBar.open('Video updated successfully', 'Close', { duration: 3000 });
          this.closeEditDialog();
          this.loadVideos();
        },
        error: (error) => {
          this.isUpdating = false;
          this.snackBar.open('Failed to remove premium settings', 'Close', { duration: 3000 });
        }
      })
    );
  }

  deleteVideo(videoId: number) {
    if (confirm('Are you sure you want to delete this video? This action cannot be undone.')) {
      this.subscriptions.add(
        this.videoService.deleteVideo(videoId).subscribe({
          next: () => {
            this.snackBar.open('Video deleted successfully', 'Close', { duration: 3000 });
            this.loadVideos();
          },
          error: (error) => {
            this.snackBar.open('Failed to delete video', 'Close', { duration: 3000 });
          }
        })
      );
    }
  }

  closeEditDialog() {
    this.editingVideo = null;
    this.editForm.reset();
  }

  formatDuration(duration: string): string {
    // Convert duration string to readable format
    const parts = duration.split(':');
    if (parts.length === 3) {
      const hours = parseInt(parts[0]);
      const minutes = parseInt(parts[1]);
      const seconds = parseInt(parts[2]);
      
      if (hours > 0) {
        return `${hours}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
      } else {
        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
      }
    }
    return duration;
  }

  getStatusClass(status: VideoStatus): string {
    switch (status) {
      case VideoStatus.Published: return 'ready';
      case VideoStatus.Processing: return 'processing';
      case VideoStatus.Draft: return 'processing';
      case VideoStatus.Deleted: return 'failed';
      default: return 'processing';
    }
  }

  getStatusText(status: VideoStatus): string {
    switch (status) {
      case VideoStatus.Published: return 'Ready';
      case VideoStatus.Processing: return 'Processing';
      case VideoStatus.Draft: return 'Draft';
      case VideoStatus.Private: return 'Private';
      case VideoStatus.Unlisted: return 'Unlisted';
      case VideoStatus.Deleted: return 'Deleted';
      default: return 'Processing';
    }
  }
}

