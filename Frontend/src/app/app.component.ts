import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { SignalRService } from './services/signalr.service';
import { NotificationService } from './services/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Drashyam';
  isAuthenticated = false;
  private authSubscription?: Subscription;

  constructor(
    private authService: AuthService,
    private signalRService: SignalRService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit() {
    // Subscribe to authentication state
    this.authSubscription = this.authService.isAuthenticated$.subscribe(
      isAuth => {
        this.isAuthenticated = isAuth;
        if (isAuth) {
          this.initializeApp();
        }
      }
    );

    // Initialize SignalR connection
    this.signalRService.startConnection();
  }

  ngOnDestroy() {
    this.authSubscription?.unsubscribe();
    this.signalRService.stopConnection();
  }

  private initializeApp() {
    // Start SignalR hubs
    this.signalRService.startVideoHub();
    this.signalRService.startLiveStreamHub();
    
    // Subscribe to notifications
    this.signalRService.onNotificationReceived().subscribe(notification => {
      this.notificationService.showNotification(notification);
    });

    // Subscribe to live stream notifications
    this.signalRService.onLiveStreamStarted().subscribe(stream => {
      this.notificationService.showInfo(`Live stream started: ${stream.title}`);
    });
  }
}
