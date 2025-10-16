import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from './services/auth.service';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    standalone: false
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Drashyam';
  isAuthenticated = false;
  private authSubscription?: Subscription;

  constructor(
    private authService: AuthService
  ) {}

  ngOnInit() {
    // Subscribe to authentication state
    this.authSubscription = this.authService.isAuthenticated$.subscribe(
      isAuth => {
        this.isAuthenticated = isAuth;
      }
    );
  }

  ngOnDestroy() {
    this.authSubscription?.unsubscribe();
  }
}
