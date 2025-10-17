import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { VideoService } from '../../services/video.service';
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
  
  private subscriptions$ = new Subscription();

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private videoService: VideoService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Subscribe to current user
    this.subscriptions$.add(
      this.authService.currentUser$.subscribe(user => {
        this.currentUser = user;
        if (user) {
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
          profilePictureUrl: '/assets/default-avatar.png',
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
          profilePictureUrl: '/assets/default-avatar.png',
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
          profilePictureUrl: '/assets/default-avatar.png',
          subscriberCount: 500000
        },
        isActive: false,
        subscribedAt: new Date('2024-03-10')
      }
    ];
  }

  private loadVideoCounts(): void {
    // Mock data for now - in real implementation, these would come from services
    this.likedVideosCount = 45;
    this.historyCount = 128;
    this.watchLaterCount = 12;
    this.playlistsCount = 8;
  }

  navigateToChannel(channelId: string): void {
    this.router.navigate(['/channel', channelId]);
  }

  navigateToChannels(): void {
    this.router.navigate(['/channels']);
  }
}
