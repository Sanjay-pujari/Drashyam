import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { PagedResult } from './video.service';

export interface VideoNotification {
  id: number;
  videoId: number;
  videoTitle: string;
  videoThumbnailUrl: string;
  videoDuration: string;
  videoViewCount: number;
  channelId: number;
  channelName: string;
  channelProfilePictureUrl: string;
  createdAt: Date;
  isRead: boolean;
  readAt?: Date;
  notificationType: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/api/notification`;
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();
  private hubConnection?: signalR.HubConnection;

  constructor(private http: HttpClient) {}

  // SignalR real-time notifications
  startConnection(token?: string): void {
    if (this.hubConnection) return;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace(/\/$/, '')}/notificationHub`, {
        accessTokenFactory: () => token || localStorage.getItem('token') || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('NotificationReceived', () => {
      this.refreshUnreadCount();
    });

    this.hubConnection.onreconnected(() => this.refreshUnreadCount());

    this.hubConnection.start().catch(() => {});
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().catch(() => {});
      this.hubConnection = undefined;
    }
  }

  getNotifications(page: number = 1, pageSize: number = 20): Observable<PagedResult<VideoNotification>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<VideoNotification>>(this.apiUrl, { params });
  }

  getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/count`).pipe(
      tap(count => this.unreadCountSubject.next(count))
    );
  }

  markAsRead(notificationId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${notificationId}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => this.unreadCountSubject.next(0))
    );
  }

  deleteNotification(notificationId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${notificationId}`);
  }

  refreshUnreadCount(): void {
    this.getUnreadCount().subscribe();
  }
}