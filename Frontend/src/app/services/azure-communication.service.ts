import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface StreamingEndpoint {
  streamKey: string;
  hlsUrl: string;
  rtmpUrl: string;
  webRtcUrl: string;
  status: StreamingStatus;
  createdAt: string;
  lastUpdatedAt?: string;
  currentViewers: number;
  peakViewers: number;
}

export interface StreamingAnalytics {
  streamKey: string;
  currentViewers: number;
  totalViewers: number;
  averageViewerDuration: string;
  peakViewers: number;
  bitrateKbps: number;
  qualityScore: number;
  ingestLatencyMs: number;
  egressLatencyMs: number;
  packetLossRate: number;
}

export interface StreamingHealth {
  streamKey: string;
  status: string;
  cpuUsagePercent: number;
  memoryUsagePercent: number;
  networkLatencyMs: number;
  packetLossRate: number;
  isHealthy: boolean;
  lastCheckedAt: string;
}

export enum StreamingStatus {
  Created = 'Created',
  Active = 'Active',
  Inactive = 'Inactive',
  Error = 'Error',
  Processing = 'Processing',
  Stopped = 'Stopped'
}

@Injectable({
  providedIn: 'root'
})
export class AzureCommunicationService {
  private apiUrl = `${environment.apiUrl}/api/streaming`;

  constructor(private http: HttpClient) {}

  // Streaming Endpoint Management
  createStreamingEndpoint(streamKey: string): Observable<StreamingEndpoint> {
    return this.http.post<StreamingEndpoint>(`${this.apiUrl}/create-endpoint?streamKey=${streamKey}`, {});
  }

  startStreamingEndpoint(streamKey: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/start-endpoint?streamKey=${streamKey}`, {});
  }

  stopStreamingEndpoint(streamKey: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/stop-endpoint?streamKey=${streamKey}`, {});
  }

  getStreamingEndpoint(streamKey: string): Observable<StreamingEndpoint> {
    return this.http.get<StreamingEndpoint>(`${this.apiUrl}/endpoint/${streamKey}`);
  }

  deleteStreamingEndpoint(streamKey: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete-endpoint?streamKey=${streamKey}`);
  }

  // Playback URLs
  getStreamPlaybackUrl(streamKey: string, protocol: 'hls' | 'rtmp' | 'webrtc'): Observable<{ streamKey: string; protocol: string; url: string }> {
    return this.http.get<{ streamKey: string; protocol: string; url: string }>(`${this.apiUrl}/playback-url?streamKey=${streamKey}&protocol=${protocol}`);
  }

  // Analytics
  getStreamAnalytics(streamKey: string): Observable<StreamingAnalytics> {
    return this.http.get<StreamingAnalytics>(`${this.apiUrl}/analytics/${streamKey}`);
  }

  getStreamHealth(streamKey: string): Observable<StreamingHealth> {
    return this.http.get<StreamingHealth>(`${this.apiUrl}/health/${streamKey}`);
  }

  // Helper methods for common operations
  getHlsUrl(streamKey: string): Observable<string> {
    return this.http.get<{ url: string }>(`${this.apiUrl}/playback-url?streamKey=${streamKey}&protocol=hls`).pipe(
      map(response => response.url)
    );
  }

  getRtmpUrl(streamKey: string): Observable<string> {
    return this.http.get<{ url: string }>(`${this.apiUrl}/playback-url?streamKey=${streamKey}&protocol=rtmp`).pipe(
      map(response => response.url)
    );
  }

  getWebRtcUrl(streamKey: string): Observable<string> {
    return this.http.get<{ url: string }>(`${this.apiUrl}/playback-url?streamKey=${streamKey}&protocol=webrtc`).pipe(
      map(response => response.url)
    );
  }
}

// Import map operator
import { map } from 'rxjs/operators';
