import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Notification {
  type: string;
  message: string;
  timestamp: Date;
  data?: any;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private hubConnection: HubConnection;
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/notificationHub`)
      .withAutomaticReconnect()
      .build();

    this.startConnection();
    this.setupEventHandlers();
  }

  private startConnection(): void {
    this.hubConnection.start()
      .then(() => {
        console.log('Connected to notification hub');
      })
      .catch(err => {
        console.error('Error connecting to notification hub:', err);
        setTimeout(() => this.startConnection(), 5000);
      });
  }

  private setupEventHandlers(): void {
    // Invite notifications
    this.hubConnection.on('InviteNotification', (notification: Notification) => {
      this.addNotification(notification);
    });

    // Referral notifications
    this.hubConnection.on('ReferralNotification', (notification: Notification) => {
      this.addNotification(notification);
    });

    // Connection state changes
    this.hubConnection.onclose(() => {
      console.log('Disconnected from notification hub');
    });

    this.hubConnection.onreconnecting(() => {
      console.log('Reconnecting to notification hub...');
    });

    this.hubConnection.onreconnected(() => {
      console.log('Reconnected to notification hub');
    });
  }

  private addNotification(notification: Notification): void {
    const currentNotifications = this.notificationsSubject.value;
    const newNotifications = [notification, ...currentNotifications].slice(0, 50); // Keep last 50 notifications
    this.notificationsSubject.next(newNotifications);
  }

  public getNotifications(): Observable<Notification[]> {
    return this.notifications$;
  }

  public clearNotifications(): void {
    this.notificationsSubject.next([]);
  }

  public markAsRead(notification: Notification): void {
    // Implementation for marking notifications as read
    // This could involve updating a local state or calling an API
  }

  public getUnreadCount(): number {
    // Implementation for getting unread notification count
    // This would depend on your notification model
    return this.notificationsSubject.value.length;
  }

  public isConnected(): boolean {
    return this.hubConnection.state === HubConnectionState.Connected;
  }

  public async disconnect(): Promise<void> {
    if (this.hubConnection.state === HubConnectionState.Connected) {
      await this.hubConnection.stop();
    }
  }
}
