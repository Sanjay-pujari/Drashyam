import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ChannelBranding {
  id: number;
  channelId: number;
  logoUrl?: string;
  bannerUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  customDomain?: string;
  customCss?: string;
  aboutText?: string;
  websiteUrl?: string;
  socialMediaLinks?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  channel?: any;
}

export interface ChannelBrandingCreate {
  logoUrl?: string;
  bannerUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  customDomain?: string;
  customCss?: string;
  aboutText?: string;
  websiteUrl?: string;
  socialMediaLinks?: string;
}

export interface ChannelBrandingUpdate {
  logoUrl?: string;
  bannerUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  customDomain?: string;
  customCss?: string;
  aboutText?: string;
  websiteUrl?: string;
  socialMediaLinks?: string;
  isActive?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ChannelBrandingService {
  private apiUrl = `${environment.apiUrl}/api/channelbranding`;

  constructor(private http: HttpClient) {}

  getChannelBranding(channelId: number): Observable<ChannelBranding> {
    return this.http.get<ChannelBranding>(`${this.apiUrl}/channel/${channelId}`);
  }

  createChannelBranding(channelId: number, branding: ChannelBrandingCreate): Observable<ChannelBranding> {
    return this.http.post<ChannelBranding>(`${this.apiUrl}/channel/${channelId}`, branding);
  }

  updateChannelBranding(channelId: number, branding: ChannelBrandingUpdate): Observable<ChannelBranding> {
    return this.http.put<ChannelBranding>(`${this.apiUrl}/channel/${channelId}`, branding);
  }

  deleteChannelBranding(channelId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/channel/${channelId}`);
  }

  activateChannelBranding(channelId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/channel/${channelId}/activate`, {});
  }

  deactivateChannelBranding(channelId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/channel/${channelId}/deactivate`, {});
  }
}
