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
import { Router } from '@angular/router';
import { VideoService } from '../../services/video.service';
import { ChannelService } from '../../services/channel.service';
import { Channel } from '../../models/video.model';
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
  maxFileSize = environment.maxVideoSize;
  supportedFormats = environment.supportedVideoFormats;

  constructor(
    private fb: FormBuilder,
    private videoService: VideoService,
    private channelService: ChannelService,
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
    this.channelService.getUserChannels('me', { page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.userChannels = result.items;
      },
      error: (err) => console.error('Failed to load channels:', err)
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
    if (file) {
      this.selectedThumbnail = file;
    }
  }

  onSubmit() {
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
          console.error('Upload failed:', err);
          alert('Upload failed. Please try again.');
        }
      });
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
