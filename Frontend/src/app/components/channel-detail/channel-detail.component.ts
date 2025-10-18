import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ChannelService } from '../../services/channel.service';
import { VideoService } from '../../services/video.service';
import { AuthService } from '../../services/auth.service';
import { Channel } from '../../models/channel.model';
import { Video } from '../../models/video.model';

@Component({
  selector: 'app-channel-detail',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatButtonModule, MatIconModule, 
    MatTabsModule, MatChipsModule, MatProgressSpinnerModule
  ],
  templateUrl: './channel-detail.component.html',
  styleUrls: ['./channel-detail.component.scss']
})
export class ChannelDetailComponent implements OnInit {
  channel: Channel | null = null;
  videos: Video[] = [];
  isLoading = true;
  isSubscribed = false;
  isOwner = false;
  channelId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private channelService: ChannelService,
    private videoService: VideoService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.channelId = +params['id'];
      console.log('Channel ID from route:', this.channelId);
      if (this.channelId) {
        this.loadChannel();
        this.loadChannelVideos();
        this.checkSubscription();
        this.checkOwnership();
      } else {
        console.error('Invalid channel ID:', params['id']);
      }
    });
  }

  loadChannel() {
    if (!this.channelId) return;
    
    console.log('Loading channel with ID:', this.channelId);
    this.channelService.getChannel(this.channelId).subscribe({
      next: (channel) => {
        console.log('Channel loaded successfully:', channel);
        this.channel = channel;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load channel:', err);
        console.error('Error details:', {
          status: err.status,
          statusText: err.statusText,
          message: err.message,
          error: err.error
        });
        this.isLoading = false;
        if (err.status === 404) {
          console.log('Channel not found, redirecting to channels list');
          this.router.navigate(['/channels']);
        } else if (err.status === 0) {
          console.error('Network error - API might not be running or CORS issue');
        }
      }
    });
  }

  loadChannelVideos() {
    if (!this.channelId) return;
    
    console.log('Loading videos for channel ID:', this.channelId);
    this.videoService.getVideosByChannel(this.channelId).subscribe({
      next: (videos) => {
        console.log('Videos loaded successfully:', videos);
        this.videos = videos;
      },
      error: (err) => {
        console.error('Failed to load channel videos:', err);
        console.error('Video loading error details:', {
          status: err.status,
          statusText: err.statusText,
          message: err.message,
          error: err.error
        });
      }
    });
  }

  checkSubscription() {
    if (!this.channelId) return;
    
    this.channelService.getSubscriptionStatus(this.channelId).subscribe({
      next: (status) => {
        this.isSubscribed = status.isSubscribed;
      },
      error: (err) => {
        console.error('Failed to check subscription status:', err);
      }
    });
  }

  checkOwnership() {
    this.authService.currentUser$.subscribe(user => {
      if (user && this.channel) {
        this.isOwner = user.id === this.channel.userId;
      }
    });
  }

  toggleSubscription() {
    if (!this.channelId) return;
    
    if (this.isSubscribed) {
      this.channelService.unsubscribe(this.channelId).subscribe({
        next: () => {
          this.isSubscribed = false;
          if (this.channel) {
            this.channel.subscriberCount = Math.max(0, this.channel.subscriberCount - 1);
          }
        },
        error: (err) => {
          console.error('Failed to unsubscribe:', err);
        }
      });
    } else {
      this.channelService.subscribe(this.channelId).subscribe({
        next: () => {
          this.isSubscribed = true;
          if (this.channel) {
            this.channel.subscriberCount++;
          }
        },
        error: (err) => {
          console.error('Failed to subscribe:', err);
        }
      });
    }
  }

  editChannel() {
    if (this.channelId) {
      this.router.navigate(['/channel/edit', this.channelId]);
    }
  }

  goToVideo(videoId: number) {
    this.router.navigate(['/video', videoId]);
  }

  formatSubscriberCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1) + 'M';
    } else if (count >= 1000) {
      return (count / 1000).toFixed(1) + 'K';
    }
    return count.toString();
  }

  goBackToChannels() {
    this.router.navigate(['/channels']);
  }

  getSafeImageUrl(imageUrl: string | null | undefined): string {
    // Check if the URL is from a known problematic Azure Storage account
    if (imageUrl && (
      imageUrl.includes('storage.blob.core.windows.net') || 
      imageUrl.includes('drashyamstorage.blob.core.windows.net')
    )) {
      // Return default image for known problematic URLs
      return '/assets/default-avatar.svg';
    }
    return imageUrl || '/assets/default-avatar.svg';
  }

  onImageError(event: any) {
    // Only log once per image to reduce console noise
    if (!event.target.dataset.fallbackApplied) {
      console.log('Image failed to load, using default image');
      event.target.dataset.fallbackApplied = 'true';
    }
    
    // Check if it's a video thumbnail or profile picture
    if (event.target.alt && event.target.alt.includes('avatar')) {
      event.target.src = '/assets/default-avatar.svg';
    } else {
      // Default video thumbnail
      event.target.src = '/assets/default-video-thumbnail.svg';
    }
  }
}
