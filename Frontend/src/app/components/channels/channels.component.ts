import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ChannelService } from '../../services/channel.service';
import { Channel } from '../../models/channel.model';

@Component({
	selector: 'app-channels',
	standalone: true,
	imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
	templateUrl: './channels.component.html',
	styleUrls: ['./channels.component.scss']
})
export class ChannelsComponent implements OnInit {
  channels: Channel[] = [];
  isLoading = true;
  error: string | null = null;

  constructor(
    private channelService: ChannelService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadChannels();
  }

  loadChannels() {
    console.log('Loading channels...');
    this.channelService.getChannels({ page: 1, pageSize: 20 }).subscribe({
      next: (result) => {
        console.log('Channels loaded successfully:', result);
        this.channels = result.items;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load channels:', err);
        console.error('Error details:', {
          status: err.status,
          statusText: err.statusText,
          message: err.message,
          error: err.error
        });
        this.error = 'Failed to load channels. Please try again.';
        this.isLoading = false;
        if (err.status === 0) {
          this.error = 'Unable to connect to the server. Please make sure the backend API is running.';
        }
      }
    });
  }

  goToChannel(channelId: number) {
    this.router.navigate(['/channels', channelId]);
  }

  formatSubscriberCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1) + 'M';
    } else if (count >= 1000) {
      return (count / 1000).toFixed(1) + 'K';
    }
    return count.toString();
  }

  getSafeImageUrl(imageUrl: string | null | undefined): string {
    // Check if the URL is from a known problematic Azure Storage account
    if (imageUrl && (
      imageUrl.includes('storage.blob.core.windows.net') || 
      imageUrl.includes('drashyamstorage.blob.core.windows.net')
    )) {
      // Return default avatar for known problematic URLs
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








