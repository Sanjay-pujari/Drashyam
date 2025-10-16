import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/video.model';
import { Observable, Subscription } from 'rxjs';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss'],
    standalone: true,
    imports: [CommonModule, RouterLink, MatToolbarModule, MatButtonModule, MatIconModule, MatMenuModule, MatDividerModule]
})
export class HeaderComponent implements OnInit, OnDestroy {
  title = 'Drashyam';
  currentUser$: Observable<User | null>;
  isAuthenticated$: Observable<boolean>;
  private subscription?: Subscription;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
    this.isAuthenticated$ = this.authService.isAuthenticated$;
  }

  ngOnInit() {
    // Check if user is already authenticated on component init
    this.subscription = this.isAuthenticated$.subscribe(isAuth => {
      if (!isAuth && this.authService.isAuthenticated()) {
        // If token exists but user not in state, fetch current user
        this.authService.getCurrentUser().subscribe();
      }
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  logout() {
    this.authService.logout();
    this.router.navigateByUrl('/');
  }
}

