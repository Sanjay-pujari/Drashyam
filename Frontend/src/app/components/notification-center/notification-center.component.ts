import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, VideoNotification } from '../../services/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-center.component.html',
  styleUrls: ['./notification-center.component.scss']
})
export class NotificationCenterComponent implements OnInit, OnDestroy {
  notifications: VideoNotification[] = [];
  isOpen = false;
  private subscription: Subscription = new Subscription();

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.subscription.add(
      this.notificationService.getNotifications().subscribe(result => {
        this.notifications = result.items;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  toggleCenter(): void {
    this.isOpen = !this.isOpen;
  }

  closeCenter(): void {
    this.isOpen = false;
  }

  clearAll(): void {
    // TODO: Implement clear notifications functionality
    console.log('Clear all notifications');
  }

  markAsRead(notification: VideoNotification): void {
    // TODO: Implement mark as read functionality
    console.log('Mark as read:', notification);
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'Success':
        return '✅';
      case 'Warning':
        return '⚠️';
      case 'Error':
        return '❌';
      case 'Invite':
        return '📧';
      case 'Referral':
        return '🔗';
      case 'Reward':
        return '💰';
      case 'Stats':
        return '📊';
      default:
        return 'ℹ️';
    }
  }

  getNotificationClass(type: string): string {
    switch (type) {
      case 'Success':
        return 'notification-success';
      case 'Warning':
        return 'notification-warning';
      case 'Error':
        return 'notification-error';
      case 'Invite':
        return 'notification-invite';
      case 'Referral':
        return 'notification-referral';
      case 'Reward':
        return 'notification-reward';
      case 'Stats':
        return 'notification-stats';
      default:
        return 'notification-info';
    }
  }

  formatTimestamp(timestamp: Date): string {
    const now = new Date();
    const diff = now.getTime() - new Date(timestamp).getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;
    return new Date(timestamp).toLocaleDateString();
  }

  trackByTimestamp(index: number, notification: VideoNotification): any {
    return notification.createdAt;
  }
}
