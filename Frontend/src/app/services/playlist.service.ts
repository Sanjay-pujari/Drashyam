import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Playlist {
  id: number;
  name: string;
  description?: string;
  userId: string;
  userName: string;
  channelId?: number;
  channelName?: string;
  visibility: PlaylistVisibility;
  createdAt: string;
  updatedAt?: string;
  videoCount: number;
  thumbnailUrl?: string;
}

export interface PlaylistCreateDto {
  name: string;
  description?: string;
  channelId?: number;
  visibility: PlaylistVisibility;
}

export interface PlaylistUpdateDto {
  name: string;
  description?: string;
  visibility: PlaylistVisibility;
}

export interface PlaylistVideo {
  id: number;
  playlistId: number;
  videoId: number;
  order: number;
  addedAt: string;
  videoTitle: string;
  videoThumbnailUrl?: string;
  videoDuration: string;
  videoViewCount: number;
  channelName: string;
}

export interface PlaylistVideoCreateDto {
  videoId: number;
  order?: number;
}

export interface PlaylistVideoUpdateDto {
  videoId: number;
  order: number;
}

export enum PlaylistVisibility {
  Public = 0,
  Unlisted = 1,
  Private = 2
}

@Injectable({
  providedIn: 'root'
})
export class PlaylistService {
  private apiUrl = `${environment.apiUrl}/api/playlist`;

  constructor(private http: HttpClient) {}

  getPlaylists(page: number = 1, pageSize: number = 20): Observable<PagedResult<Playlist>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Playlist>>(this.apiUrl, { params });
  }

  createPlaylist(createDto: PlaylistCreateDto): Observable<Playlist> {
    return this.http.post<Playlist>(this.apiUrl, createDto);
  }

  getPlaylist(id: number): Observable<Playlist> {
    return this.http.get<Playlist>(`${this.apiUrl}/${id}`);
  }

  updatePlaylist(id: number, updateDto: PlaylistUpdateDto): Observable<Playlist> {
    return this.http.put<Playlist>(`${this.apiUrl}/${id}`, updateDto);
  }

  deletePlaylist(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getPlaylistVideos(playlistId: number, page: number = 1, pageSize: number = 20): Observable<PagedResult<PlaylistVideo>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<PlaylistVideo>>(`${this.apiUrl}/${playlistId}/videos`, { params });
  }

  addVideoToPlaylist(playlistId: number, createDto: PlaylistVideoCreateDto): Observable<PlaylistVideo> {
    return this.http.post<PlaylistVideo>(`${this.apiUrl}/${playlistId}/videos`, createDto);
  }

  removeVideoFromPlaylist(playlistId: number, videoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${playlistId}/videos/${videoId}`);
  }

  reorderPlaylistVideos(playlistId: number, updates: PlaylistVideoUpdateDto[]): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${playlistId}/videos/reorder`, updates);
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
