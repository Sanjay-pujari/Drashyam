import { Component, OnInit, OnDestroy, ViewChild, ElementRef, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HlsPlayerService, StreamQuality, StreamStatus, StreamMetrics } from '../../services/hls-player.service';
import { SignalRService } from '../../services/signalr.service';
import { LiveStreamService } from '../../services/live-stream.service';
import { Subject, takeUntil, interval } from 'rxjs';

@Component({
  selector: 'app-live-stream-player',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './live-stream-player.component.html',
  styleUrls: ['./live-stream-player.component.scss']
})
export class LiveStreamPlayerComponent implements OnInit, OnDestroy {
  @ViewChild('videoPlayer', { static: true }) videoPlayer!: ElementRef<HTMLVideoElement>;
  @Input() streamKey: string = '';
  @Input() autoPlay: boolean = true;
  @Input() showControls: boolean = true;
  @Input() showChat: boolean = true;
  @Input() showAnalytics: boolean = false;
  @Output() streamStarted = new EventEmitter<void>();
  @Output() streamStopped = new EventEmitter<void>();
  @Output() qualityChanged = new EventEmitter<StreamQuality>();
  @Output() viewerCountChanged = new EventEmitter<number>();

  // Player state
  isPlaying = false;
  isBuffering = false;
  isMuted = false;
  volume = 1;
  currentQuality: StreamQuality | null = null;
  availableQualities: StreamQuality[] = [];
  
  // Stream data
  streamStatus: StreamStatus | null = null;
  streamMetrics: StreamMetrics | null = null;
  viewerCount = 0;
  hlsUrl = '';
  
  // UI state
  showQualitySelector = false;
  showFullscreen = false;
  showSettings = false;
  isConnecting = false;
  connectionError = '';
  
  // Analytics
  analyticsVisible = false;
  metricsUpdateInterval: any;
  
  private destroy$ = new Subject<void>();

  constructor(
    private hlsPlayerService: HlsPlayerService,
    private signalRService: SignalRService,
    private liveStreamService: LiveStreamService
  ) {}

  ngOnInit(): void {
    this.initializePlayer();
    this.setupSignalRConnection();
    this.startMetricsUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stopMetricsUpdates();
    this.hlsPlayerService.destroy();
  }

  private async initializePlayer(): Promise<void> {
    try {
      this.isConnecting = true;
      this.connectionError = '';

      // Initialize HLS player
      this.hlsPlayerService.initializePlayer(this.videoPlayer.nativeElement);

      // Load stream
      if (this.streamKey) {
        await this.loadStream();
      }

      this.isConnecting = false;
    } catch (error) {
      console.error('Error initializing player:', error);
      this.connectionError = 'Failed to initialize video player';
      this.isConnecting = false;
    }
  }

  private async loadStream(): Promise<void> {
    try {
      // Get stream details from API
      const streamDetails = await this.liveStreamService.getLiveStreamById(parseInt(this.streamKey)).toPromise();
      
      if (streamDetails && streamDetails.hlsUrl) {
        this.hlsUrl = streamDetails.hlsUrl;
        
        // Load stream in player
        await this.hlsPlayerService.loadStream(this.streamKey, this.hlsUrl);
        
        // Get available qualities
        this.availableQualities = this.hlsPlayerService.getAvailableQualities();
        this.currentQuality = this.hlsPlayerService.getCurrentQuality();
        
        // Auto-play if enabled
        if (this.autoPlay) {
          this.play();
        }
        
        this.streamStarted.emit();
      } else {
        throw new Error('Stream not found or not available');
      }
    } catch (error) {
      console.error('Error loading stream:', error);
      this.connectionError = 'Failed to load stream';
    }
  }

  private setupSignalRConnection(): void {
    // Connect to SignalR for real-time updates
    this.signalRService.connect();
    
    // Listen for stream updates
    this.signalRService.onStreamUpdate()
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        if (update.streamKey === this.streamKey) {
          this.handleStreamUpdate(update);
        }
      });

    // Listen for viewer count updates
    this.signalRService.onViewerCountUpdate()
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        if (update.streamKey === this.streamKey) {
          this.viewerCount = update.viewerCount;
          this.viewerCountChanged.emit(this.viewerCount);
        }
      });
  }

  private handleStreamUpdate(update: any): void {
    switch (update.type) {
      case 'streamStarted':
        this.play();
        break;
      case 'streamStopped':
        this.pause();
        break;
      case 'qualityChanged':
        this.setQuality(update.quality);
        break;
    }
  }

  private startMetricsUpdates(): void {
    this.metricsUpdateInterval = setInterval(() => {
      this.updateMetrics();
    }, 5000); // Update every 5 seconds
  }

  private stopMetricsUpdates(): void {
    if (this.metricsUpdateInterval) {
      clearInterval(this.metricsUpdateInterval);
    }
  }

  private updateMetrics(): void {
    this.streamStatus = this.hlsPlayerService.getStreamStatus();
    this.streamMetrics = this.hlsPlayerService.getStreamMetrics();
    this.isBuffering = !this.hlsPlayerService.isStreamHealthy();
  }

  // Player controls
  play(): void {
    this.hlsPlayerService.play();
    this.isPlaying = true;
  }

  pause(): void {
    this.hlsPlayerService.pause();
    this.isPlaying = false;
  }

  stop(): void {
    this.hlsPlayerService.stop();
    this.isPlaying = false;
    this.streamStopped.emit();
  }

  togglePlayPause(): void {
    if (this.isPlaying) {
      this.pause();
    } else {
      this.play();
    }
  }

  toggleMute(): void {
    this.isMuted = !this.isMuted;
    this.hlsPlayerService.setMuted(this.isMuted);
  }

  setVolume(volume: number): void {
    this.volume = volume;
    this.hlsPlayerService.setVolume(volume);
    if (volume > 0) {
      this.isMuted = false;
    }
  }

  setQuality(quality: StreamQuality): void {
    this.hlsPlayerService.setQuality(quality);
    this.currentQuality = quality;
    this.showQualitySelector = false;
    this.qualityChanged.emit(quality);
  }

  toggleQualitySelector(): void {
    this.showQualitySelector = !this.showQualitySelector;
  }

  toggleFullscreen(): void {
    if (this.videoPlayer.nativeElement.requestFullscreen) {
      this.videoPlayer.nativeElement.requestFullscreen();
      this.showFullscreen = true;
    }
  }

  toggleSettings(): void {
    this.showSettings = !this.showSettings;
  }

  toggleAnalytics(): void {
    this.analyticsVisible = !this.analyticsVisible;
  }

  // Stream management
  async refreshStream(): Promise<void> {
    try {
      this.isConnecting = true;
      this.connectionError = '';
      
      this.stop();
      await this.loadStream();
      
      this.isConnecting = false;
    } catch (error) {
      console.error('Error refreshing stream:', error);
      this.connectionError = 'Failed to refresh stream';
      this.isConnecting = false;
    }
  }

  // Utility methods
  formatBitrate(bitrate: number): string {
    if (bitrate >= 1000000) {
      return `${(bitrate / 1000000).toFixed(1)} Mbps`;
    } else if (bitrate >= 1000) {
      return `${(bitrate / 1000).toFixed(1)} Kbps`;
    } else {
      return `${bitrate} bps`;
    }
  }

  formatViewerCount(count: number): string {
    if (count >= 1000000) {
      return `${(count / 1000000).toFixed(1)}M`;
    } else if (count >= 1000) {
      return `${(count / 1000).toFixed(1)}K`;
    } else {
      return count.toString();
    }
  }

  getQualityName(quality: StreamQuality): string {
    return `${quality.name} (${quality.width}x${quality.height})`;
  }

  isStreamHealthy(): boolean {
    return this.hlsPlayerService.isStreamHealthy();
  }

  getConnectionStatus(): string {
    if (this.isConnecting) return 'Connecting...';
    if (this.connectionError) return 'Error';
    if (this.isBuffering) return 'Buffering...';
    if (this.isPlaying) return 'Live';
    return 'Stopped';
  }

  getConnectionStatusClass(): string {
    if (this.isConnecting) return 'connecting';
    if (this.connectionError) return 'error';
    if (this.isBuffering) return 'buffering';
    if (this.isPlaying) return 'live';
    return 'stopped';
  }
}
