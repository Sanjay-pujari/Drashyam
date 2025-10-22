import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { VideoProcessingService } from '../../services/video-processing.service';
import { VideoProcessingProgress } from '../../models/video-processing-progress.model';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-video-processing-status',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressBarModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './video-processing-status.component.html',
  styleUrls: ['./video-processing-status.component.scss']
})
export class VideoProcessingStatusComponent implements OnInit, OnDestroy {
  @Input() videoId!: number;
  
  processingProgress: VideoProcessingProgress | null = null;
  isProcessing = false;
  private progressSubscription?: Subscription;
  private statusCheckInterval?: Subscription;

  constructor(
    private videoProcessingService: VideoProcessingService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    if (this.videoId) {
      this.startProgressTracking();
    }
  }

  ngOnDestroy(): void {
    if (this.progressSubscription) {
      this.progressSubscription.unsubscribe();
    }
    if (this.statusCheckInterval) {
      this.statusCheckInterval.unsubscribe();
    }
  }

  private startProgressTracking(): void {
    // Check initial status
    this.checkProcessingStatus();
    
    // Set up interval to check status every 5 seconds
    this.statusCheckInterval = interval(5000).subscribe(() => {
      this.checkProcessingStatus();
    });
  }

  private checkProcessingStatus(): void {
    this.videoProcessingService.isVideoProcessing(this.videoId).subscribe({
      next: (isProcessing) => {
        this.isProcessing = isProcessing;
        
        if (isProcessing) {
          this.loadProcessingProgress();
        }
      },
      error: (error) => {
        console.error('Error checking processing status:', error);
      }
    });
  }

  private loadProcessingProgress(): void {
    this.videoProcessingService.getProcessingProgress(this.videoId).subscribe({
      next: (progress) => {
        this.processingProgress = progress;
        
        // Check if processing is complete
        if (progress.status === 'Completed' || progress.status === 'Failed') {
          this.isProcessing = false;
          if (this.statusCheckInterval) {
            this.statusCheckInterval.unsubscribe();
          }
          
          if (progress.status === 'Completed') {
            this.snackBar.open('Video processing completed!', 'Close', { duration: 5000 });
          } else if (progress.status === 'Failed') {
            this.snackBar.open('Video processing failed. Please try again.', 'Close', { duration: 5000 });
          }
        }
      },
      error: (error) => {
        console.error('Error loading processing progress:', error);
      }
    });
  }

  getStatusIcon(): string {
    if (!this.processingProgress) return 'help';
    
    switch (this.processingProgress.status) {
      case 'Queued':
        return 'schedule';
      case 'Processing':
        return 'play_circle';
      case 'Completed':
        return 'check_circle';
      case 'Failed':
        return 'error';
      default:
        return 'help';
    }
  }

  getStatusColor(): string {
    if (!this.processingProgress) return 'primary';
    
    switch (this.processingProgress.status) {
      case 'Queued':
        return 'accent';
      case 'Processing':
        return 'primary';
      case 'Completed':
        return 'primary';
      case 'Failed':
        return 'warn';
      default:
        return 'primary';
    }
  }

  refreshStatus(): void {
    this.checkProcessingStatus();
  }
}
