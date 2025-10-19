import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface HistoryItem {
  id: number;
  videoId: number;
  videoTitle: string;
  videoThumbnailUrl: string;
  videoUrl: string;
  channelName: string;
  channelId: string;
  viewCount: number;
  duration: string;
  viewedAt: string;
  watchDuration: string;
  userAgent?: string;
  deviceType?: string;
}

export interface HistoryCreateDto {
  videoId: number;
  watchDurationSeconds: number;
  userAgent?: string;
  deviceType?: string;
}

export interface HistoryUpdateDto {
  watchDurationSeconds: number;
}

@Injectable({
  providedIn: 'root'
})
export class HistoryService {
  private apiUrl = `${environment.apiUrl}/api/history`;

  constructor(private http: HttpClient) {}

  getUserHistory(page: number = 1, pageSize: number = 20): Observable<HistoryItem[]> {
    return this.http.get<HistoryItem[]>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
  }

  addToHistory(historyDto: HistoryCreateDto): Observable<HistoryItem> {
    return this.http.post<HistoryItem>(this.apiUrl, historyDto);
  }

  updateHistory(historyId: number, historyDto: HistoryUpdateDto): Observable<HistoryItem> {
    return this.http.put<HistoryItem>(`${this.apiUrl}/${historyId}`, historyDto);
  }

  removeFromHistory(historyId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${historyId}`);
  }

  clearHistory(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/clear`);
  }

  getHistoryItem(historyId: number): Observable<HistoryItem> {
    return this.http.get<HistoryItem>(`${this.apiUrl}/${historyId}`);
  }

  isVideoInHistory(videoId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/check/${videoId}`);
  }

  getHistoryCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/count`);
  }
}

