import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
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
import { QuotaService, QuotaCheck } from '../../services/quota.service';
import { Channel } from '../../models/channel.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-video-upload',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatSelectModule, MatProgressBarModule, MatCardModule,
    MatIconModule, MatChipsModule, MatSlideToggleModule
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
  quotaCheck: QuotaCheck | null = null;
  quotaWarning = false;

  constructor(
    private fb: FormBuilder,
    private videoService: VideoService,
    private channelService: ChannelService,
    private quotaService: QuotaService,
    private snackBar: MatSnackBar,
    public router: Router
  ) {
    this.uploadForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      channelId: [null],
      visibility: [0, Validators.required], // 0 = Public, 1 = Private, 2 = Unlisted
      category: [''],
      tags: [''],
      isPremium: [false],
      premiumPrice: [null],
      premiumCurrency: ['USD']
    });
  }

  ngOnInit() {
    this.loadUserChannels();
  }

  loadUserChannels() {
    this.isLoadingChannels = true;
    this.channelService.getUserChannels('me', { page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.userChannels = result.items;
        this.isLoadingChannels = false;
      },
      error: (err) => {
        this.isLoadingChannels = false;
      }
    });
  }

  onVideoFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (file.size > this.maxFileSize) {
        alert(`File size exceeds ${this.maxFileSize / (1024 * 1024 * 1024)}GB limit`);
        return;
      }
      
      const extension = file.name.split('.').pop()?.toLowerCase();
      if (!this.supportedFormats.includes(extension || '')) {
        alert(`Unsupported format. Supported: ${this.supportedFormats.join(', ')}`);
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
    
    if (this.uploadForm.valid && this.selectedFile) {
      this.isUploading = true;
      this.uploadProgress = 0;

      const formData = new FormData();
      formData.append('videoFile', this.selectedFile);
      formData.append('title', this.uploadForm.value.title);
      formData.append('description', this.uploadForm.value.description || '');
      formData.append('visibility', this.uploadForm.value.visibility);
      formData.append('category', this.uploadForm.value.category || '');
      formData.append('tags', this.uploadForm.value.tags || '');
      
      // Add premium content fields
      formData.append('isPremium', this.uploadForm.value.isPremium.toString());
      if (this.uploadForm.value.isPremium) {
        formData.append('premiumPrice', this.uploadForm.value.premiumPrice);
        formData.append('premiumCurrency', this.uploadForm.value.premiumCurrency);
      }
      
      if (this.uploadForm.value.channelId) {
        formData.append('channelId', this.uploadForm.value.channelId);
      }
      
      if (this.selectedThumbnail) {
        formData.append('thumbnailFile', this.selectedThumbnail);
      }

      // Log FormData contents in a compatible way

      // Simulate progress (in real app, you'd track actual upload progress)
      const progressInterval = setInterval(() => {
        this.uploadProgress += 10;
        if (this.uploadProgress >= 90) {
          clearInterval(progressInterval);
        }
      }, 200);

      this.videoService.uploadVideo(formData).subscribe({
        next: (video) => {
          clearInterval(progressInterval);
          this.uploadProgress = 100;
          setTimeout(() => {
            this.router.navigate(['/videos', video.id]);
          }, 1000);
        },
        error: (err) => {
          clearInterval(progressInterval);
          this.isUploading = false;
          alert('Upload failed. Please try again.');
        }
      });
    } else {
      if (!this.uploadForm.valid) {
      }
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
    video.preload = 'metadata';
    
    video.onloadedmetadata = () => {
      video.currentTime = 1; // Seek to 1 second
    };
    
    video.onseeked = () => {
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');
      
      if (ctx) {
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        
        canvas.toBlob((blob) => {
          if (blob) {
            // Convert Blob to File
            const thumbnailFile = new File([blob], 'thumbnail.jpg', {
              type: 'image/jpeg',
              lastModified: Date.now()
            });
            this.selectedThumbnail = thumbnailFile;
          }
        }, 'image/jpeg', 0.8);
      }
    };
    
    video.src = URL.createObjectURL(file);
  }

  setRecordedVideo(file: File) {
    this.selectedFile = file;
    
    // Update the file input display
    const fileInput = document.getElementById('videoFile') as HTMLInputElement;
    if (fileInput) {
      // Create a new FileList with the recorded file
      const dataTransfer = new DataTransfer();
      dataTransfer.items.add(file);
      fileInput.files = dataTransfer.files;
    }
    
    // Generate thumbnail for recorded video
    this.generateThumbnail(file);
    
    // Show success message
    this.snackBar.open('Recorded video loaded successfully!', 'Close', { duration: 3000 });
    
    // Check quota for recorded video
    this.checkQuotaForUpload();
  }

  private checkQuotaForUpload() {
    if (!this.selectedFile || !this.uploadForm.get('channelId')?.value) return;

    const channelId = this.uploadForm.get('channelId')?.value;
    const fileSize = this.selectedFile.size;

    this.quotaService.checkVideoUpload(channelId, fileSize).subscribe({
      next: (quotaCheck) => {
        this.quotaCheck = quotaCheck;
        this.quotaWarning = quotaCheck.warnings?.hasWarnings || false;

        if (!quotaCheck.canUpload) {
          this.snackBar.open(quotaCheck.reason || 'Upload quota exceeded', 'Close', { duration: 5000 });
        } else if (quotaCheck.warnings?.hasWarnings) {
          this.snackBar.open('Quota warning: ' + quotaCheck.warnings.warnings.join(', '), 'Close', { duration: 5000 });
        }
      },
      error: (error) => {
        console.error('Error checking quota:', error);
        this.snackBar.open('Failed to check upload quota', 'Close', { duration: 3000 });
      }
    });
  }

  onChannelChange() {
    // Re-check quota when channel changes
    if (this.selectedFile) {
      this.checkQuotaForUpload();
    }
  }

  getQuotaWarningMessage(): string {
    if (!this.quotaCheck?.warnings) return '';
    return this.quotaCheck.warnings.warnings.join(', ');
  }

  getQuotaRecommendation(): string {
    if (!this.quotaCheck?.warnings) return '';
    return this.quotaCheck.warnings.recommendedAction;
  }
}
