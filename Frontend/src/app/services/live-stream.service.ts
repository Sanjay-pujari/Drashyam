import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, combineLatest } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { LiveStream } from '../models/live-stream.model';
import { AzureCommunicationService, StreamingEndpoint, StreamingAnalytics, StreamingHealth } from './azure-communication.service';


export interface StreamQuality {
  name: string;
  width: number;
  height: number;
  bitrate: number;
  framerate: number;
  codec: string;
  isDefault: boolean;
  isEnabled: boolean;
}

export interface StreamAnalytics {
  streamId: number;
  totalViewers: number;
  peakViewers: number;
  currentViewers: number;
  duration: string;
  averageViewerCount: number;
  totalChatMessages: number;
  totalReactions: number;
  engagementRate: number;
}

export interface StreamHealth {
  streamId: number;
  status: string;
  cpuUsage: number;
  memoryUsage: number;
  networkLatency: number;
  bitrate: number;
  framerate: number;
  droppedFrames: number;
  lastUpdate: Date;
  alerts: any[];
}

export interface RecordingInfo {
  streamId: number;
  isRecording: boolean;
  startTime?: Date;
  endTime?: Date;
  recordingUrl?: string;
  thumbnailUrl?: string;
  fileSize: number;
  duration: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class LiveStreamService {
  private apiUrl = `${environment.apiUrl}/api`;

  constructor(
    private http: HttpClient,
    private azureCommunicationService: AzureCommunicationService
  ) {}

  // Stream Management
  getStreams(page: number = 1, pageSize: number = 10): Observable<any> {
    return this.http.get(`${this.apiUrl}/livestream?page=${page}&pageSize=${pageSize}`);
  }

  getStream(streamId: number): Observable<LiveStream> {
    return this.http.get<LiveStream>(`${this.apiUrl}/livestream/${streamId}`);
  }

  createStream(stream: Partial<LiveStream>): Observable<LiveStream> {
    return this.http.post<LiveStream>(`${this.apiUrl}/livestream`, stream);
  }

  updateStream(streamId: number, stream: Partial<LiveStream>): Observable<LiveStream> {
    return this.http.put<LiveStream>(`${this.apiUrl}/livestream/${streamId}`, stream);
  }

  deleteStream(streamId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/livestream/${streamId}`);
  }

  // Stream Control
  startStream(streamId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/streaming/${streamId}/start`, {});
  }

  stopStream(streamId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/streaming/${streamId}/stop`, {});
  }

  pauseStream(streamId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/streaming/${streamId}/pause`, {});
  }

  resumeStream(streamId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/streaming/${streamId}/resume`, {});
  }

  // Stream Quality
  getStreamQuality(streamId: number): Observable<StreamQuality> {
    return this.http.get<StreamQuality>(`${this.apiUrl}/streaming/${streamId}/quality`);
  }

  updateStreamQuality(streamId: number, quality: Partial<StreamQuality>): Observable<StreamQuality> {
    return this.http.put<StreamQuality>(`${this.apiUrl}/streaming/${streamId}/quality`, quality);
  }

  getAvailableQualities(): Observable<StreamQuality[]> {
    return this.http.get<StreamQuality[]>(`${this.apiUrl}/streaming/qualities`);
  }

  // Recording
  startRecording(streamId: number): Observable<RecordingInfo> {
    return this.http.post<RecordingInfo>(`${this.apiUrl}/streaming/${streamId}/recording/start`, {});
  }

  stopRecording(streamId: number): Observable<RecordingInfo> {
    return this.http.post<RecordingInfo>(`${this.apiUrl}/streaming/${streamId}/recording/stop`, {});
  }

  getRecordingStatus(streamId: number): Observable<RecordingInfo> {
    return this.http.get<RecordingInfo>(`${this.apiUrl}/streaming/${streamId}/recording`);
  }

  // Analytics
  getStreamAnalytics(streamId: number): Observable<StreamAnalytics> {
    return this.http.get<StreamAnalytics>(`${this.apiUrl}/streamanalytics/stream/${streamId}/realtime`);
  }

  getStreamHealth(streamId: number): Observable<StreamHealth> {
    return this.http.get<StreamHealth>(`${this.apiUrl}/streamanalytics/stream/${streamId}/health`);
  }

  getViewerAnalytics(streamId: number, startTime: Date, endTime: Date): Observable<any[]> {
    const params = {
      startTime: startTime.toISOString(),
      endTime: endTime.toISOString()
    };
    return this.http.get<any[]>(`${this.apiUrl}/streamanalytics/stream/${streamId}/viewers`, { params });
  }

  getQualityAnalytics(streamId: number, startTime: Date, endTime: Date): Observable<any[]> {
    const params = {
      startTime: startTime.toISOString(),
      endTime: endTime.toISOString()
    };
    return this.http.get<any[]>(`${this.apiUrl}/streamanalytics/stream/${streamId}/quality`, { params });
  }

  // Stream Endpoints
  getStreamEndpoint(streamId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/streaming/${streamId}/endpoint`);
  }

  validateStreamKey(streamKey: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/streaming/validate-key/${streamKey}`);
  }

  getStreamConfiguration(streamId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/streaming/${streamId}/config`);
  }

  // User Streams
  getUserStreams(userId: string, page: number = 1, pageSize: number = 10): Observable<any> {
    return this.http.get(`${this.apiUrl}/livestream/user/${userId}?page=${page}&pageSize=${pageSize}`);
  }

  getLiveStreams(): Observable<LiveStream[]> {
    return this.http.get<LiveStream[]>(`${this.apiUrl}/livestream/live`);
  }

  getLiveStreamById(streamId: number): Observable<LiveStream> {
    return this.http.get<LiveStream>(`${this.apiUrl}/livestream/${streamId}`);
  }

  getActiveLiveStreams(params: { page: number; pageSize: number }): Observable<any> {
    return this.http.get(`${this.apiUrl}/livestream/active?page=${params.page}&pageSize=${params.pageSize}`);
  }

  getFeaturedStreams(): Observable<LiveStream[]> {
    return this.http.get<LiveStream[]>(`${this.apiUrl}/livestream/featured`);
  }

  getTrendingStreams(): Observable<LiveStream[]> {
    return this.http.get<LiveStream[]>(`${this.apiUrl}/livestream/trending`);
  }

  // Stream Search
  searchStreams(query: string, category?: string, page: number = 1, pageSize: number = 10): Observable<any> {
    let url = `${this.apiUrl}/livestream/search?query=${encodeURIComponent(query)}&page=${page}&pageSize=${pageSize}`;
    if (category) {
      url += `&category=${encodeURIComponent(category)}`;
    }
    return this.http.get(url);
  }

  // Stream Categories
  getStreamCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/livestream/categories`);
  }

  // Stream Statistics
  getStreamStatistics(streamId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/livestream/${streamId}/statistics`);
  }

  // Stream Reports
  generateStreamReport(streamId: number, startTime: Date, endTime: Date): Observable<any> {
    const params = {
      startTime: startTime.toISOString(),
      endTime: endTime.toISOString()
    };
    return this.http.get(`${this.apiUrl}/streamanalytics/stream/${streamId}/report`, { params });
  }

  // Stream Comparison
  compareStreams(streamIds: number[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/streamanalytics/compare`, streamIds);
  }

  // Dashboard
  getStreamDashboard(streamId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/streamanalytics/stream/${streamId}/dashboard`);
  }

  getUserDashboard(userId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/streamanalytics/user/${userId}/dashboard`);
  }

  getGlobalDashboard(): Observable<any> {
    return this.http.get(`${this.apiUrl}/streamanalytics/global/dashboard`);
  }

  // Azure Communication Services Integration
  createStreamingEndpoint(streamKey: string): Observable<StreamingEndpoint> {
    return this.azureCommunicationService.createStreamingEndpoint(streamKey);
  }

  startStreamingEndpoint(streamKey: string): Observable<any> {
    return this.azureCommunicationService.startStreamingEndpoint(streamKey);
  }

  stopStreamingEndpoint(streamKey: string): Observable<any> {
    return this.azureCommunicationService.stopStreamingEndpoint(streamKey);
  }

  getStreamingEndpoint(streamKey: string): Observable<StreamingEndpoint> {
    return this.azureCommunicationService.getStreamingEndpoint(streamKey);
  }

  deleteStreamingEndpoint(streamKey: string): Observable<any> {
    return this.azureCommunicationService.deleteStreamingEndpoint(streamKey);
  }

  getStreamPlaybackUrl(streamKey: string, protocol: 'hls' | 'rtmp' | 'webrtc'): Observable<{ streamKey: string; protocol: string; url: string }> {
    return this.azureCommunicationService.getStreamPlaybackUrl(streamKey, protocol);
  }

  getStreamAnalytics(streamKey: string): Observable<StreamingAnalytics> {
    return this.azureCommunicationService.getStreamAnalytics(streamKey);
  }

  getStreamHealth(streamKey: string): Observable<StreamingHealth> {
    return this.azureCommunicationService.getStreamHealth(streamKey);
  }

  // Enhanced stream management with Azure Communication Services
  createStreamWithEndpoint(stream: Partial<LiveStream>): Observable<{ stream: LiveStream; endpoint: StreamingEndpoint }> {
    return this.createStream(stream).pipe(
      switchMap(createdStream => 
        this.azureCommunicationService.createStreamingEndpoint(createdStream.streamKey).pipe(
          map(endpoint => ({ stream: createdStream, endpoint }))
        )
      )
    );
  }

  getStreamWithEndpoint(streamId: number): Observable<{ stream: LiveStream; endpoint?: StreamingEndpoint }> {
    return this.getStream(streamId).pipe(
      switchMap(stream => 
        this.azureCommunicationService.getStreamingEndpoint(stream.streamKey).pipe(
          map(endpoint => ({ stream, endpoint }))
        )
      )
    );
  }

  getStreamWithAnalytics(streamId: number): Observable<{ stream: LiveStream; analytics?: StreamingAnalytics; health?: StreamingHealth }> {
    return this.getStream(streamId).pipe(
      switchMap(stream => 
        combineLatest([
          this.azureCommunicationService.getStreamAnalytics(stream.streamKey),
          this.azureCommunicationService.getStreamHealth(stream.streamKey)
        ]).pipe(
          map(([analytics, health]) => ({ stream, analytics, health }))
        )
      )
    );
  }

  // Enhanced analytics with Azure Communication Services
  getEnhancedStreamAnalytics(streamId: number): Observable<{ 
    stream: LiveStream; 
    analytics: StreamingAnalytics; 
    health: StreamingHealth;
    traditionalAnalytics: StreamAnalytics;
  }> {
    return this.getStream(streamId).pipe(
      switchMap(stream => 
        combineLatest([
          this.azureCommunicationService.getStreamAnalytics(stream.streamKey),
          this.azureCommunicationService.getStreamHealth(stream.streamKey),
          this.getStreamAnalytics(streamId)
        ]).pipe(
          map(([analytics, health, traditionalAnalytics]) => ({ 
            stream, 
            analytics, 
            health, 
            traditionalAnalytics 
          }))
        )
      )
    );
  }

  // Stream URLs for different protocols
  getHlsUrl(streamKey: string): Observable<string> {
    return this.azureCommunicationService.getHlsUrl(streamKey);
  }

  getRtmpUrl(streamKey: string): Observable<string> {
    return this.azureCommunicationService.getRtmpUrl(streamKey);
  }

  getWebRtcUrl(streamKey: string): Observable<string> {
    return this.azureCommunicationService.getWebRtcUrl(streamKey);
  }
}