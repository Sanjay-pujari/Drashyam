import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { LiveStream, LiveStreamCreate, LiveStreamUpdate } from '../models/live-stream.model';
import { PagedResult } from './video.service';

@Injectable({
  providedIn: 'root'
})
export class LiveStreamService {
  private apiUrl = `${environment.apiUrl}/api/livestreams`;

  constructor(private http: HttpClient) {}

  getActiveLiveStreams(filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<LiveStream>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<LiveStream>>(`${this.apiUrl}/active`, { params });
  }

  getLiveStreamById(id: number): Observable<LiveStream> {
    return this.http.get<LiveStream>(`${this.apiUrl}/${id}`);
  }

  getLiveStreamByStreamKey(streamKey: string): Observable<LiveStream> {
    return this.http.get<LiveStream>(`${this.apiUrl}/stream-key/${streamKey}`);
  }

  createLiveStream(streamData: LiveStreamCreate): Observable<LiveStream> {
    return this.http.post<LiveStream>(this.apiUrl, streamData);
  }

  updateLiveStream(id: number, streamData: LiveStreamUpdate): Observable<LiveStream> {
    return this.http.put<LiveStream>(`${this.apiUrl}/${id}`, streamData);
  }

  deleteLiveStream(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getUserLiveStreams(userId: string, filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<LiveStream>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<LiveStream>>(`${this.apiUrl}/user/${userId}`, { params });
  }

  startLiveStream(id: number): Observable<LiveStream> {
    return this.http.post<LiveStream>(`${this.apiUrl}/${id}/start`, {});
  }

  stopLiveStream(id: number): Observable<LiveStream> {
    return this.http.post<LiveStream>(`${this.apiUrl}/${id}/stop`, {});
  }

  generateStreamKey(id: number): Observable<{ streamKey: string }> {
    return this.http.post<{ streamKey: string }>(`${this.apiUrl}/${id}/generate-key`, {});
  }

  validateStreamKey(streamKey: string): Observable<{ isValid: boolean }> {
    return this.http.post<{ isValid: boolean }>(`${this.apiUrl}/validate-key`, { streamKey });
  }

  getChannelLiveStreams(channelId: number, filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<LiveStream>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<LiveStream>>(`${this.apiUrl}/channel/${channelId}`, { params });
  }

  joinLiveStream(id: number): Observable<LiveStream> {
    return this.http.post<LiveStream>(`${this.apiUrl}/${id}/join`, {});
  }

  leaveLiveStream(id: number): Observable<LiveStream> {
    return this.http.post<LiveStream>(`${this.apiUrl}/${id}/leave`, {});
  }

  getLiveStreamViewerCount(id: number): Observable<{ viewerCount: number }> {
    return this.http.get<{ viewerCount: number }>(`${this.apiUrl}/${id}/viewers`);
  }
}
