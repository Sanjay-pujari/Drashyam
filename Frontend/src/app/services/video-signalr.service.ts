import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VideoLikeUpdate {
  videoId: number;
  likeCount: number;
  dislikeCount: number;
  isLiked: boolean;
  isDisliked: boolean;
}

export interface CommentUpdate {
  videoId: number;
  comment: any;
  action: 'added' | 'updated' | 'deleted';
}

export interface CommentLikeUpdate {
  commentId: number;
  likeCount: number;
  dislikeCount: number;
  isLiked: boolean;
  isDisliked: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class VideoSignalRService {
  private hubConnection?: HubConnection;
  private isConnected = false;
  
  // Subjects for real-time updates
  private videoLikeUpdateSubject = new BehaviorSubject<VideoLikeUpdate | null>(null);
  private commentUpdateSubject = new BehaviorSubject<CommentUpdate | null>(null);
  private commentLikeUpdateSubject = new BehaviorSubject<CommentLikeUpdate | null>(null);
  
  // Observables for components to subscribe to
  public videoLikeUpdate$ = this.videoLikeUpdateSubject.asObservable();
  public commentUpdate$ = this.commentUpdateSubject.asObservable();
  public commentLikeUpdate$ = this.commentLikeUpdateSubject.asObservable();

  constructor() {}

  async startConnection(token?: string): Promise<void> {
    if (this.hubConnection) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace(/\/$/, '')}/videoHub`, {
        accessTokenFactory: () => token || localStorage.getItem('token') || ''
      })
      .withAutomaticReconnect()
      .build();

    // Set up event listeners
    this.setupEventListeners();

    try {
      await this.hubConnection.start();
      this.isConnected = true;
      console.log('Video SignalR connection established');
    } catch (error) {
      console.error('Error starting Video SignalR connection:', error);
      this.isConnected = false;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = undefined;
      this.isConnected = false;
      console.log('Video SignalR connection stopped');
    }
  }

  private setupEventListeners(): void {
    if (!this.hubConnection) return;

    // Listen for video like updates
    this.hubConnection.on('ReceiveLike', (videoId: number, likeCount: number) => {
      this.videoLikeUpdateSubject.next({
        videoId,
        likeCount,
        dislikeCount: 0, // Will be updated by the component
        isLiked: true,
        isDisliked: false
      });
    });

    // Listen for video dislike updates
    this.hubConnection.on('ReceiveDislike', (videoId: number, dislikeCount: number) => {
      this.videoLikeUpdateSubject.next({
        videoId,
        likeCount: 0, // Will be updated by the component
        dislikeCount,
        isLiked: false,
        isDisliked: true
      });
    });

    // Listen for new comments
    this.hubConnection.on('ReceiveComment', (videoId: number, comment: any) => {
      this.commentUpdateSubject.next({
        videoId,
        comment,
        action: 'added'
      });
    });

    // Listen for comment like updates
    this.hubConnection.on('ReceiveCommentLike', (commentId: number, likeCount: number, dislikeCount: number) => {
      this.commentLikeUpdateSubject.next({
        commentId,
        likeCount,
        dislikeCount,
        isLiked: true,
        isDisliked: false
      });
    });

    // Listen for comment dislike updates
    this.hubConnection.on('ReceiveCommentDislike', (commentId: number, likeCount: number, dislikeCount: number) => {
      this.commentLikeUpdateSubject.next({
        commentId,
        likeCount,
        dislikeCount,
        isLiked: false,
        isDisliked: true
      });
    });

    // Connection state handlers
    this.hubConnection.onreconnected(() => {
      this.isConnected = true;
      console.log('Video SignalR connection reconnected');
    });

    this.hubConnection.onreconnecting(() => {
      this.isConnected = false;
      console.log('Video SignalR connection reconnecting...');
    });

    this.hubConnection.onclose(() => {
      this.isConnected = false;
      console.log('Video SignalR connection closed');
    });
  }

  // Join a video group to receive updates for a specific video
  async joinVideoGroup(videoId: number): Promise<void> {
    if (this.hubConnection && this.isConnected) {
      try {
        await this.hubConnection.invoke('JoinVideoGroup', videoId);
        console.log(`Joined video group for video ${videoId}`);
      } catch (error) {
        console.error('Error joining video group:', error);
      }
    }
  }

  // Leave a video group
  async leaveVideoGroup(videoId: number): Promise<void> {
    if (this.hubConnection && this.isConnected) {
      try {
        await this.hubConnection.invoke('LeaveVideoGroup', videoId);
        console.log(`Left video group for video ${videoId}`);
      } catch (error) {
        console.error('Error leaving video group:', error);
      }
    }
  }

  // Send a comment (for real-time broadcasting)
  async sendComment(videoId: number, comment: string): Promise<void> {
    if (this.hubConnection && this.isConnected) {
      try {
        await this.hubConnection.invoke('SendComment', videoId, comment);
      } catch (error) {
        console.error('Error sending comment:', error);
      }
    }
  }

  // Send a like (for real-time broadcasting)
  async sendLike(videoId: number, likeCount: number): Promise<void> {
    if (this.hubConnection && this.isConnected) {
      try {
        await this.hubConnection.invoke('SendLike', videoId, likeCount);
      } catch (error) {
        console.error('Error sending like:', error);
      }
    }
  }

  // Get connection status
  getConnectionStatus(): boolean {
    return this.isConnected;
  }
}
