import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface StreamUpdate {
  streamId: number;
  viewerCount: number;
  timestamp: Date;
}

export interface ChatMessage {
  id: number;
  userId: string;
  userName: string;
  message: string;
  timestamp: Date;
  isModerator: boolean;
  isSubscriber: boolean;
}

export interface NotificationMessage {
  id: number;
  title: string;
  message: string;
  type: string;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private liveStreamHub?: HubConnection;
  private chatHub?: HubConnection;
  private notificationHub?: HubConnection;

  // Stream updates
  private streamUpdatesSubject = new BehaviorSubject<StreamUpdate | null>(null);
  public streamUpdates$ = this.streamUpdatesSubject.asObservable();

  // Chat messages
  private chatMessagesSubject = new BehaviorSubject<ChatMessage | null>(null);
  public chatMessages$ = this.chatMessagesSubject.asObservable();

  // Notifications
  private notificationsSubject = new BehaviorSubject<NotificationMessage | null>(null);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor() {}

  // Initialize all SignalR connections
  async initializeConnections(): Promise<void> {
    await Promise.all([
      this.initializeLiveStreamHub(),
      this.initializeChatHub(),
      this.initializeNotificationHub()
    ]);
  }

  // Live Stream Hub
  private async initializeLiveStreamHub(): Promise<void> {
    this.liveStreamHub = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/liveStreamHub`, {
        accessTokenFactory: () => this.getAccessToken()
      })
      .withAutomaticReconnect()
      .build();

    // Register event handlers
    this.liveStreamHub.on('StreamStarted', (data) => {
      console.log('Stream started:', data);
    });

    this.liveStreamHub.on('StreamEnded', (data) => {
      console.log('Stream ended:', data);
    });

    this.liveStreamHub.on('ViewerCountUpdated', (data) => {
      this.streamUpdatesSubject.next({
        streamId: data.streamId,
        viewerCount: data.viewerCount,
        timestamp: new Date(data.timestamp)
      });
    });

    this.liveStreamHub.on('StreamQualityChanged', (data) => {
      console.log('Stream quality changed:', data);
    });

    this.liveStreamHub.on('StreamMetricsUpdated', (data) => {
      console.log('Stream metrics updated:', data);
    });

    await this.liveStreamHub.start();
  }

  // Chat Hub
  private async initializeChatHub(): Promise<void> {
    this.chatHub = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/chatHub`, {
        accessTokenFactory: () => this.getAccessToken()
      })
      .withAutomaticReconnect()
      .build();

    // Register event handlers
    this.chatHub.on('MessageReceived', (message) => {
      this.chatMessagesSubject.next({
        id: message.id,
        userId: message.userId,
        userName: message.userName,
        message: message.content,
        timestamp: new Date(message.timestamp),
        isModerator: message.isModerator || false,
        isSubscriber: message.isSubscriber || false
      });
    });

    this.chatHub.on('UserJoinedChat', (data) => {
      console.log('User joined chat:', data);
    });

    this.chatHub.on('UserLeftChat', (data) => {
      console.log('User left chat:', data);
    });

    this.chatHub.on('ReactionReceived', (data) => {
      console.log('Reaction received:', data);
    });

    await this.chatHub.start();
  }

  // Notification Hub
  private async initializeNotificationHub(): Promise<void> {
    this.notificationHub = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/notificationHub`, {
        accessTokenFactory: () => this.getAccessToken()
      })
      .withAutomaticReconnect()
      .build();

    // Register event handlers
    this.notificationHub.on('ReceiveNotification', (notification) => {
      this.notificationsSubject.next({
        id: notification.id,
        title: notification.title,
        message: notification.message,
        type: notification.type,
        timestamp: new Date(notification.createdAt)
      });
    });

    this.notificationHub.on('ReceiveGlobalNotification', (notification) => {
      console.log('Global notification:', notification);
    });

    this.notificationHub.on('SubscribedToNotifications', (message) => {
      console.log('Subscribed to notifications:', message);
    });

    this.notificationHub.on('UnsubscribedFromNotifications', (message) => {
      console.log('Unsubscribed from notifications:', message);
    });

    await this.notificationHub.start();
  }

  // Live Stream Hub Methods
  async joinStream(streamId: number): Promise<void> {
    if (this.liveStreamHub?.state === HubConnectionState.Connected) {
      await this.liveStreamHub.invoke('JoinStream', streamId.toString());
    }
  }

  async leaveStream(streamId: number): Promise<void> {
    if (this.liveStreamHub?.state === HubConnectionState.Connected) {
      await this.liveStreamHub.invoke('LeaveStream', streamId.toString());
    }
  }

  async sendStreamUpdate(streamId: number, updateData: any): Promise<void> {
    if (this.liveStreamHub?.state === HubConnectionState.Connected) {
      await this.liveStreamHub.invoke('SendStreamUpdate', streamId.toString(), updateData);
    }
  }

  async updateViewerCount(streamId: number, viewerCount: number): Promise<void> {
    if (this.liveStreamHub?.state === HubConnectionState.Connected) {
      await this.liveStreamHub.invoke('UpdateViewerCount', streamId.toString(), viewerCount);
    }
  }

  // Chat Hub Methods
  async joinChat(liveStreamId: number): Promise<void> {
    if (this.chatHub?.state === HubConnectionState.Connected) {
      await this.chatHub.invoke('JoinChat', liveStreamId.toString());
    }
  }

  async leaveChat(liveStreamId: number): Promise<void> {
    if (this.chatHub?.state === HubConnectionState.Connected) {
      await this.chatHub.invoke('LeaveChat', liveStreamId.toString());
    }
  }

  async sendMessage(liveStreamId: number, user: string, message: string): Promise<void> {
    if (this.chatHub?.state === HubConnectionState.Connected) {
      await this.chatHub.invoke('SendMessage', liveStreamId.toString(), user, message);
    }
  }

  async sendReaction(liveStreamId: number, user: string, reactionType: string): Promise<void> {
    if (this.chatHub?.state === HubConnectionState.Connected) {
      await this.chatHub.invoke('SendReaction', liveStreamId.toString(), user, reactionType);
    }
  }

  // Notification Hub Methods
  async subscribeToNotifications(userId: string): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('SubscribeToNotifications', userId);
    }
  }

  async unsubscribeFromNotifications(userId: string): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('UnsubscribeFromNotifications', userId);
    }
  }

  async sendNotificationToUser(userId: string, message: string): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('SendNotificationToUser', userId, message);
    }
  }

  async sendGlobalNotification(message: string): Promise<void> {
    if (this.notificationHub?.state === HubConnectionState.Connected) {
      await this.notificationHub.invoke('SendGlobalNotification', message);
    }
  }

  // Connection Management
  async disconnectAll(): Promise<void> {
    const disconnectPromises = [];

    if (this.liveStreamHub?.state === HubConnectionState.Connected) {
      disconnectPromises.push(this.liveStreamHub.stop());
    }

    if (this.chatHub?.state === HubConnectionState.Connected) {
      disconnectPromises.push(this.chatHub.stop());
    }

    if (this.notificationHub?.state === HubConnectionState.Connected) {
      disconnectPromises.push(this.notificationHub.stop());
    }

    await Promise.all(disconnectPromises);
  }

  // Connection Status
  getConnectionStatus(): { liveStream: boolean; chat: boolean; notifications: boolean } {
    return {
      liveStream: this.liveStreamHub?.state === HubConnectionState.Connected,
      chat: this.chatHub?.state === HubConnectionState.Connected,
      notifications: this.notificationHub?.state === HubConnectionState.Connected
    };
  }

  // Helper Methods
  private getAccessToken(): string {
    // Get JWT token from localStorage or auth service
    return localStorage.getItem('access_token') || '';
  }

  // Cleanup
  ngOnDestroy(): void {
    this.disconnectAll();
  }
}
