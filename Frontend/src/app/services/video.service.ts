import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Video {
  id: number;
  title: string;
  description?: string;
  videoUrl: string;
  thumbnailUrl?: string;
  userId: string;
  userName: string;
  userProfilePicture?: string;
  channelId?: number;
  channelName?: string;
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
  shareToken?: string;
  isLiked: boolean;
  isDisliked: boolean;
  isSubscribed: boolean;
}

export interface VideoUploadRequest {
  title: string;
  description?: string;
  visibility: string;
  channelId?: number;
  tags?: string;
  category?: string;
  videoFile: File;
  thumbnailFile?: File;
}

export interface VideoFilter {
  page: number;
  pageSize: number;
  search?: string;
  category?: string;
  type?: string;
  visibility?: string;
  sortBy?: string;
  sortOrder?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class VideoService {
  private readonly API_URL = environment.apiUrl;
  private currentVideoSubject = new BehaviorSubject<Video | null>(null);
  public currentVideo$ = this.currentVideoSubject.asObservable();

  constructor(private http: HttpClient) {}

  uploadVideo(uploadData: VideoUploadRequest): Observable<Video> {
    const formData = new FormData();
    formData.append('title', uploadData.title);
    formData.append('description', uploadData.description || '');
    formData.append('visibility', uploadData.visibility);
    formData.append('videoFile', uploadData.videoFile);
    
    if (uploadData.channelId) {
      formData.append('channelId', uploadData.channelId.toString());
    }
    if (uploadData.tags) {
      formData.append('tags', uploadData.tags);
    }
    if (uploadData.category) {
      formData.append('category', uploadData.category);
    }
    if (uploadData.thumbnailFile) {
      formData.append('thumbnailFile', uploadData.thumbnailFile);
    }

    return this.http.post<Video>(`${this.API_URL}/videos/upload`, formData);
  }

  getVideos(filter: VideoFilter): Observable<PagedResult<Video>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.search) {
      params = params.set('search', filter.search);
    }
    if (filter.category) {
      params = params.set('category', filter.category);
    }
    if (filter.type) {
      params = params.set('type', filter.type);
    }
    if (filter.visibility) {
      params = params.set('visibility', filter.visibility);
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortOrder) {
      params = params.set('sortOrder', filter.sortOrder);
    }

    return this.http.get<PagedResult<Video>>(`${this.API_URL}/videos`, { params });
  }

  getVideoById(id: number): Observable<Video> {
    return this.http.get<Video>(`${this.API_URL}/videos/${id}`);
  }

  getVideoByShareToken(shareToken: string): Observable<Video> {
    return this.http.get<Video>(`${this.API_URL}/videos/share/${shareToken}`);
  }

  getUserVideos(userId: string, filter: VideoFilter): Observable<PagedResult<Video>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.search) {
      params = params.set('search', filter.search);
    }
    if (filter.category) {
      params = params.set('category', filter.category);
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortOrder) {
      params = params.set('sortOrder', filter.sortOrder);
    }

    return this.http.get<PagedResult<Video>>(`${this.API_URL}/videos/user/${userId}`, { params });
  }

  getChannelVideos(channelId: number, filter: VideoFilter): Observable<PagedResult<Video>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.search) {
      params = params.set('search', filter.search);
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortOrder) {
      params = params.set('sortOrder', filter.sortOrder);
    }

    return this.http.get<PagedResult<Video>>(`${this.API_URL}/videos/channel/${channelId}`, { params });
  }

  updateVideo(id: number, updateData: Partial<VideoUploadRequest>): Observable<Video> {
    const formData = new FormData();
    
    if (updateData.title) {
      formData.append('title', updateData.title);
    }
    if (updateData.description !== undefined) {
      formData.append('description', updateData.description);
    }
    if (updateData.visibility) {
      formData.append('visibility', updateData.visibility);
    }
    if (updateData.tags) {
      formData.append('tags', updateData.tags);
    }
    if (updateData.category) {
      formData.append('category', updateData.category);
    }
    if (updateData.thumbnailFile) {
      formData.append('thumbnailFile', updateData.thumbnailFile);
    }

    return this.http.put<Video>(`${this.API_URL}/videos/${id}`, formData);
  }

  deleteVideo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/videos/${id}`);
  }

  likeVideo(id: number, type: 'like' | 'dislike'): Observable<Video> {
    return this.http.post<Video>(`${this.API_URL}/videos/${id}/like`, { type });
  }

  recordView(id: number, watchDuration: number): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/videos/${id}/view`, { watchDuration });
  }

  generateShareLink(id: number): Observable<{ shareToken: string }> {
    return this.http.post<{ shareToken: string }>(`${this.API_URL}/videos/${id}/share`, {});
  }

  searchVideos(query: string, filter: VideoFilter): Observable<PagedResult<Video>> {
    filter.search = query;
    return this.getVideos(filter);
  }

  getTrendingVideos(filter: VideoFilter): Observable<PagedResult<Video>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Video>>(`${this.API_URL}/videos/trending`, { params });
  }

  getRecommendedVideos(filter: VideoFilter): Observable<PagedResult<Video>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Video>>(`${this.API_URL}/videos/recommended`, { params });
  }

  setCurrentVideo(video: Video | null) {
    this.currentVideoSubject.next(video);
  }

  getCurrentVideo(): Video | null {
    return this.currentVideoSubject.value;
  }
}
