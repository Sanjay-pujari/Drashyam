import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { NgxSpinnerModule } from 'ngx-spinner';
import { AuthService } from './services/auth.service';
import { Subscription } from 'rxjs';
import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    standalone: true,
    imports: [CommonModule, RouterOutlet, NgxSpinnerModule, HeaderComponent, SidebarComponent]
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
    this.authSubscription = this.authService.currentUser$.subscribe(user => {
      this.isAuthenticated = !!user || this.authService.isAuthenticated();
    });
  }

  ngOnDestroy() {
    this.authSubscription?.unsubscribe();
  }
}
