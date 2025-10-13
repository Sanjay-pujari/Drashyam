import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Video } from '../models/video.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface VideoFilter {
  page?: number;
  pageSize?: number;
  search?: string;
  category?: string;
  sortBy?: string;
  sortOrder?: string;
}

@Injectable({
  providedIn: 'root'
})
export class VideoService {
  private apiUrl = `${environment.apiUrl}/api/videos`;

  constructor(private http: HttpClient) {}

  getVideos(filter: VideoFilter = {}): Observable<PagedResult<Video>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());
    if (filter.search) params = params.set('search', filter.search);
    if (filter.category) params = params.set('category', filter.category);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
    if (filter.sortOrder) params = params.set('sortOrder', filter.sortOrder);

    return this.http.get<PagedResult<Video>>(this.apiUrl, { params });
  }

  getVideoById(id: number): Observable<Video> {
    return this.http.get<Video>(`${this.apiUrl}/${id}`);
  }

  getVideoByShareToken(shareToken: string): Observable<Video> {
    return this.http.get<Video>(`${this.apiUrl}/share/${shareToken}`);
  }

  uploadVideo(videoData: FormData): Observable<Video> {
    return this.http.post<Video>(`${this.apiUrl}/upload`, videoData);
  }

  updateVideo(id: number, videoData: FormData): Observable<Video> {
    return this.http.put<Video>(`${this.apiUrl}/${id}`, videoData);
  }

  deleteVideo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  likeVideo(id: number, likeType: 'like' | 'dislike'): Observable<Video> {
    return this.http.post<Video>(`${this.apiUrl}/${id}/like`, { type: likeType });
  }

  recordVideoView(id: number, watchDuration: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/view`, { watchDuration });
  }

  searchVideos(query: string, filter: VideoFilter = {}): Observable<PagedResult<Video>> {
    let params = new HttpParams().set('query', query);
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Video>>(`${this.apiUrl}/search`, { params });
  }

  getTrendingVideos(filter: VideoFilter = {}): Observable<PagedResult<Video>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Video>>(`${this.apiUrl}/trending`, { params });
  }

  getRecommendedVideos(filter: VideoFilter = {}): Observable<PagedResult<Video>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Video>>(`${this.apiUrl}/recommended`, { params });
  }

  getUserVideos(userId: string, filter: VideoFilter = {}): Observable<PagedResult<Video>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Video>>(`${this.apiUrl}/user/${userId}`, { params });
  }

  getChannelVideos(channelId: number, filter: VideoFilter = {}): Observable<PagedResult<Video>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Video>>(`${this.apiUrl}/channel/${channelId}`, { params });
  }

  generateShareLink(id: number): Observable<{ shareToken: string }> {
    return this.http.post<{ shareToken: string }>(`${this.apiUrl}/${id}/share`, {});
  }
}