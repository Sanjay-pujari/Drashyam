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
    // This would typically call a service to get user's channel subscriptions
    // For now, we'll use mock data
    this.subscriptions = [
      {
        id: '1',
        channelId: 'channel1',
        channel: {
          name: 'Tech Channel',
          profilePictureUrl: '/assets/default-avatar.svg',
          subscriberCount: 150000
        },
        isActive: true,
        subscribedAt: new Date('2024-01-15')
      },
      {
        id: '2',
        channelId: 'channel2',
        channel: {
          name: 'Gaming Hub',
          profilePictureUrl: '/assets/default-avatar.svg',
          subscriberCount: 250000
        },
        isActive: true,
        subscribedAt: new Date('2024-02-20')
      },
      {
        id: '3',
        channelId: 'channel3',
        channel: {
          name: 'Music World',
          profilePictureUrl: '/assets/default-avatar.svg',
          subscriberCount: 500000
        },
        isActive: false,
        subscribedAt: new Date('2024-03-10')
      }
    ];
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
    this.watchLaterService.getWatchLaterCount().subscribe({
      next: (count) => {
        this.watchLaterCount = count;
      },
      error: (error) => {
        console.error('Error loading watch later count:', error);
        this.watchLaterCount = 0;
      }
    });
  }

  private loadPlaylistsCount(): void {
    this.playlistService.getPlaylists(1, 1).subscribe({
      next: (result) => {
        this.playlistsCount = result.totalCount;
      },
      error: (error) => {
        console.error('Error loading playlists count:', error);
        this.playlistsCount = 0;
      }
    });
  }
}
