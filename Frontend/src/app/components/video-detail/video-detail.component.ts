import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';
import { likeVideo } from '../../store/video/video.actions';
import { VideoService } from '../../services/video.service';
import { Video } from '../../models/video.model';
import { VideoPlayerComponent } from '../video-player/video-player.component';
import { AppState } from '../../store/app.state';
import { selectVideoById } from '../../store/video/video.selectors';

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
    private videoService: VideoService,
    private store: Store<AppState>
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

    // Subscribe to video updates from the store
    if (this.videoId) {
      this.store.select(selectVideoById, { id: this.videoId }).subscribe(updatedVideo => {
        if (updatedVideo && this.video && updatedVideo.id === this.video.id) {
          this.video = updatedVideo;
        }
      });
    }
  }

  loadVideo() {
    if (!this.videoId) return;
    
    this.videoService.getVideoById(this.videoId).subscribe({
      next: (video) => {
        this.video = video;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 404) {
          this.router.navigate(['/videos']);
        }
      }
    });
  }

  loadVideoByShareToken(token: string) {
    this.videoService.getVideoByShareToken(token).subscribe({
      next: (video) => {
        this.video = video;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 404) {
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

  getChannelName(): string {
    if (this.video?.channel) {
      return this.video.channel.name;
    } else if (this.video?.user) {
      return `${this.video.user.firstName} ${this.video.user.lastName}`;
    }
    return 'Unknown Channel';
  }

  getChannelAvatar(): string {
    if (this.video?.channel?.profilePictureUrl) {
      return this.video.channel.profilePictureUrl;
    } else if (this.video?.user?.profilePictureUrl) {
      return this.video.user.profilePictureUrl;
    }
    return '/assets/default-avatar.svg';
  }

  toggleFavorite() {
    if (!this.video) return;
    const wasLiked = !!this.video.isLiked;
    const currentLikes = Number(this.video.likeCount || 0);
    
    // Create a new video object with updated like counts to avoid readonly error
    this.video = {
      ...this.video,
      isLiked: !wasLiked,
      likeCount: Math.max(0, currentLikes + (wasLiked ? -1 : 1))
    };
    
    this.store.dispatch(likeVideo({ videoId: this.video.id, likeType: wasLiked ? 'dislike' : 'like' }));
  }

  navigateToChannel() {
    if (this.video?.channel) {
      this.router.navigate(['/channels', this.video.channel.id]);
    } else if (this.video?.user) {
      // For users without channels, you might want to create a user profile page
    }
  }

  formatSubscriberCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1) + 'M';
    } else if (count >= 1000) {
      return (count / 1000).toFixed(1) + 'K';
    }
    return count.toString();
  }

}
