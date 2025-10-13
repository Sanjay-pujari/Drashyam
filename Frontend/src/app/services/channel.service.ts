import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Channel, ChannelCreate, ChannelUpdate } from '../models/channel.model';
import { PagedResult } from './video.service';

@Injectable({
  providedIn: 'root'
})
export class ChannelService {
  private apiUrl = `${environment.apiUrl}/api/channels`;

  constructor(private http: HttpClient) {}

  getChannels(filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Channel>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Channel>>(this.apiUrl, { params });
  }

  getChannelById(id: number): Observable<Channel> {
    return this.http.get<Channel>(`${this.apiUrl}/${id}`);
  }

  getChannelByCustomUrl(customUrl: string): Observable<Channel> {
    return this.http.get<Channel>(`${this.apiUrl}/custom/${customUrl}`);
  }

  createChannel(channelData: ChannelCreate): Observable<Channel> {
    return this.http.post<Channel>(this.apiUrl, channelData);
  }

  updateChannel(id: number, channelData: ChannelUpdate): Observable<Channel> {
    return this.http.put<Channel>(`${this.apiUrl}/${id}`, channelData);
  }

  deleteChannel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getUserChannels(userId: string, filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Channel>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Channel>>(`${this.apiUrl}/user/${userId}`, { params });
  }

  searchChannels(query: string, filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Channel>> {
    let params = new HttpParams().set('query', query);
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Channel>>(`${this.apiUrl}/search`, { params });
  }

  subscribeToChannel(channelId: number): Observable<Channel> {
    return this.http.post<Channel>(`${this.apiUrl}/${channelId}/subscribe`, {});
  }

  unsubscribeFromChannel(channelId: number): Observable<Channel> {
    return this.http.post<Channel>(`${this.apiUrl}/${channelId}/unsubscribe`, {});
  }

  isSubscribed(channelId: number): Observable<{ isSubscribed: boolean }> {
    return this.http.get<{ isSubscribed: boolean }>(`${this.apiUrl}/${channelId}/subscription-status`);
  }

  getSubscribedChannels(filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Channel>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Channel>>(`${this.apiUrl}/subscribed`, { params });
  }

  getChannelSubscribers(channelId: number, filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<any>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<any>>(`${this.apiUrl}/${channelId}/subscribers`, { params });
  }

  updateChannelBanner(channelId: number, bannerFile: File): Observable<Channel> {
    const formData = new FormData();
    formData.append('banner', bannerFile);
    
    return this.http.post<Channel>(`${this.apiUrl}/${channelId}/banner`, formData);
  }

  updateChannelProfilePicture(channelId: number, profilePictureFile: File): Observable<Channel> {
    const formData = new FormData();
    formData.append('profilePicture', profilePictureFile);
    
    return this.http.post<Channel>(`${this.apiUrl}/${channelId}/profile-picture`, formData);
  }
}
