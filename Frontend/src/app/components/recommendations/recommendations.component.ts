import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { RecommendationService, Recommendation, TrendingVideo, UserPreference } from '../../services/recommendation.service';
import { VideoService } from '../../services/video.service';

@Component({
  selector: 'app-recommendations',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatChipsModule,
    MatSnackBarModule
  ],
  template: `
    <div class="recommendations-container">
      <div class="recommendations-header">
        <h1>Discover Videos</h1>
        <p>Find your next favorite video with personalized recommendations</p>
      </div>

      <mat-tab-group class="recommendations-tabs" [(selectedIndex)]="selectedTabIndex">
        <!-- Personalized Recommendations Tab -->
        <mat-tab label="For You">
          <div class="tab-content">
            <div class="recommendations-grid" *ngIf="!isLoadingPersonalized && personalizedRecommendations.length > 0">
              <mat-card class="video-card" *ngFor="let recommendation of personalizedRecommendations" 
                        (click)="onVideoClick(recommendation.video, recommendation.id)">
                <div class="video-thumbnail">
                  <img [src]="recommendation.video.thumbnailUrl || '/assets/default-video-thumbnail.svg'" 
                       [alt]="recommendation.video.title"
                       class="thumbnail-image">
                  <div class="video-duration">{{ formatDuration(recommendation.video.duration) }}</div>
                  <div class="recommendation-badge" [class]="getRecommendationBadgeClass(recommendation.type)">
                    {{ getRecommendationTypeLabel(recommendation.type) }}
                  </div>
                </div>
                <mat-card-content>
                  <h3 class="video-title">{{ recommendation.video.title }}</h3>
                  <p class="video-channel">{{ recommendation.video.user?.firstName }} {{ recommendation.video.user?.lastName }}</p>
                  <div class="video-stats">
                    <span class="views">{{ formatNumber(recommendation.video.viewCount) }} views</span>
                    <span class="date">{{ formatDate(recommendation.video.createdAt) }}</span>
                  </div>
                  <p class="recommendation-reason" *ngIf="recommendation.reason">{{ recommendation.reason }}</p>
                </mat-card-content>
                <mat-card-actions>
                  <button mat-icon-button (click)="onLikeClick(recommendation.video, $event)">
                    <mat-icon [class.liked]="recommendation.video.isLiked">thumb_up</mat-icon>
                  </button>
                  <button mat-icon-button (click)="onDislikeClick(recommendation.video, $event)">
                    <mat-icon [class.disliked]="recommendation.video.isDisliked">thumb_down</mat-icon>
                  </button>
                  <button mat-icon-button (click)="onShareClick(recommendation.video, $event)">
                    <mat-icon>share</mat-icon>
                  </button>
                </mat-card-actions>
              </mat-card>
            </div>
            <div class="loading-container" *ngIf="isLoadingPersonalized">
              <mat-spinner></mat-spinner>
              <p>Loading personalized recommendations...</p>
            </div>
            <div class="empty-state" *ngIf="!isLoadingPersonalized && personalizedRecommendations.length === 0">
              <mat-icon>recommend</mat-icon>
              <h3>No recommendations yet</h3>
              <p>Watch some videos to get personalized recommendations</p>
              <button mat-raised-button color="primary" (click)="loadPersonalizedRecommendations()">
                Refresh Recommendations
              </button>
            </div>
          </div>
        </mat-tab>

        <!-- Trending Videos Tab -->
        <mat-tab label="Trending">
          <div class="tab-content">
            <div class="trending-filters">
              <mat-chip-listbox>
                <mat-chip-option (click)="loadTrendingVideos()" [selected]="selectedTrendingCategory === null">
                  All
                </mat-chip-option>
                <mat-chip-option *ngFor="let category of trendingCategories" 
                                 (click)="loadTrendingVideos(category)"
                                 [selected]="selectedTrendingCategory === category">
                  {{ category }}
                </mat-chip-option>
              </mat-chip-listbox>
            </div>
            <div class="recommendations-grid" *ngIf="!isLoadingTrending && trendingVideos.length > 0">
              <mat-card class="video-card trending-card" *ngFor="let trending of trendingVideos; let i = index" 
                        (click)="onVideoClick(trending.video, trending.id)">
                <div class="trending-position">#{{ i + 1 }}</div>
                <div class="video-thumbnail">
                  <img [src]="trending.video.thumbnailUrl || '/assets/default-video-thumbnail.svg'" 
                       [alt]="trending.video.title"
                       class="thumbnail-image">
                  <div class="video-duration">{{ formatDuration(trending.video.duration) }}</div>
                  <div class="trending-badge">TRENDING</div>
                </div>
                <mat-card-content>
                  <h3 class="video-title">{{ trending.video.title }}</h3>
                  <p class="video-channel">{{ trending.video.user?.firstName }} {{ trending.video.user?.lastName }}</p>
                  <div class="video-stats">
                    <span class="views">{{ formatNumber(trending.video.viewCount) }} views</span>
                    <span class="date">{{ formatDate(trending.video.createdAt) }}</span>
                  </div>
                  <div class="trending-score">Trending Score: {{ trending.trendingScore.toFixed(2) }}</div>
                </mat-card-content>
                <mat-card-actions>
                  <button mat-icon-button (click)="onLikeClick(trending.video, $event)">
                    <mat-icon [class.liked]="trending.video.isLiked">thumb_up</mat-icon>
                  </button>
                  <button mat-icon-button (click)="onDislikeClick(trending.video, $event)">
                    <mat-icon [class.disliked]="trending.video.isDisliked">thumb_down</mat-icon>
                  </button>
                  <button mat-icon-button (click)="onShareClick(trending.video, $event)">
                    <mat-icon>share</mat-icon>
                  </button>
                </mat-card-actions>
              </mat-card>
            </div>
            <div class="loading-container" *ngIf="isLoadingTrending">
              <mat-spinner></mat-spinner>
              <p>Loading trending videos...</p>
            </div>
            <div class="empty-state" *ngIf="!isLoadingTrending && trendingVideos.length === 0">
              <mat-icon>trending_up</mat-icon>
              <h3>No trending videos</h3>
              <p>Check back later for trending content</p>
            </div>
          </div>
        </mat-tab>

        <!-- Categories Tab -->
        <mat-tab label="Categories">
          <div class="tab-content">
            <div class="category-grid">
              <mat-card class="category-card" *ngFor="let category of categories" 
                        (click)="loadCategoryRecommendations(category)">
                <mat-card-content>
                  <mat-icon class="category-icon">{{ getCategoryIcon(category) }}</mat-icon>
                  <h3>{{ category }}</h3>
                  <p>Explore {{ category.toLowerCase() }} videos</p>
                </mat-card-content>
              </mat-card>
            </div>
            <div class="recommendations-grid" *ngIf="categoryRecommendations.length > 0">
              <mat-card class="video-card" *ngFor="let recommendation of categoryRecommendations" 
                        (click)="onVideoClick(recommendation.video, recommendation.id)">
                <div class="video-thumbnail">
                  <img [src]="recommendation.video.thumbnailUrl || '/assets/default-video-thumbnail.svg'" 
                       [alt]="recommendation.video.title"
                       class="thumbnail-image">
                  <div class="video-duration">{{ formatDuration(recommendation.video.duration) }}</div>
                </div>
                <mat-card-content>
                  <h3 class="video-title">{{ recommendation.video.title }}</h3>
                  <p class="video-channel">{{ recommendation.video.user?.firstName }} {{ recommendation.video.user?.lastName }}</p>
                  <div class="video-stats">
                    <span class="views">{{ formatNumber(recommendation.video.viewCount) }} views</span>
                    <span class="date">{{ formatDate(recommendation.video.createdAt) }}</span>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        </mat-tab>
      </mat-tab-group>
    </div>
  `,
  styles: [`
    .recommendations-container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 2rem;
    }

    .recommendations-header {
      text-align: center;
      margin-bottom: 2rem;
    }

    .recommendations-header h1 {
      color: #333;
      margin-bottom: 0.5rem;
    }

    .recommendations-header p {
      color: #666;
      margin: 0;
    }

    .recommendations-tabs {
      margin-top: 2rem;
    }

    .tab-content {
      padding: 1rem 0;
    }

    .recommendations-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1.5rem;
      margin-top: 1rem;
    }

    .video-card {
      cursor: pointer;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
      position: relative;
    }

    .video-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }

    .video-thumbnail {
      position: relative;
      width: 100%;
      height: 200px;
      overflow: hidden;
    }

    .thumbnail-image {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .video-duration {
      position: absolute;
      bottom: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.8);
      color: white;
      padding: 2px 6px;
      border-radius: 4px;
      font-size: 12px;
    }

    .recommendation-badge {
      position: absolute;
      top: 8px;
      left: 8px;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 10px;
      font-weight: bold;
      text-transform: uppercase;
    }

    .recommendation-badge.personalized {
      background: #4caf50;
      color: white;
    }

    .recommendation-badge.collaborative {
      background: #2196f3;
      color: white;
    }

    .recommendation-badge.trending {
      background: #ff9800;
      color: white;
    }

    .trending-badge {
      position: absolute;
      top: 8px;
      right: 8px;
      background: #f44336;
      color: white;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 10px;
      font-weight: bold;
      text-transform: uppercase;
    }

    .trending-position {
      position: absolute;
      top: 8px;
      left: 8px;
      background: rgba(0, 0, 0, 0.8);
      color: white;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: bold;
    }

    .video-title {
      font-size: 16px;
      font-weight: 500;
      margin: 0.5rem 0;
      line-height: 1.3;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .video-channel {
      color: #666;
      font-size: 14px;
      margin: 0.25rem 0;
    }

    .video-stats {
      display: flex;
      justify-content: space-between;
      font-size: 12px;
      color: #666;
      margin: 0.5rem 0;
    }

    .recommendation-reason {
      font-size: 12px;
      color: #4caf50;
      font-style: italic;
      margin: 0.5rem 0 0 0;
    }

    .trending-score {
      font-size: 12px;
      color: #ff9800;
      font-weight: 500;
      margin: 0.5rem 0 0 0;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 3rem;
    }

    .loading-container p {
      margin-top: 1rem;
      color: #666;
    }

    .empty-state {
      text-align: center;
      padding: 3rem;
    }

    .empty-state mat-icon {
      font-size: 48px;
      color: #ccc;
      margin-bottom: 1rem;
    }

    .empty-state h3 {
      color: #333;
      margin-bottom: 0.5rem;
    }

    .empty-state p {
      color: #666;
      margin-bottom: 1rem;
    }

    .trending-filters {
      margin-bottom: 1rem;
    }

    .category-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .category-card {
      cursor: pointer;
      text-align: center;
      transition: transform 0.2s ease;
    }

    .category-card:hover {
      transform: translateY(-2px);
    }

    .category-icon {
      font-size: 48px;
      color: #1976d2;
      margin-bottom: 1rem;
    }

    .liked {
      color: #4caf50 !important;
    }

    .disliked {
      color: #f44336 !important;
    }

    @media (max-width: 768px) {
      .recommendations-container {
        padding: 1rem;
      }

      .recommendations-grid {
        grid-template-columns: 1fr;
      }

      .category-grid {
        grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
      }
    }
  `]
})
export class RecommendationsComponent implements OnInit, OnDestroy {
  personalizedRecommendations: Recommendation[] = [];
  trendingVideos: TrendingVideo[] = [];
  categoryRecommendations: Recommendation[] = [];
  categories: string[] = ['Gaming', 'Music', 'Sports', 'Education', 'Entertainment', 'Technology', 'Lifestyle', 'News'];
  trendingCategories: string[] = ['Gaming', 'Music', 'Sports', 'Education', 'Entertainment', 'Technology'];
  selectedTrendingCategory: string | null = null;
  selectedTabIndex = 0;
  
  isLoadingPersonalized = false;
  isLoadingTrending = false;
  
  private subscriptions: Subscription[] = [];

  constructor(
    private recommendationService: RecommendationService,
    private videoService: VideoService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadPersonalizedRecommendations();
    this.loadTrendingVideos();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadPersonalizedRecommendations(): void {
    this.isLoadingPersonalized = true;
    this.subscriptions.push(
      this.recommendationService.getPersonalizedRecommendations({
        limit: 20,
        includeTrending: true,
        includePersonalized: true
      }).subscribe({
        next: (recommendations) => {
          this.personalizedRecommendations = recommendations;
          this.isLoadingPersonalized = false;
        },
        error: (error) => {
          console.error('Error loading personalized recommendations:', error);
          this.snackBar.open('Failed to load recommendations', 'Close', { duration: 3000 });
          this.isLoadingPersonalized = false;
        }
      })
    );
  }

  loadTrendingVideos(category?: string): void {
    this.isLoadingTrending = true;
    this.selectedTrendingCategory = category || null;
    
    this.subscriptions.push(
      this.recommendationService.getTrendingVideos(category, undefined, 20).subscribe({
        next: (trending) => {
          this.trendingVideos = trending;
          this.isLoadingTrending = false;
        },
        error: (error) => {
          console.error('Error loading trending videos:', error);
          this.snackBar.open('Failed to load trending videos', 'Close', { duration: 3000 });
          this.isLoadingTrending = false;
        }
      })
    );
  }

  loadCategoryRecommendations(category: string): void {
    this.subscriptions.push(
      this.recommendationService.getCategoryRecommendations(category, 20).subscribe({
        next: (recommendations) => {
          this.categoryRecommendations = recommendations;
          this.selectedTabIndex = 2; // Switch to categories tab
        },
        error: (error) => {
          console.error('Error loading category recommendations:', error);
          this.snackBar.open('Failed to load category recommendations', 'Close', { duration: 3000 });
        }
      })
    );
  }

  onVideoClick(video: any, recommendationId?: number): void {
    // Track interaction
    if (recommendationId) {
      this.trackInteraction({
        videoId: video.id,
        type: 'View',
        score: 1.0
      });
    }

    // Navigate to video
    this.router.navigate(['/video', video.id]);
  }

  onLikeClick(video: any, event: Event): void {
    event.stopPropagation();
    
    this.trackInteraction({
      videoId: video.id,
      type: 'Like',
      score: 1.0
    });

    // Toggle like state
    video.isLiked = !video.isLiked;
    if (video.isLiked) {
      video.isDisliked = false;
    }
  }

  onDislikeClick(video: any, event: Event): void {
    event.stopPropagation();
    
    this.trackInteraction({
      videoId: video.id,
      type: 'Dislike',
      score: -0.5
    });

    // Toggle dislike state
    video.isDisliked = !video.isDisliked;
    if (video.isDisliked) {
      video.isLiked = false;
    }
  }

  onShareClick(video: any, event: Event): void {
    event.stopPropagation();
    
    this.trackInteraction({
      videoId: video.id,
      type: 'Share',
      score: 0.8
    });

    // Implement share functionality
    if (navigator.share) {
      navigator.share({
        title: video.title,
        url: `${window.location.origin}/video/${video.id}`
      });
    } else {
      // Fallback to clipboard
      navigator.clipboard.writeText(`${window.location.origin}/video/${video.id}`);
      this.snackBar.open('Link copied to clipboard', 'Close', { duration: 2000 });
    }
  }

  private trackInteraction(interaction: any): void {
    this.subscriptions.push(
      this.recommendationService.trackInteraction(interaction).subscribe({
        next: () => {
          // Interaction tracked successfully
        },
        error: (error) => {
          console.error('Error tracking interaction:', error);
        }
      })
    );
  }

  formatDuration(duration: string): string {
    const seconds = parseInt(duration);
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));
    
    if (diffInDays === 0) {
      return 'Today';
    } else if (diffInDays === 1) {
      return 'Yesterday';
    } else if (diffInDays < 7) {
      return `${diffInDays} days ago`;
    } else {
      return date.toLocaleDateString();
    }
  }

  getRecommendationTypeLabel(type: string): string {
    switch (type) {
      case 'Personalized': return 'For You';
      case 'Collaborative': return 'Similar Users';
      case 'Trending': return 'Trending';
      case 'Category': return 'Category';
      case 'Channel': return 'Channel';
      default: return type;
    }
  }

  getRecommendationBadgeClass(type: string): string {
    return type.toLowerCase();
  }

  getCategoryIcon(category: string): string {
    const iconMap: { [key: string]: string } = {
      'Gaming': 'sports_esports',
      'Music': 'music_note',
      'Sports': 'sports',
      'Education': 'school',
      'Entertainment': 'movie',
      'Technology': 'computer',
      'Lifestyle': 'favorite',
      'News': 'newspaper'
    };
    return iconMap[category] || 'category';
  }
}
