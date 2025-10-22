import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../services/auth.service';
import { SidebarService } from '../../services/sidebar.service';
import { NotificationService, VideoNotification } from '../../services/notification.service';
import { CartService } from '../../services/cart.service';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';
import { User } from '../../models/user.model';
import { Observable, Subscription } from 'rxjs';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss'],
    standalone: true,
    imports: [CommonModule, RouterLink, MatToolbarModule, MatButtonModule, MatIconModule, MatMenuModule, MatDividerModule, MatProgressSpinnerModule, ThemeToggleComponent]
})
export class HeaderComponent implements OnInit, OnDestroy {
  title = 'Drashyam';
  currentUser$: Observable<User | null>;
  isAuthenticated$: Observable<boolean>;
  isSidebarCollapsed$: Observable<boolean>;
  unreadCount$: Observable<number>;
  notifications: VideoNotification[] = [];
  isLoadingNotifications = false;
  cartItemCount = 0;
  private subscription?: Subscription;

  constructor(
    private authService: AuthService,
    private sidebarService: SidebarService,
    private notificationService: NotificationService,
    private cartService: CartService,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
    this.isAuthenticated$ = this.authService.isAuthenticated$;
    this.isSidebarCollapsed$ = this.sidebarService.isCollapsed$;
    this.unreadCount$ = this.notificationService.unreadCount$;
  }

  ngOnInit() {
    // Check if user is already authenticated on component init
    this.subscription = this.isAuthenticated$.subscribe(isAuth => {
      if (!isAuth && this.authService.isAuthenticated()) {
        // If token exists but user not in state, fetch current user
        this.authService.getCurrentUser().subscribe();
      }
      if (isAuth) {
        // Load notification count when user is authenticated
        this.notificationService.refreshUnreadCount();
        // Start SignalR connection for real-time notifications
        this.notificationService.startConnection();
      }
    });

    // Subscribe to cart changes
    this.cartService.cart$.subscribe(cart => {
      this.cartItemCount = cart.totalItems;
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  toggleSidebar(): void {
    this.sidebarService.toggle();
  }

  logout() {
    this.authService.logout();
    this.notificationService.stopConnection();
    this.router.navigateByUrl('/');
  }

  loadNotifications() {
    this.isLoadingNotifications = true;
    this.notificationService.getNotifications(1, 10).subscribe({
      next: (result) => {
        this.notifications = result.items || [];
        this.isLoadingNotifications = false;
      },
      error: (error) => {
        this.isLoadingNotifications = false;
      }
    });
  }

  onNotificationClick(notification: VideoNotification) {
    // Mark as read if not already read
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe({
        next: () => {
          notification.isRead = true;
          this.notificationService.refreshUnreadCount();
        },
        error: (error) => {
        }
      });
    }

    // Navigate to video
    this.router.navigate(['/videos', notification.videoId]);
  }

  markAllAsRead() {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.notificationService.refreshUnreadCount();
      },
      error: (error) => {
      }
    });
  }

  deleteNotification(notificationId: number) {
    this.notificationService.deleteNotification(notificationId).subscribe({
      next: () => {
        this.notifications = this.notifications.filter(n => n.id !== notificationId);
        this.notificationService.refreshUnreadCount();
      },
      error: (error) => {
      }
    });
  }

  onImageError(event: any) {
    event.target.src = '/assets/default-video-thumbnail.svg';
  }
}

