import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Recommendation {
  id: number;
  videoId: number;
  type: string;
  score: number;
  reason?: string;
  createdAt: string;
  video: Video;
}

export interface TrendingVideo {
  id: number;
  videoId: number;
  category?: string;
  country?: string;
  trendingScore: number;
  position: number;
  calculatedAt: string;
  video: Video;
}

export interface UserPreference {
  id: number;
  category?: string;
  tag?: string;
  weight: number;
  createdAt: string;
  updatedAt: string;
}

export interface RecommendationRequest {
  limit?: number;
  category?: string;
  type?: string;
  includeTrending?: boolean;
  includePersonalized?: boolean;
}

export interface Interaction {
  videoId: number;
  type: string;
  score?: number;
  watchDuration?: string;
}

export interface RecommendationFeedback {
  recommendationId: number;
  isClicked: boolean;
  isLiked: boolean;
  isDisliked: boolean;
  watchDuration?: string;
}

export interface Video {
  id: number;
  title: string;
  description?: string;
  videoUrl: string;
  thumbnailUrl?: string;
  userId: string;
  channelId?: number;
  status: string;
  type: string;
  visibility: string;
  createdAt: string;
  publishedAt?: string;
  viewCount: number;
  likeCount: number;
  dislikeCount: number;
  commentCount: number;
  duration: string;
  fileSize: number;
  tags?: string;
  category?: string;
  isMonetized: boolean;
  revenue?: number;
  shareToken?: string;
  user?: any;
  channel?: any;
  isLiked?: boolean;
  isDisliked?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class RecommendationService {
  private apiUrl = `${environment.apiUrl}/api/recommendation`;

  constructor(private http: HttpClient) {}

  // Get personalized recommendations
  getPersonalizedRecommendations(request: RecommendationRequest = {}): Observable<Recommendation[]> {
    const params: any = {};
    if (request.limit) params.limit = request.limit;
    if (request.category) params.category = request.category;
    if (request.type) params.type = request.type;
    if (request.includeTrending !== undefined) params.includeTrending = request.includeTrending;
    if (request.includePersonalized !== undefined) params.includePersonalized = request.includePersonalized;

    return this.http.get<Recommendation[]>(`${this.apiUrl}/personalized`, { params });
  }

  // Get trending videos
  getTrendingVideos(category?: string, country?: string, limit: number = 20): Observable<TrendingVideo[]> {
    const params: any = { limit };
    if (category) params.category = category;
    if (country) params.country = country;

    return this.http.get<TrendingVideo[]>(`${this.apiUrl}/trending`, { params });
  }

  // Get similar videos
  getSimilarVideos(videoId: number, limit: number = 10): Observable<Recommendation[]> {
    return this.http.get<Recommendation[]>(`${this.apiUrl}/similar/${videoId}`, {
      params: { limit: limit.toString() }
    });
  }

  // Get category recommendations
  getCategoryRecommendations(category: string, limit: number = 20): Observable<Recommendation[]> {
    return this.http.get<Recommendation[]>(`${this.apiUrl}/category/${category}`, {
      params: { limit: limit.toString() }
    });
  }

  // Get channel recommendations
  getChannelRecommendations(channelId: number, limit: number = 20): Observable<Recommendation[]> {
    return this.http.get<Recommendation[]>(`${this.apiUrl}/channel/${channelId}`, {
      params: { limit: limit.toString() }
    });
  }

  // Track user interaction
  trackInteraction(interaction: Interaction): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/interaction`, interaction);
  }

  // Update user preferences
  updatePreferences(preferences: UserPreference[]): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/preferences`, preferences);
  }

  // Get user preferences
  getPreferences(): Observable<UserPreference[]> {
    return this.http.get<UserPreference[]>(`${this.apiUrl}/preferences`);
  }

  // Record recommendation feedback
  recordFeedback(feedback: RecommendationFeedback): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/feedback`, feedback);
  }

  // Update recommendations
  updateRecommendations(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/update`, {});
  }

  // Calculate trending videos (admin only)
  calculateTrendingVideos(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/calculate-trending`, {});
  }
}
