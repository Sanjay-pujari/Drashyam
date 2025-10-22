import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router } from '@angular/router';
import { VideoRecorderComponent } from '../video-recorder/video-recorder.component';
import { VideoUploadComponent } from '../video-upload/video-upload.component';
import { QuotaService, QuotaStatus } from '../../services/quota.service';

@Component({
  selector: 'app-video-record-upload',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    VideoRecorderComponent,
    VideoUploadComponent
  ],
  templateUrl: './video-record-upload.component.html',
  styleUrls: ['./video-record-upload.component.scss']
})
export class VideoRecordUploadComponent implements OnInit {
  @ViewChild('videoUpload') videoUpload!: VideoUploadComponent;

  selectedTabIndex = 0;
  recordedVideoBlob: Blob | null = null;
  quotaStatus: QuotaStatus | null = null;
  quotaWarning = false;
  isProcessingRecording = false;

  constructor(
    private snackBar: MatSnackBar,
    private quotaService: QuotaService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadQuotaStatus();
  }

  onRecordingComplete(blob: Blob) {
    this.recordedVideoBlob = blob;
    this.isProcessingRecording = true;
    
    // Show success message
    this.snackBar.open('Video recorded successfully! Switch to Upload tab to add details.', 'Close', {
      duration: 5000
    });

    // Auto-switch to upload tab
    setTimeout(() => {
      this.selectedTabIndex = 1;
      this.isProcessingRecording = false;
      this.loadRecordedVideo();
    }, 1000);
  }

  onRecordingCanceled() {
    this.recordedVideoBlob = null;
    this.snackBar.open('Recording canceled', 'Close', { duration: 2000 });
  }

  private loadRecordedVideo() {
    if (this.recordedVideoBlob && this.videoUpload) {
      // Convert blob to file
      const fileName = `recorded-video-${Date.now()}.webm`;
      const file = new File([this.recordedVideoBlob], fileName, {
        type: this.recordedVideoBlob.type || 'video/webm'
      });

      // Set the recorded video in the upload component
      this.videoUpload.setRecordedVideo(file);
    }
  }

  onTabChange(index: number) {
    this.selectedTabIndex = index;
    
    // If switching to upload tab and we have a recorded video, load it
    if (index === 1 && this.recordedVideoBlob && this.videoUpload) {
      this.loadRecordedVideo();
    }
  }

  resetRecording() {
    this.recordedVideoBlob = null;
    this.selectedTabIndex = 0;
  }

  loadQuotaStatus() {
    this.quotaService.getQuotaStatus().subscribe({
      next: (status) => {
        this.quotaStatus = status;
        this.quotaWarning = status.videoUsagePercentage >= 100 || status.storageUsagePercentage >= 100;
      },
      error: (error) => {
        console.error('Error loading quota status:', error);
      }
    });
  }

  formatBytes(bytes: number): string {
    return this.quotaService.formatBytes(bytes);
  }

  getUsageColor(percentage: number): string {
    return this.quotaService.getUsageColor(percentage);
  }

  navigateToQuota() {
    this.router.navigate(['/quota']);
  }
}
