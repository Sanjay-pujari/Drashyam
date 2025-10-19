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
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { VideoService } from '../../services/video.service';
import { ChannelService } from '../../services/channel.service';
import { Channel } from '../../models/channel.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-video-upload',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatSelectModule, MatProgressBarModule, MatCardModule,
    MatIconModule, MatChipsModule
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

  constructor(
    private fb: FormBuilder,
    private videoService: VideoService,
    private channelService: ChannelService,
    private snackBar: MatSnackBar,
    public router: Router
  ) {
    this.uploadForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      channelId: [null],
      visibility: ['Public', Validators.required],
      category: [''],
      tags: ['']
    });
  }

  ngOnInit() {
    this.loadUserChannels();
  }

  loadUserChannels() {
    console.log('Loading user channels...');
    this.isLoadingChannels = true;
    this.channelService.getUserChannels('me', { page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        console.log('Channels loaded successfully:', result);
        this.userChannels = result.items;
        this.isLoadingChannels = false;
        console.log('User channels:', this.userChannels);
      },
      error: (err) => {
        console.error('Failed to load channels:', err);
        console.error('Error details:', {
          status: err.status,
          statusText: err.statusText,
          message: err.message,
          error: err.error
        });
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
    }
  }

  onThumbnailSelected(event: any) {
    const file = event.target.files[0];
    console.log('Thumbnail file selected:', file);
    if (file) {
      this.selectedThumbnail = file;
      console.log('Thumbnail set:', this.selectedThumbnail);
    }
  }

  onSubmit() {
    console.log('Form valid:', this.uploadForm.valid);
    console.log('Form errors:', this.uploadForm.errors);
    console.log('Selected file:', this.selectedFile);
    console.log('Form value:', this.uploadForm.value);
    
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
      
      if (this.uploadForm.value.channelId) {
        formData.append('channelId', this.uploadForm.value.channelId);
      }
      
      if (this.selectedThumbnail) {
        formData.append('thumbnailFile', this.selectedThumbnail);
      }

      console.log('FormData contents:');
      // Log FormData contents in a compatible way
      console.log('Video file:', this.selectedFile?.name);
      console.log('Title:', this.uploadForm.value.title);
      console.log('Description:', this.uploadForm.value.description);
      console.log('Visibility:', this.uploadForm.value.visibility);
      console.log('Category:', this.uploadForm.value.category);
      console.log('Tags:', this.uploadForm.value.tags);
      console.log('Channel ID:', this.uploadForm.value.channelId);
      console.log('Thumbnail file:', this.selectedThumbnail?.name);

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
          console.log('Upload successful:', video);
          setTimeout(() => {
            this.router.navigate(['/videos', video.id]);
          }, 1000);
        },
        error: (err) => {
          clearInterval(progressInterval);
          this.isUploading = false;
          console.error('Upload failed:', err);
          console.error('Error details:', {
            status: err.status,
            statusText: err.statusText,
            message: err.message,
            error: err.error
          });
          alert('Upload failed. Please try again.');
        }
      });
    } else {
      console.log('Form is invalid or no file selected');
      if (!this.uploadForm.valid) {
        console.log('Form validation errors:', this.getFormValidationErrors());
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
            this.selectedThumbnail = blob;
            console.log('Thumbnail generated for recorded video');
          }
        }, 'image/jpeg', 0.8);
      }
    };
    
    video.src = URL.createObjectURL(file);
  }

  setRecordedVideo(file: File) {
    console.log('Setting recorded video:', file);
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
  }
}
