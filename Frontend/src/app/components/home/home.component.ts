import { Component, OnInit, OnDestroy } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable, Subscription } from 'rxjs';
import { AppState } from '../../store/app.state';
import { Video } from '../../models/video.model';
import { selectVideos, selectVideoLoading, selectVideoError } from '../../store/video/video.selectors';
import { loadVideos, loadTrendingVideos, loadRecommendedVideos } from '../../store/video/video.actions';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  videos$: Observable<Video[]>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  
  trendingVideos: Video[] = [];
  recommendedVideos: Video[] = [];
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
  private subscriptions: Subscription[] = [];

  constructor(private store: Store<AppState>) {
    this.videos$ = this.store.select(selectVideos);
    this.loading$ = this.store.select(selectVideoLoading);
    this.error$ = this.store.select(selectVideoError);
  }

  ngOnInit() {
    this.loadInitialVideos();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private loadInitialVideos() {
    // Load main videos
    this.store.dispatch(loadVideos({ page: 1, pageSize: 20 }));
    
    // Load trending videos
    this.store.dispatch(loadTrendingVideos({ page: 1, pageSize: 10 }));
    
    // Load recommended videos
    this.store.dispatch(loadRecommendedVideos({ page: 1, pageSize: 10 }));
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
  }

  onChannelClick(channelId: number) {
    // Navigate to channel page
    console.log('Navigate to channel:', channelId);
  }

  loadMoreVideos() {
    // Load more videos for pagination
    console.log('Load more videos');
  }

  trackByVideoId(index: number, video: Video): number {
    return video.id;
  }

  formatDuration(duration: string): string {
    // Convert duration string to readable format
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
}
