import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { Store } from '@ngrx/store';
import { Inject } from '@angular/core';
import { Observable, Subscription, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { AppState } from '../../store/app.state';
import { Video } from '../../models/video.model';
import { User } from '../../models/user.model';
import { selectVideos, selectVideoLoading, selectVideoError } from '../../store/video/video.selectors';
import { selectCurrentUser } from '../../store/user/user.selectors';
import { loadVideos, likeVideo } from '../../store/video/video.actions';
import { VideoService } from '../../services/video.service';
import { ChannelService } from '../../services/channel.service';
import { AdBannerComponent } from '../ad-banner/ad-banner.component';

@Component({
    selector: 'app-home',
    standalone: true,
    imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, MatButtonModule, MatChipsModule, AdBannerComponent],
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  videos$: Observable<Video[]>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  currentUser$: Observable<User | null>;
  
  trendingVideos: Video[] = [];
  recommendedVideos: Video[] = [];
  subscribedVideos: Video[] = [];
  categories = [
    'All',
    'Music',
    'Gaming',
    'Education',
    'Entertainment',
    'Technology',
    'Sports',
    'News',
    'Lifestyle',
    'Travel'
  ];
  
  selectedCategory = 'All';
  
  // Search properties
  searchQuery = '';
  searchResults: Video[] = [];
  isSearchMode = false;
  isSearching = false;
  
  private subscriptions: Subscription[] = [];

  constructor(
    @Inject(Store) private store: Store<AppState>, 
    private videoService: VideoService, 
    private channelService: ChannelService,
    private router: Router
  ) {
    this.videos$ = this.store.select(selectVideos);
    this.loading$ = this.store.select(selectVideoLoading);
    this.error$ = this.store.select(selectVideoError);
    this.currentUser$ = this.store.select(selectCurrentUser);
  }

  ngOnInit() {
    this.loadInitialVideos();
    this.loadHomeFeed();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadInitialVideos() {
    // Load main videos
    this.store.dispatch(loadVideos({ page: 1, pageSize: 20 }));
  }

  loadHomeFeed() {
    const sub = this.videoService.getHomeFeed({ page: 1, pageSize: 10 }).subscribe({
      next: (feed) => {
        this.trendingVideos = feed.trending.items || [];
        this.recommendedVideos = feed.recommended.items || [];
        this.subscribedVideos = feed.subscribed.items || [];
      },
      error: (error) => {
        console.error('Error loading home feed:', error);
        // If home feed fails, we still have the main videos from loadInitialVideos()
        this.trendingVideos = [];
        this.recommendedVideos = [];
        this.subscribedVideos = [];
      }
    });
    this.subscriptions.push(sub);
  }

  onCategoryChange(category: string) {
    this.selectedCategory = category;
    if (category === 'All') {
      this.store.dispatch(loadVideos({ page: 1, pageSize: 20 }));
    } else {
      this.store.dispatch(loadVideos({ 
        page: 1, 
        pageSize: 20, 
        category: category 
      }));
    }
  }

  onVideoClick(video: Video) {
    // Navigate to video detail page
    console.log('Navigate to video:', video.id);
    this.router.navigate(['/videos', video.id]);
  }

  onChannelClick(channelId: number) {
    // Navigate to channel page
    console.log('Navigate to channel:', channelId);
    this.router.navigate(['/channels', channelId]);
  }

  toggleFavorite(video: Video) {
    // Optimistic UI update
    const wasLiked = !!video.isLiked;
    video.isLiked = !wasLiked;
    const currentLikes = Number(video.likeCount || 0);
    video.likeCount = Math.max(0, currentLikes + (wasLiked ? -1 : 1));

    const likeType = wasLiked ? 'dislike' : 'like';
    this.store.dispatch(likeVideo({ videoId: video.id, likeType }));
  }

  toggleSubscribe(video: Video) {
    if (!video.channelId) return;
    const action$ = video.channel?.isSubscribed ? this.channelService.unsubscribeFromChannel(video.channelId) : this.channelService.subscribeToChannel(video.channelId);
    const sub = action$.subscribe(() => {
      // TODO: Implement proper state management for subscription updates
      // For now, we'll just log the action - the UI will update on next data load
      console.log('Subscription updated for channel:', video.channelId);
    });
    this.subscriptions.push(sub);
  }

  loadMoreVideos() {
    // Load more videos for pagination
    console.log('Load more videos');
  }

  onSearchInput(event: any) {
    const query = event.target.value.trim();
    if (query.length === 0) {
      this.clearSearch();
    }
  }

  onSearch() {
    const query = this.searchQuery.trim();
    if (query.length === 0) {
      this.clearSearch();
      return;
    }

    this.isSearchMode = true;
    this.isSearching = true;
    this.searchResults = [];

    const searchSub = this.videoService.searchVideos(query, { page: 1, pageSize: 20 }).subscribe({
      next: (result) => {
        this.searchResults = result.items || [];
        this.isSearching = false;
      },
      error: (error) => {
        console.error('Search error:', error);
        this.searchResults = [];
        this.isSearching = false;
      }
    });

    this.subscriptions.push(searchSub);
  }

  clearSearch() {
    this.searchQuery = '';
    this.searchResults = [];
    this.isSearchMode = false;
    this.isSearching = false;
  }

  trackByVideoId(index: number, video: Video): number {
    return video.id;
  }

  formatDuration(duration: string | any): string {
    // Handle both string and TimeSpan object from backend
    let seconds: number;
    
    if (typeof duration === 'string') {
      // If it's a string, try to parse it
      seconds = parseInt(duration);
    } else if (duration && typeof duration === 'object') {
      // If it's a TimeSpan object from backend
      seconds = Math.floor(duration.ticks / 10000000); // Convert ticks to seconds
    } else {
      // Fallback to 0 if duration is invalid
      seconds = 0;
    }
    
    // If seconds is NaN or 0, return 0:00
    if (isNaN(seconds) || seconds === 0) {
      return '0:00';
    }

    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    } else {
      return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }
  }
}
