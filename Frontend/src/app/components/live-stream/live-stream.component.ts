import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { LiveStreamService, LiveStream, StreamAnalytics, StreamHealth } from '../../services/live-stream.service';
import { SignalRService, StreamUpdate, ChatMessage, NotificationMessage } from '../../services/signalr.service';
import { LiveChatService } from '../../services/live-chat.service';

@Component({
  selector: 'app-live-stream',
  templateUrl: './live-stream.component.html',
  styleUrls: ['./live-stream.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class LiveStreamComponent implements OnInit, OnDestroy {
  @Input() streamId?: number;
  @Output() streamEnded = new EventEmitter<void>();

  private destroy$ = new Subject<void>();

  // Stream Data
  stream: LiveStream | null = null;
  streamAnalytics: StreamAnalytics | null = null;
  streamHealth: StreamHealth | null = null;
  isStreamOwner = false;
  isWatching = false;

  // Stream Controls
  isStreaming = false;
  isRecording = false;
  canStartStream = false;
  canStopStream = false;
  canPauseStream = false;
  canResumeStream = false;

  // Stream Quality
  currentQuality: string = '1080p';
  availableQualities: any[] = [];
  isQualityChanging = false;

  // Viewer Count
  viewerCount = 0;
  peakViewerCount = 0;

  // Chat
  chatEnabled = true;
  chatMessages: ChatMessage[] = [];
  newMessage = '';
  isSendingMessage = false;

  // Notifications
  notifications: NotificationMessage[] = [];

  // Loading States
  isLoading = true;
  isError = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private liveStreamService: LiveStreamService,
    private signalRService: SignalRService,
    private liveChatService: LiveChatService
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.cleanup();
  }

  private async initializeComponent(): Promise<void> {
    try {
      // Get stream ID from route or input
      if (!this.streamId) {
        this.streamId = +this.route.snapshot.paramMap.get('id')!;
      }

      if (!this.streamId) {
        this.router.navigate(['/']);
        return;
      }

      // Initialize SignalR connections
      await this.signalRService.initializeConnections();

      // Load stream data
      await this.loadStreamData();

      // Setup real-time listeners
      this.setupRealTimeListeners();

      // Join stream and chat
      await this.joinStream();

      this.isLoading = false;
    } catch (error) {
      console.error('Error initializing live stream:', error);
      this.isError = true;
      this.errorMessage = 'Failed to load stream';
      this.isLoading = false;
    }
  }

  private async loadStreamData(): Promise<void> {
    try {
      // Load stream details
      this.stream = await this.liveStreamService.getStream(this.streamId!).toPromise() || null;
      
      if (!this.stream) {
        this.router.navigate(['/']);
        return;
      }

      // Load analytics
      this.streamAnalytics = await this.liveStreamService.getStreamAnalytics(this.streamId!).toPromise() || null;
      
      // Load health data
      this.streamHealth = await this.liveStreamService.getStreamHealth(this.streamId!).toPromise() || null;

      // Load available qualities
      this.availableQualities = await this.liveStreamService.getAvailableQualities().toPromise() || [];

      // Set initial state
      this.updateStreamState();
      this.updateViewerCount();
    } catch (error) {
      console.error('Error loading stream data:', error);
      throw error;
    }
  }

  private setupRealTimeListeners(): void {
    // Stream updates
    this.signalRService.streamUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        if (update && update.streamId === this.streamId) {
          this.viewerCount = update.viewerCount;
        }
      });

    // Chat messages
    this.signalRService.chatMessages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        if (message) {
          this.chatMessages.push(message);
          this.scrollToBottom();
        }
      });

    // Notifications
    this.signalRService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notification => {
        if (notification) {
          this.notifications.push(notification);
          this.showNotification(notification);
        }
      });
  }

  private async joinStream(): Promise<void> {
    try {
      // Join stream hub
      await this.signalRService.joinStream(this.streamId!);

      // Join chat if enabled
      if (this.chatEnabled) {
        await this.signalRService.joinChat(this.streamId!);
      }

      this.isWatching = true;
    } catch (error) {
      console.error('Error joining stream:', error);
    }
  }

  private async leaveStream(): Promise<void> {
    try {
      // Leave stream hub
      await this.signalRService.leaveStream(this.streamId!);

      // Leave chat
      await this.signalRService.leaveChat(this.streamId!);

      this.isWatching = false;
    } catch (error) {
      console.error('Error leaving stream:', error);
    }
  }

  private updateStreamState(): void {
    if (!this.stream) return;

    this.isStreaming = this.stream.status === 'Live';
    this.isRecording = this.stream.isRecording || false;
    
    // Update control states based on stream status
    this.canStartStream = this.stream.status === 'Scheduled';
    this.canStopStream = this.isStreaming;
    this.canPauseStream = this.isStreaming;
    this.canResumeStream = this.stream.status === 'Paused';
  }

  private updateViewerCount(): void {
    if (this.stream) {
      this.viewerCount = this.stream.viewerCount;
      this.peakViewerCount = this.stream.peakViewerCount;
    }
  }

  // Stream Controls
  async startStream(): Promise<void> {
    try {
      await this.liveStreamService.startStream(this.streamId!).toPromise();
      this.isStreaming = true;
      this.updateStreamState();
    } catch (error) {
      console.error('Error starting stream:', error);
    }
  }

  async stopStream(): Promise<void> {
    try {
      await this.liveStreamService.stopStream(this.streamId!).toPromise();
      this.isStreaming = false;
      this.updateStreamState();
      this.streamEnded.emit();
    } catch (error) {
      console.error('Error stopping stream:', error);
    }
  }

  async pauseStream(): Promise<void> {
    try {
      await this.liveStreamService.pauseStream(this.streamId!).toPromise();
      this.updateStreamState();
    } catch (error) {
      console.error('Error pausing stream:', error);
    }
  }

  async resumeStream(): Promise<void> {
    try {
      await this.liveStreamService.resumeStream(this.streamId!).toPromise();
      this.updateStreamState();
    } catch (error) {
      console.error('Error resuming stream:', error);
    }
  }

  // Recording Controls
  async startRecording(): Promise<void> {
    try {
      await this.liveStreamService.startRecording(this.streamId!).toPromise();
      this.isRecording = true;
    } catch (error) {
      console.error('Error starting recording:', error);
    }
  }

  async stopRecording(): Promise<void> {
    try {
      await this.liveStreamService.stopRecording(this.streamId!).toPromise();
      this.isRecording = false;
    } catch (error) {
      console.error('Error stopping recording:', error);
    }
  }

  // Quality Management
  async changeQuality(quality: string): Promise<void> {
    if (this.isQualityChanging) return;

    try {
      this.isQualityChanging = true;
      this.currentQuality = quality;
      
      // Update quality on server
      await this.liveStreamService.updateStreamQuality(this.streamId!, { name: quality }).toPromise();
      
      // Notify viewers of quality change
      await this.signalRService.sendStreamUpdate(this.streamId!, { quality: quality });
    } catch (error) {
      console.error('Error changing quality:', error);
    } finally {
      this.isQualityChanging = false;
    }
  }

  // Chat Functions
  async sendMessage(): Promise<void> {
    if (!this.newMessage.trim() || this.isSendingMessage) return;

    try {
      this.isSendingMessage = true;
      
      const message = {
        liveStreamId: this.streamId!,
        message: this.newMessage,
        messageType: 'Text'
      };

      await this.liveChatService.sendMessage(message).toPromise();
      this.newMessage = '';
    } catch (error) {
      console.error('Error sending message:', error);
    } finally {
      this.isSendingMessage = false;
    }
  }

  async sendReaction(reactionType: string): Promise<void> {
    try {
      await this.signalRService.sendReaction(this.streamId!, 'user', reactionType);
    } catch (error) {
      console.error('Error sending reaction:', error);
    }
  }

  // Utility Methods
  private scrollToBottom(): void {
    // Scroll chat to bottom
    setTimeout(() => {
      const chatContainer = document.querySelector('.chat-messages');
      if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
      }
    }, 100);
  }

  private showNotification(notification: NotificationMessage): void {
    // Show notification toast
    console.log('Notification:', notification);
  }

  private cleanup(): void {
    if (this.isWatching) {
      this.leaveStream();
    }
  }

  // Public Methods
  toggleChat(): void {
    this.chatEnabled = !this.chatEnabled;
  }

  refreshStream(): void {
    this.loadStreamData();
  }

  shareStream(): void {
    if (navigator.share) {
      navigator.share({
        title: this.stream?.title,
        text: `Watch ${this.stream?.title} live on Drashyam`,
        url: window.location.href
      });
    } else {
      // Fallback to copying URL
      navigator.clipboard.writeText(window.location.href);
    }
  }

  getStreamUrl(): string {
    return this.stream?.streamUrl || '';
  }

  getHlsUrl(): string {
    return this.stream?.hlsUrl || '';
  }

  isStreamLive(): boolean {
    return this.stream?.status === 'Live';
  }

  getStreamDuration(): string {
    if (!this.stream?.startTime) return '00:00:00';
    
    const start = new Date(this.stream.startTime);
    const now = new Date();
    const duration = Math.floor((now.getTime() - start.getTime()) / 1000);
    
    const hours = Math.floor(duration / 3600);
    const minutes = Math.floor((duration % 3600) / 60);
    const seconds = duration % 60;
    
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  removeNotification(notification: NotificationMessage): void {
    this.notifications = this.notifications.filter(n => n !== notification);
  }
}
