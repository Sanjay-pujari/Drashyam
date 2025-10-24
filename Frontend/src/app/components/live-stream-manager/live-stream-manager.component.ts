import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LiveStreamPlayerComponent } from '../live-stream-player/live-stream-player.component';
import { LiveStreamChatComponent } from '../live-stream-chat/live-stream-chat.component';
import { StreamAnalyticsDashboardComponent } from '../stream-analytics-dashboard/stream-analytics-dashboard.component';
import { LiveStreamService } from '../../services/live-stream.service';
import { SignalRService } from '../../services/signalr.service';
import { Subject, takeUntil } from 'rxjs';

export interface LiveStreamManagerConfig {
  streamId: string;
  userId: string;
  userName: string;
  isOwner: boolean;
  isModerator: boolean;
  showChat: boolean;
  showAnalytics: boolean;
  showPlayer: boolean;
  autoStart: boolean;
  enableRecording: boolean;
  enableMonetization: boolean;
}

@Component({
  selector: 'app-live-stream-manager',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    LiveStreamPlayerComponent,
    LiveStreamChatComponent,
    StreamAnalyticsDashboardComponent
  ],
  templateUrl: './live-stream-manager.component.html',
  styleUrls: ['./live-stream-manager.component.scss']
})
export class LiveStreamManagerComponent implements OnInit, OnDestroy {
  @ViewChild('player') player!: LiveStreamPlayerComponent;
  @ViewChild('chat') chat!: LiveStreamChatComponent;
  @ViewChild('analytics') analytics!: StreamAnalyticsDashboardComponent;

  // Configuration
  config: LiveStreamManagerConfig = {
    streamId: '',
    userId: '',
    userName: '',
    isOwner: false,
    isModerator: false,
    showChat: true,
    showAnalytics: false,
    showPlayer: true,
    autoStart: false,
    enableRecording: false,
    enableMonetization: false
  };

  // Stream state
  isStreaming = false;
  isRecording = false;
  streamStatus = 'offline';
  viewerCount = 0;
  streamKey = '';
  rtmpUrl = '';
  hlsUrl = '';
  
  // UI state
  layout: 'default' | 'chat-focus' | 'analytics-focus' | 'fullscreen' = 'default';
  showSettings = false;
  showStreamInfo = false;
  showModerationTools = false;
  
  // Stream info
  streamTitle = '';
  streamDescription = '';
  streamCategory = '';
  streamTags: string[] = [];
  
  // Recording
  recordingStartTime: Date | null = null;
  recordingDuration = 0;
  
  // Monetization
  totalRevenue = 0;
  superChats = 0;
  donations = 0;
  subscriptions = 0;
  
  private destroy$ = new Subject<void>();

  constructor(
    private liveStreamService: LiveStreamService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.initializeStream();
    this.setupSignalRConnection();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.disconnectFromStream();
  }

  private async initializeStream(): Promise<void> {
    try {
      // Load stream configuration
      await this.loadStreamConfig();
      
      // Initialize SignalR connection
      this.signalRService.connect();
      
      // Auto-start if configured
      if (this.config.autoStart) {
        await this.startStream();
      }
    } catch (error) {
      console.error('Error initializing stream:', error);
    }
  }

  private async loadStreamConfig(): Promise<void> {
    // In a real implementation, you would load this from your API
    // For now, we'll use default values
    this.config = {
      streamId: '1',
      userId: 'user123',
      userName: 'Streamer',
      isOwner: true,
      isModerator: true,
      showChat: true,
      showAnalytics: false,
      showPlayer: true,
      autoStart: false,
      enableRecording: true,
      enableMonetization: true
    };

    this.streamKey = this.generateStreamKey();
    this.rtmpUrl = 'rtmp://localhost:1935/live';
    this.hlsUrl = `https://localhost:5000/streams/${this.streamKey}/playlist.m3u8`;
  }

  private setupSignalRConnection(): void {
    // Listen for stream updates
    this.signalRService.onStreamUpdate()
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        this.handleStreamUpdate(update);
      });

    // Listen for viewer count updates
    this.signalRService.onViewerCountUpdate()
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        this.viewerCount = update.viewerCount;
      });

    // Listen for chat messages
    this.signalRService.onChatMessage()
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        // Handle chat message
      });

    // Listen for monetization events
    this.signalRService.onMonetizationEvent()
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        this.handleMonetizationEvent(event);
      });
  }

  private handleStreamUpdate(update: any): void {
    switch (update.type) {
      case 'streamStarted':
        this.isStreaming = true;
        this.streamStatus = 'live';
        break;
      case 'streamStopped':
        this.isStreaming = false;
        this.streamStatus = 'offline';
        break;
      case 'viewerCountUpdated':
        this.viewerCount = update.viewerCount;
        break;
    }
  }

  private handleMonetizationEvent(event: any): void {
    switch (event.type) {
      case 'superChat':
        this.superChats += event.amount;
        this.totalRevenue += event.amount;
        break;
      case 'donation':
        this.donations += event.amount;
        this.totalRevenue += event.amount;
        break;
      case 'subscription':
        this.subscriptions += event.amount;
        this.totalRevenue += event.amount;
        break;
    }
  }

  // Stream Management
  async startStream(): Promise<void> {
    try {
      if (this.isStreaming) return;

      // Start the stream
      await this.liveStreamService.startStream({
        streamKey: this.streamKey,
        title: this.streamTitle,
        description: this.streamDescription,
        category: this.streamCategory,
        tags: this.streamTags
      }).toPromise();

      this.isStreaming = true;
      this.streamStatus = 'live';

      // Start recording if enabled
      if (this.config.enableRecording) {
        await this.startRecording();
      }

      // Notify via SignalR
      this.signalRService.sendStreamUpdate(this.config.streamId, 'streamStarted', {
        streamKey: this.streamKey,
        title: this.streamTitle
      });
    } catch (error) {
      console.error('Error starting stream:', error);
    }
  }

  async stopStream(): Promise<void> {
    try {
      if (!this.isStreaming) return;

      // Stop recording if active
      if (this.isRecording) {
        await this.stopRecording();
      }

      // Stop the stream
      await this.liveStreamService.stopStream(this.config.streamId).toPromise();

      this.isStreaming = false;
      this.streamStatus = 'offline';

      // Notify via SignalR
      this.signalRService.sendStreamUpdate(this.config.streamId, 'streamStopped', {});
    } catch (error) {
      console.error('Error stopping stream:', error);
    }
  }

  async startRecording(): Promise<void> {
    try {
      if (this.isRecording) return;

      await this.liveStreamService.startRecording(this.config.streamId).toPromise();
      
      this.isRecording = true;
      this.recordingStartTime = new Date();
    } catch (error) {
      console.error('Error starting recording:', error);
    }
  }

  async stopRecording(): Promise<void> {
    try {
      if (!this.isRecording) return;

      await this.liveStreamService.stopRecording(this.config.streamId).toPromise();
      
      this.isRecording = false;
      this.recordingStartTime = null;
    } catch (error) {
      console.error('Error stopping recording:', error);
    }
  }

  // UI Controls
  toggleLayout(newLayout: 'default' | 'chat-focus' | 'analytics-focus' | 'fullscreen'): void {
    this.layout = newLayout;
  }

  toggleChat(): void {
    this.config.showChat = !this.config.showChat;
  }

  toggleAnalytics(): void {
    this.config.showAnalytics = !this.config.showAnalytics;
  }

  toggleSettings(): void {
    this.showSettings = !this.showSettings;
  }

  toggleStreamInfo(): void {
    this.showStreamInfo = !this.showStreamInfo;
  }

  toggleModerationTools(): void {
    this.showModerationTools = !this.showModerationTools;
  }

  // Stream Configuration
  updateStreamInfo(): void {
    // Update stream information
    this.liveStreamService.updateStreamInfo(this.config.streamId, {
      title: this.streamTitle,
      description: this.streamDescription,
      category: this.streamCategory,
      tags: this.streamTags
    }).subscribe();
  }

  // Utility methods
  private generateStreamKey(): string {
    return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
  }

  private disconnectFromStream(): void {
    if (this.isStreaming) {
      this.stopStream();
    }
    this.signalRService.disconnect();
  }

  formatDuration(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);
    
    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    } else {
      return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  getRecordingDuration(): number {
    if (!this.recordingStartTime) return 0;
    return Math.floor((Date.now() - this.recordingStartTime.getTime()) / 1000);
  }

  getStreamStatusColor(): string {
    switch (this.streamStatus) {
      case 'live': return '#4CAF50';
      case 'offline': return '#9E9E9E';
      case 'starting': return '#FF9800';
      case 'stopping': return '#FF9800';
      default: return '#9E9E9E';
    }
  }

  getLayoutClass(): string {
    return `layout-${this.layout}`;
  }

  // Event handlers
  onStreamStarted(): void {
    console.log('Stream started');
  }

  onStreamStopped(): void {
    console.log('Stream stopped');
  }

  onViewerCountChanged(count: number): void {
    this.viewerCount = count;
  }

  onQualityChanged(quality: any): void {
    console.log('Quality changed:', quality);
  }

  onMessageSent(message: any): void {
    console.log('Message sent:', message);
  }

  onReactionSent(reaction: any): void {
    console.log('Reaction sent:', reaction);
  }

  onUserMentioned(userName: string): void {
    console.log('User mentioned:', userName);
  }

  onAnalyticsUpdated(analytics: any): void {
    console.log('Analytics updated:', analytics);
  }

  toggleRecording(): void {
    if (this.isRecording) {
      this.stopRecording();
    } else {
      this.startRecording();
    }
  }
}
