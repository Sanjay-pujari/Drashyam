import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ChannelService } from '../../services/channel.service';
import { VideoService } from '../../services/video.service';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';
import { MonetizationService } from '../../services/monetization.service';
import { Channel } from '../../models/channel.model';
import { Video } from '../../models/video.model';

@Component({
  selector: 'app-channel-detail',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatButtonModule, MatIconModule, 
    MatTabsModule, MatChipsModule, MatProgressSpinnerModule, MatSlideToggleModule
  ],
  templateUrl: './channel-detail.component.html',
  styleUrls: ['./channel-detail.component.scss']
})
export class ChannelDetailComponent implements OnInit {
  channel: Channel | null = null;
  videos: Video[] = [];
  merchandise: any[] = [];
  isLoading = true;
  isSubscribed = false;
  isOwner = false;
  channelId: number | null = null;
  notificationsEnabled = true;
  isUpdatingNotifications = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private channelService: ChannelService,
    private videoService: VideoService,
    private authService: AuthService,
    private cartService: CartService,
    private monetizationService: MonetizationService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.channelId = +params['id'];
      if (this.channelId) {
        this.loadChannel();
        this.loadChannelVideos();
        this.loadChannelMerchandise();
        this.checkSubscription();
        this.checkOwnership();
      } else {
      }
    });
  }

  loadChannel() {
    if (!this.channelId) return;
    
    this.channelService.getChannel(this.channelId).subscribe({
      next: (channel) => {
        this.channel = channel;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 404) {
          this.router.navigate(['/channels']);
        } else if (err.status === 0) {
        }
      }
    });
  }

  loadChannelVideos() {
    if (!this.channelId) return;
    
    this.videoService.getVideosByChannel(this.channelId).subscribe({
      next: (videos) => {
        this.videos = videos;
      },
      error: (err) => {
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
        }
      });
    } else {
      this.channelService.subscribe(this.channelId).subscribe({
        next: () => {
          this.isSubscribed = true;
          if (this.channel) {
            this.channel.subscriberCount++;
          }
          // Load notification preference after subscribing
          this.loadNotificationPreference();
        },
        error: (err) => {
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

  toggleNotifications(enabled: boolean) {
    if (!this.channelId) return;
    
    this.isUpdatingNotifications = true;
    this.channelService.updateNotificationPreference(this.channelId, enabled).subscribe({
      next: () => {
        this.notificationsEnabled = enabled;
        this.isUpdatingNotifications = false;
      },
      error: (error) => {
        this.isUpdatingNotifications = false;
        // Revert the toggle state
        this.notificationsEnabled = !enabled;
      }
    });
  }

  private loadNotificationPreference() {
    if (!this.channelId || !this.isSubscribed) return;
    
    this.channelService.getNotificationPreference(this.channelId).subscribe({
      next: (enabled) => {
        this.notificationsEnabled = enabled;
      },
      error: (error) => {
        this.notificationsEnabled = true; // Default to enabled
      }
    });
  }

  loadChannelMerchandise() {
    if (!this.channelId) return;
    
    console.log('Loading merchandise for channel:', this.channelId);
    
    this.monetizationService.getChannelMerchandise(this.channelId).subscribe({
      next: (merchandise) => {
        console.log('Merchandise loaded successfully:', merchandise);
        this.merchandise = merchandise;
      },
      error: (error) => {
        console.error('Error loading merchandise:', error);
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          url: error.url
        });
        this.merchandise = [];
      }
    });
  }

  viewMerchandise(merchandiseId: number) {
    // For now, we'll just log the merchandise ID
    // In a real implementation, you'd navigate to a merchandise detail page
    console.log('Viewing merchandise:', merchandiseId);
    // this.router.navigate(['/merchandise', merchandiseId]);
  }

  addToCart(item: any) {
    if (!this.authService.isAuthenticated()) {
      alert('Please login to add items to cart');
      this.router.navigate(['/login']);
      return;
    }

    const cartItem = {
      id: item.id,
      name: item.name,
      price: item.price,
      currency: item.currency,
      imageUrl: item.imageUrl,
      channelId: item.channelId,
      channelName: item.channelName,
      sizes: item.sizes,
      colors: item.colors
    };

    this.cartService.addItem(cartItem);
    alert(`${item.name} added to cart!`);
  }

  isInCart(item: any): boolean {
    return this.cartService.isItemInCart(item.id);
  }
}
