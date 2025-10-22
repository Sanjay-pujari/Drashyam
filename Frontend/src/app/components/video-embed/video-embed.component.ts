import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { VideoService } from '../../services/video.service';
import { Video } from '../../models/video.model';
import { VideoPlayerComponent } from '../video-player/video-player.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-video-embed',
  standalone: true,
  imports: [
    CommonModule,
    VideoPlayerComponent,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './video-embed.component.html',
  styleUrls: ['./video-embed.component.scss']
})
export class VideoEmbedComponent implements OnInit, OnDestroy {
  video: Video | null = null;
  isLoading = true;
  error: string | null = null;
  private token: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private videoService: VideoService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.token = params['token'];
      if (this.token) {
        this.loadVideoByShareToken(this.token);
      }
    });
  }

  ngOnDestroy() {
    // Cleanup if needed
  }

  loadVideoByShareToken(token: string) {
    this.isLoading = true;
    this.error = null;

    this.videoService.getVideoByShareToken(token).subscribe({
      next: (video) => {
        this.video = video;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading video:', err);
        this.error = 'Video not found or access denied';
        this.isLoading = false;
      }
    });
  }

  openInNewTab() {
    if (this.token) {
      const url = `${window.location.origin}/watch/${this.token}`;
      window.open(url, '_blank');
    }
  }

  goToChannel() {
    if (this.video?.channel?.id) {
      const url = `${window.location.origin}/channels/${this.video.channel.id}`;
      window.open(url, '_blank');
    }
  }

  getChannelName(): string {
    if (this.video?.channel?.name) {
      return this.video.channel.name;
    }
    if (this.video?.user?.firstName && this.video?.user?.lastName) {
      return `${this.video.user.firstName} ${this.video.user.lastName}`;
    }
    return 'Unknown Channel';
  }

  getChannelAvatar(): string {
    if (this.video?.channel?.profilePictureUrl) {
      return this.video.channel.profilePictureUrl;
    }
    if (this.video?.user?.profilePictureUrl) {
      return this.video.user.profilePictureUrl;
    }
    return '/assets/default-avatar.png';
  }

  formatViewCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1) + 'M';
    }
    if (count >= 1000) {
      return (count / 1000).toFixed(1) + 'K';
    }
    return count.toString();
  }

  formatSubscriberCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1) + 'M';
    }
    if (count >= 1000) {
      return (count / 1000).toFixed(1) + 'K';
    }
    return count.toString();
  }
}
