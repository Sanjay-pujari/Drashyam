import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Channel, ChannelCreate, ChannelUpdate } from '../models/channel.model';
import { PagedResult } from './video.service';

@Injectable({
  providedIn: 'root'
})
export class ChannelService {
  private apiUrl = `${environment.apiUrl}/api/channel`;

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
    return this.http.get<Channel>(`${this.apiUrl}/by-url/${customUrl}`);
  }

  createChannel(channelData: ChannelCreate): Observable<Channel> {
    // Wrap the data in createDto as expected by backend
    const requestBody = { createDto: channelData };
    return this.http.post<Channel>(this.apiUrl, requestBody);
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

    if (userId === 'me') {
      return this.http.get<PagedResult<Channel>>(`${this.apiUrl}/me`, { params });
    }
    return this.http.get<PagedResult<Channel>>(`${this.apiUrl}/user/${userId}`, { params });
  }

  getMyChannels(filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Channel>> {
    return this.getUserChannels('me', filter);
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

  isSubscribed(channelId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/${channelId}/is-subscribed`);
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
    formData.append('bannerFile', bannerFile);
    
    return this.http.post<Channel>(`${this.apiUrl}/${channelId}/banner`, formData);
  }

  updateChannelProfilePicture(channelId: number, profilePictureFile: File): Observable<Channel> {
    const formData = new FormData();
    formData.append('profilePicture', profilePictureFile);
    
    return this.http.post<Channel>(`${this.apiUrl}/${channelId}/profile-picture`, formData);
  }

  // Additional methods for channel detail component
  getChannel(id: number): Observable<Channel> {
    const url = `${this.apiUrl}/${id}`;
    console.log('Channel service - API URL:', this.apiUrl);
    console.log('Channel service - Full URL:', url);
    return this.http.get<Channel>(url);
  }

  getSubscriptionStatus(channelId: number): Observable<{ isSubscribed: boolean }> {
    return this.http.get<boolean>(`${this.apiUrl}/${channelId}/is-subscribed`).pipe(
      map(isSubscribed => ({ isSubscribed }))
    );
  }

  subscribe(channelId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${channelId}/subscribe`, {});
  }

  unsubscribe(channelId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${channelId}/unsubscribe`, {});
  }
}
