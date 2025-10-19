import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { SidebarService } from '../../services/sidebar.service';
import { UserService } from '../../services/user.service';
import { VideoService } from '../../services/video.service';
import { HistoryService } from '../../services/history.service';
import { WatchLaterService } from '../../services/watch-later.service';
import { PlaylistService } from '../../services/playlist.service';
import { SubscriptionService } from '../../services/subscription.service';
import { User } from '../../models/user.model';

interface ChannelSubscription {
  id: string;
  channelId: string;
  channel: {
    name: string;
    profilePictureUrl?: string;
    subscriberCount: number;
  };
  isActive: boolean;
  subscribedAt: Date;
}

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
    styleUrls: ['./sidebar.component.scss'],
    standalone: true,
    imports: [CommonModule, RouterLink, MatIconModule]
})
export class SidebarComponent implements OnInit, OnDestroy {
  subscriptions: ChannelSubscription[] = [];
  likedVideosCount = 0;
  historyCount = 0;
  watchLaterCount = 0;
  playlistsCount = 0;
  currentUser: User | null = null;
  isCollapsed$ = this.sidebarService.isCollapsed$;
  
  private subscriptions$ = new Subscription();

  constructor(
    private authService: AuthService,
    private sidebarService: SidebarService,
    private userService: UserService,
    private videoService: VideoService,
    private historyService: HistoryService,
    private watchLaterService: WatchLaterService,
    private playlistService: PlaylistService,
    private subscriptionService: SubscriptionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Subscribe to current user
    this.subscriptions$.add(
      this.authService.currentUser$.subscribe(user => {
        this.currentUser = user;
        if (user && this.authService.isUserLoaded()) {
          this.loadUserData();
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions$.unsubscribe();
  }

  private loadUserData(): void {
    if (!this.currentUser) return;

    // Load user's subscriptions
    this.loadSubscriptions();
    
    // Load user's video counts
    this.loadVideoCounts();
  }

  private loadSubscriptions(): void {
    console.log('Loading real subscriptions...');
    this.subscriptionService.getSubscribedChannels({ page: 1, pageSize: 10 }).subscribe({
      next: (result) => {
        console.log('Subscriptions API response:', result);
        console.log('Total subscriptions found:', result.totalCount);
        console.log('Items in response:', result.items?.length || 0);
        
        this.subscriptions = (result.items || []).map(channel => ({
          id: channel.id.toString(),
          channelId: channel.id.toString(),
          channel: {
            name: channel.name,
            profilePictureUrl: channel.profilePictureUrl || '/assets/default-avatar.svg',
            subscriberCount: channel.subscriberCount || 0
          },
          isActive: true, // Assume active if returned from API
          subscribedAt: new Date(channel.createdAt || new Date())
        }));

        // Load notification preferences for each subscription
        this.loadNotificationPreferences();
        
        console.log('Processed subscriptions for sidebar:', this.subscriptions);
        console.log('Number of subscriptions to display:', this.subscriptions.length);
      },
      error: (error) => {
        console.error('Error loading subscriptions:', error);
        console.error('Error details:', error);
        this.subscriptions = [];
      }
    });
  }

  private loadNotificationPreferences(): void {
    this.subscriptions.forEach(subscription => {
      this.subscriptionService.getNotificationPreference(parseInt(subscription.channelId)).subscribe({
        next: (enabled) => {
          subscription.isActive = enabled;
          console.log(`Notification preference for ${subscription.channel.name}:`, enabled);
        },
        error: (error) => {
          console.error(`Error loading notification preference for ${subscription.channel.name}:`, error);
          subscription.isActive = true; // Default to enabled
        }
      });
    });
  }

  private loadVideoCounts(): void {
    // Load actual counts from backend
    this.loadHistoryCount();
    this.loadLikedVideosCount();
    this.loadWatchLaterCount();
    this.loadPlaylistsCount();
  }

  private loadHistoryCount(): void {
    this.historyService.getHistoryCount().subscribe({
      next: (count) => {
        this.historyCount = count;
      },
      error: (error) => {
        console.error('Error loading history count:', error);
        this.historyCount = 0;
      }
    });
  }

  private loadLikedVideosCount(): void {
    this.videoService.getLikedVideosCount().subscribe({
      next: (count) => {
        this.likedVideosCount = count;
      },
      error: (error) => {
        console.error('Error loading liked videos count:', error);
        this.likedVideosCount = 0;
      }
    });
  }

  navigateToChannel(channelId: string): void {
    console.log('Navigating to channel:', channelId);
    this.router.navigate(['/channels', channelId]);
  }

  navigateToChannels(): void {
    this.router.navigate(['/channels']);
  }

  navigateToHistory(): void {
    console.log('Navigating to history page');
    this.router.navigate(['/history']);
  }

  private loadWatchLaterCount(): void {
    console.log('Loading watch later count...');
    this.watchLaterService.getWatchLaterCount().subscribe({
      next: (count) => {
        console.log('Watch later count loaded:', count);
        this.watchLaterCount = count;
      },
      error: (error) => {
        console.error('Error loading watch later count:', error);
        this.watchLaterCount = 0;
      }
    });
  }

  private loadPlaylistsCount(): void {
    console.log('Loading playlists count...');
    this.playlistService.getPlaylists(1, 1).subscribe({
      next: (result) => {
        console.log('Playlists count loaded:', result.totalCount);
        this.playlistsCount = result.totalCount;
      },
      error: (error) => {
        console.error('Error loading playlists count:', error);
        this.playlistsCount = 0;
      }
    });
  }
}
