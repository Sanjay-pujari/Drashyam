import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface WatchLaterItem {
  id: number;
  videoId: number;
  videoTitle: string;
  videoThumbnailUrl?: string;
  videoDuration: string;
  videoViewCount: number;
  channelName: string;
  addedAt: string;
}

export interface WatchLaterCreateDto {
  videoId: number;
}

@Injectable({
  providedIn: 'root'
})
export class WatchLaterService {
  private apiUrl = `${environment.apiUrl}/api/watchlater`;

  constructor(private http: HttpClient) {}

  getWatchLaterItems(page: number = 1, pageSize: number = 20): Observable<PagedResult<WatchLaterItem>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<WatchLaterItem>>(this.apiUrl, { params });
  }

  addToWatchLater(videoId: number): Observable<WatchLaterItem> {
    const createDto: WatchLaterCreateDto = { videoId };
    return this.http.post<WatchLaterItem>(this.apiUrl, createDto);
  }

  removeFromWatchLater(videoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${videoId}`);
  }

  isVideoInWatchLater(videoId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/check/${videoId}`);
  }

  clearWatchLater(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/clear`);
  }

  getWatchLaterCount(): Observable<number> {
    console.log('Calling watch later count API:', `${this.apiUrl}/count`);
    return this.http.get<number>(`${this.apiUrl}/count`);
  }
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
