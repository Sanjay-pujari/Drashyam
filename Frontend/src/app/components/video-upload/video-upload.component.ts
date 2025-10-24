import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { VideoService } from '../../services/video.service';
import { ChannelService } from '../../services/channel.service';
import { QuotaService, QuotaStatus } from '../../services/quota.service';
import { VideoProcessingService } from '../../services/video-processing.service';
import { Channel } from '../../models/channel.model';
import { VideoProcessingProgress } from '../../models/video-processing-progress.model';
import { VideoProcessingStatusComponent } from '../video-processing-status/video-processing-status.component';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-video-upload',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatProgressBarModule,
    MatCardModule,
    MatIconModule,
    MatChipsModule,
    MatSlideToggleModule,
    VideoProcessingStatusComponent
  ],
  templateUrl: './video-upload.component.html',
  styleUrls: ['./video-upload.component.scss']
})
export class VideoUploadComponent implements OnInit {
  uploadForm: FormGroup;
  selectedFile: File | null = null;
  selectedThumbnail: File | null = null;
  uploadProgress = 0;
  isUploading = false;
  userChannels: Channel[] = [];
  isLoadingChannels = false;
  maxFileSize = environment.maxVideoSize;
  supportedFormats = environment.supportedVideoFormats;
  quotaStatus: QuotaStatus | null = null;
  quotaWarning = false;
  uploadedVideoId: number | null = null;
  processingProgress: VideoProcessingProgress | null = null;
  private isInitialized = false;
  private _isFormValid = false;

  constructor(
    private fb: FormBuilder,
    private videoService: VideoService,
    private channelService: ChannelService,
    private quotaService: QuotaService,
    private videoProcessingService: VideoProcessingService,
    private snackBar: MatSnackBar,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.uploadForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(1000)]],
      tags: [''],
      channelId: ['', Validators.required],
      visibility: [0], // 0 = Public, 1 = Private, 2 = Unlisted
      category: [''],
      isPremium: [false],
      premiumPrice: [0, [Validators.min(0)]],
      premiumCurrency: ['USD']
    });
  }

  ngOnInit() {
    this.loadUserChannels();
    this.loadQuotaStatus();
    this.isInitialized = true;
    
    // Set up form value change listeners
    this.uploadForm.valueChanges.subscribe(() => {
      this.updateFormValidity();
    });
    
    // Initial form validity check
    this.updateFormValidity();
  }

  loadUserChannels() {
    this.isLoadingChannels = true;
    this.channelService.getMyChannels().subscribe({
      next: (result) => {
        this.userChannels = result.items || [];
        this.isLoadingChannels = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.userChannels = [];
        this.isLoadingChannels = false;
        this.snackBar.open('Error loading channels', 'Close', { duration: 3000 });
        this.cdr.detectChanges();
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      // Validate file size
      if (file.size > this.maxFileSize) {
        this.snackBar.open(`File size exceeds maximum limit of ${this.formatFileSize(this.maxFileSize)}`, 'Close', { duration: 5000 });
        return;
      }

      // Validate file format
      const fileExtension = file.name.split('.').pop()?.toLowerCase();
      if (!this.supportedFormats.includes(fileExtension || '')) {
        this.snackBar.open(`Unsupported file format. Supported formats: ${this.supportedFormats.join(', ')}`, 'Close', { duration: 5000 });
        return;
      }

      this.selectedFile = file;
      
      // Check quota when file is selected
      this.checkQuotaForUpload();
    }
  }

  onThumbnailSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedThumbnail = file;
    }
  }

  onSubmit() {
    // Add conditional validation for premium content
    if (this.uploadForm.get('isPremium')?.value) {
      this.uploadForm.get('premiumPrice')?.setValidators([Validators.required, Validators.min(0.01)]);
    } else {
      this.uploadForm.get('premiumPrice')?.clearValidators();
    }
    this.uploadForm.get('premiumPrice')?.updateValueAndValidity();
    
    // Check if quota warning is active
    if (this.quotaWarning) {
      this.snackBar.open('Upload quota exceeded. Please upgrade your plan or delete some videos.', 'Close', { duration: 5000 });
      return;
    }
    
    if (this.isFormValid && this.selectedFile) {
      // Check quota before uploading
      this.checkQuotaBeforeUpload();
    }
  }

  private checkQuotaBeforeUpload() {
    if (!this.selectedFile) return;

    this.quotaService.canUploadVideo(this.selectedFile.size).subscribe({
      next: (canUpload) => {
        if (canUpload) {
          this.proceedWithUpload();
        } else {
          this.snackBar.open('Upload quota exceeded. Please upgrade your plan or delete some videos.', 'Close', { duration: 5000 });
          this.quotaWarning = true;
          this.cdr.detectChanges();
        }
      },
      error: (error) => {
        console.error('Quota check failed:', error);
        this.snackBar.open('Unable to check quota. Please try again.', 'Close', { duration: 3000 });
        this.cdr.detectChanges();
      }
    });
  }

  private proceedWithUpload() {
    this.isUploading = true;
    this.uploadProgress = 0;

    const formData = new FormData();
    formData.append('videoFile', this.selectedFile!);
    formData.append('title', this.uploadForm.value.title);
    formData.append('description', this.uploadForm.value.description);
    formData.append('tags', this.uploadForm.value.tags);
    formData.append('channelId', this.uploadForm.value.channelId);
    formData.append('visibility', this.uploadForm.value.visibility);
    formData.append('category', this.uploadForm.value.category);
    formData.append('isPremium', this.uploadForm.value.isPremium);
    formData.append('premiumPrice', this.uploadForm.value.premiumPrice);
    formData.append('premiumCurrency', this.uploadForm.value.premiumCurrency);

    if (this.selectedThumbnail) {
      formData.append('thumbnailFile', this.selectedThumbnail);
    }

    // Simulate upload progress
    const progressInterval = setInterval(() => {
      this.uploadProgress += Math.random() * 15;
      if (this.uploadProgress >= 90) {
        clearInterval(progressInterval);
      }
    }, 200);

    this.videoService.uploadVideo(formData).subscribe({
      next: (video) => {
        clearInterval(progressInterval);
        this.uploadProgress = 100;
        this.uploadedVideoId = video.id;
        this.snackBar.open('Video uploaded successfully! Processing will begin shortly.', 'Close', { duration: 5000 });
        // Redirect immediately to the uploaded video's page; processing will continue there
        if (this.uploadedVideoId) {
          this.router.navigate(['/videos', this.uploadedVideoId]);
        }
      },
      error: (error) => {
        clearInterval(progressInterval);
        this.isUploading = false;
        this.uploadProgress = 0;
        this.snackBar.open('Upload failed. Please try again.', 'Close', { duration: 5000 });
      }
    });
  }

  onChannelChange() {
    // Re-check quota when channel changes (only after initialization)
    if (this.isInitialized && this.selectedFile) {
      this.checkQuotaForUpload();
    }
  }

  onPremiumToggleChange() {
    // Update form validation when premium toggle changes
    this.updateFormValidity();
  }

  private checkQuotaForUpload() {
    if (!this.selectedFile) return;

    // Use setTimeout to ensure this runs after the current change detection cycle
    setTimeout(() => {
      this.quotaService.canUploadVideo(this.selectedFile!.size).subscribe({
        next: (canUpload) => {
          this.quotaWarning = !canUpload;
          if (!canUpload) {
            this.snackBar.open('Upload quota exceeded. Please upgrade your plan or delete some videos.', 'Close', { duration: 5000 });
          }
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error checking quota:', error);
          this.snackBar.open('Failed to check upload quota', 'Close', { duration: 3000 });
          this.cdr.detectChanges();
        }
      });
    }, 0);
  }

  onProcessingCompleted(status: 'Completed' | 'Failed') {
    this.isUploading = false;
    
    if (status === 'Completed') {
      if (this.uploadedVideoId) {
        this.router.navigate(['/videos', this.uploadedVideoId]);
      } else {
        this.router.navigate(['/videos']);
      }
    } else {
      this.snackBar.open('Video processing failed. Please try uploading again.', 'Close', { duration: 5000 });
    }
  }

  getFormValidationErrors() {
    const errors: any = {};
    Object.keys(this.uploadForm.controls).forEach(key => {
      const control = this.uploadForm.get(key);
      if (control && control.errors) {
        errors[key] = control.errors;
      }
    });
    return errors;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  generateThumbnail(file: File) {
    const video = document.createElement('video');
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');

    video.addEventListener('loadedmetadata', () => {
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;
      video.currentTime = 1; // Seek to 1 second
    });

    video.addEventListener('seeked', () => {
      if (ctx) {
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        canvas.toBlob((blob) => {
          if (blob) {
            const thumbnailFile = new File([blob], 'thumbnail.jpg', { type: 'image/jpeg' });
            this.selectedThumbnail = thumbnailFile;
          }
        }, 'image/jpeg', 0.8);
      }
    });

    video.src = URL.createObjectURL(file);
  }

  setRecordedVideo(file: File) {
    this.selectedFile = file;
    
    // Create a new file input event to trigger the form
    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    if (fileInput) {
      fileInput.files = dataTransfer.files;
    }

    this.generateThumbnail(file);
    this.snackBar.open('Recorded video loaded successfully!', 'Close', { duration: 3000 });
    
    // Check quota for recorded video
    this.checkQuotaForUpload();
  }

  getQuotaWarningMessage(): string {
    if (!this.quotaStatus) return '';
    return 'Upload quota exceeded. Please upgrade your plan or delete some videos.';
  }

  getQuotaRecommendation(): string {
    if (!this.quotaStatus) return '';
    return 'Consider upgrading your subscription plan to increase your upload quota.';
  }

  navigateToQuota() {
    this.router.navigate(['/quota']);
  }

  navigateToHome() {
    this.router.navigateByUrl('/');
  }

  loadQuotaStatus() {
    this.quotaService.getQuotaStatus().subscribe({
      next: (status) => {
        this.quotaStatus = status;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading quota status:', error);
        this.cdr.detectChanges();
      }
    });
  }

  formatBytes(bytes: number): string {
    return this.quotaService.formatBytes(bytes);
  }

  getUsageColor(percentage: number): string {
    return this.quotaService.getUsageColor(percentage);
  }

  get isFormValid(): boolean {
    return this._isFormValid;
  }

  private updateFormValidity(): void {
    // Use setTimeout to avoid change detection issues
    setTimeout(() => {
      this._isFormValid = this.uploadForm.valid;
      this.cdr.detectChanges();
    }, 0);
  }
}