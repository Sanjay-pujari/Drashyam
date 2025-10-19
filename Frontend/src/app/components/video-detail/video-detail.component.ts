import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { VideoService } from '../../services/video.service';
import { Video } from '../../models/video.model';
import { VideoPlayerComponent } from '../video-player/video-player.component';

@Component({
  selector: 'app-video-detail',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatButtonModule, MatIconModule, 
    MatChipsModule, MatProgressSpinnerModule, VideoPlayerComponent
  ],
  templateUrl: './video-detail.component.html',
  styleUrls: ['./video-detail.component.scss']
})
export class VideoDetailComponent implements OnInit {
  video: Video | null = null;
  isLoading = true;
  videoId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private videoService: VideoService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.videoId = +params['id'];
        this.loadVideo();
      } else if (params['token']) {
        this.loadVideoByShareToken(params['token']);
      }
    });
  }

  loadVideo() {
    if (!this.videoId) return;
    
    console.log('Loading video with ID:', this.videoId);
    this.videoService.getVideoById(this.videoId).subscribe({
      next: (video) => {
        console.log('Video loaded successfully:', video);
        this.video = video;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load video:', err);
        console.error('Error details:', {
          status: err.status,
          statusText: err.statusText,
          message: err.message,
          error: err.error
        });
        this.isLoading = false;
        if (err.status === 404) {
          console.log('Video not found, redirecting to videos list');
          this.router.navigate(['/videos']);
        }
      }
    });
  }

  loadVideoByShareToken(token: string) {
    console.log('Loading video with share token:', token);
    this.videoService.getVideoByShareToken(token).subscribe({
      next: (video) => {
        console.log('Video loaded successfully via share token:', video);
        this.video = video;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load video via share token:', err);
        this.isLoading = false;
        if (err.status === 404) {
          console.log('Shared video not found');
          this.router.navigate(['/videos']);
        }
      }
    });
  }

  goBackToVideos() {
    this.router.navigate(['/videos']);
  }

  formatDuration(duration: string): string {
    const seconds = parseInt(duration);
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    } else {
      return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }
  }

  formatViewCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1) + 'M';
    } else if (count >= 1000) {
      return (count / 1000).toFixed(1) + 'K';
    }
    return count.toString();
  }

  @ViewChild('videoPlayer') videoPlayer!: VideoPlayerComponent;

  likeVideo() {
    if (this.videoPlayer) {
      this.videoPlayer.likeVideo();
    }
  }

  dislikeVideo() {
    if (this.videoPlayer) {
      this.videoPlayer.dislikeVideo();
    }
  }

  shareVideo() {
    if (this.videoPlayer) {
      this.videoPlayer.shareVideo();
    }
  }
}
