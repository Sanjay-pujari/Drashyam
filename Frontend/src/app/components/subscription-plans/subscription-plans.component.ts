import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { SubscriptionService, SubscriptionPlan, Subscription, SubscriptionStatus } from '../../services/subscription.service';

@Component({
  selector: 'app-subscription-plans',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="subscription-plans-container">
      <div class="header">
        <h1>Choose Your Plan</h1>
        <p>Select the perfect plan for your content creation needs</p>
      </div>

      @if (isLoading) {
        <div class="loading-container">
          <mat-spinner diameter="50"></mat-spinner>
          <p>Loading subscription plans...</p>
        </div>
      } @else {
        <div class="plans-grid">
          @for (plan of plans; track plan.id) {
            <mat-card class="plan-card" [class.featured]="plan.name === 'Premium'" [class.current]="isCurrentPlan(plan)">
              @if (plan.name === 'Premium') {
                <div class="featured-badge">Most Popular</div>
              }
              
              @if (isCurrentPlan(plan)) {
                <div class="current-badge">Current Plan</div>
              }

              <mat-card-header>
                <mat-card-title>{{ plan.name }}</mat-card-title>
                <mat-card-subtitle>{{ plan.description }}</mat-card-subtitle>
              </mat-card-header>

              <mat-card-content>
                <div class="price-section">
                  <div class="price">
                    <span class="currency">$</span>
                    <span class="amount">{{ plan.price }}</span>
                    <span class="period">/{{ getBillingCycleText(plan.billingCycle) }}</span>
                  </div>
                </div>

                <div class="features">
                  <div class="feature">
                    <mat-icon>storage</mat-icon>
                    <span>{{ plan.maxStorageGB }}GB Storage</span>
                  </div>
                  <div class="feature">
                    <mat-icon>video_library</mat-icon>
                    <span>{{ plan.maxVideosPerChannel }} Videos per Channel</span>
                  </div>
                  <div class="feature">
                    <mat-icon>account_balance</mat-icon>
                    <span>{{ plan.maxChannels }} Channel{{ plan.maxChannels > 1 ? 's' : '' }}</span>
                  </div>
                  <div class="feature" [class.included]="!plan.hasAds">
                    <mat-icon>{{ plan.hasAds ? 'block' : 'check' }}</mat-icon>
                    <span>{{ plan.hasAds ? 'Ads Included' : 'No Ads' }}</span>
                  </div>
                  <div class="feature" [class.included]="plan.hasAnalytics">
                    <mat-icon>{{ plan.hasAnalytics ? 'check' : 'block' }}</mat-icon>
                    <span>{{ plan.hasAnalytics ? 'Advanced Analytics' : 'Basic Analytics' }}</span>
                  </div>
                  <div class="feature" [class.included]="plan.hasMonetization">
                    <mat-icon>{{ plan.hasMonetization ? 'check' : 'block' }}</mat-icon>
                    <span>{{ plan.hasMonetization ? 'Monetization Tools' : 'No Monetization' }}</span>
                  </div>
                  <div class="feature" [class.included]="plan.hasLiveStreaming">
                    <mat-icon>{{ plan.hasLiveStreaming ? 'check' : 'block' }}</mat-icon>
                    <span>{{ plan.hasLiveStreaming ? 'Live Streaming' : 'No Live Streaming' }}</span>
                  </div>
                </div>
              </mat-card-content>

              <mat-card-actions>
                @if (isCurrentPlan(plan)) {
                  <button mat-raised-button color="primary" disabled>
                    <mat-icon>check</mat-icon>
                    Current Plan
                  </button>
                } @else {
                  <button 
                    mat-raised-button 
                    [color]="plan.name === 'Premium' ? 'accent' : 'primary'"
                    (click)="selectPlan(plan)"
                    [disabled]="isLoading">
                    @if (isLoading && selectedPlanId === plan.id) {
                      <mat-spinner diameter="20"></mat-spinner>
                    } @else {
                      <ng-container>
                        <mat-icon>payment</mat-icon>
                        Select Plan
                      </ng-container>
                    }
                  </button>
                }
              </mat-card-actions>
            </mat-card>
          }
        </div>

        @if (currentSubscription) {
          <div class="current-subscription-info">
            <mat-card>
              <mat-card-header>
                <mat-card-title>Current Subscription</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="subscription-details">
                  <div class="plan-info">
                    <h3>{{ currentSubscription.plan?.name }}</h3>
                    <p>Status: <span [class]="getStatusClass(currentSubscription.status)">{{ getStatusText(currentSubscription.status) }}</span></p>
                    <p>Next Billing: {{ formatDate(currentSubscription.endDate) }}</p>
                  </div>
                  <div class="subscription-actions">
                    <button mat-stroked-button (click)="manageSubscription()">
                      <mat-icon>settings</mat-icon>
                      Manage Subscription
                    </button>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .subscription-plans-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }

    .header {
      text-align: center;
      margin-bottom: 40px;
    }

    .header h1 {
      font-size: 2.5rem;
      margin-bottom: 10px;
      color: #333;
    }

    .header p {
      font-size: 1.1rem;
      color: #666;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 20px;
      padding: 40px;
    }

    .plans-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 30px;
      margin-bottom: 40px;
    }

    .plan-card {
      position: relative;
      transition: transform 0.3s ease, box-shadow 0.3s ease;
      border: 2px solid transparent;
    }

    .plan-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
    }

    .plan-card.featured {
      border-color: #ff4081;
      transform: scale(1.05);
    }

    .plan-card.current {
      border-color: #4caf50;
      background-color: #f8fff8;
    }

    .featured-badge {
      position: absolute;
      top: -10px;
      right: 20px;
      background: #ff4081;
      color: white;
      padding: 5px 15px;
      border-radius: 20px;
      font-size: 0.8rem;
      font-weight: bold;
      z-index: 1;
    }

    .current-badge {
      position: absolute;
      top: -10px;
      left: 20px;
      background: #4caf50;
      color: white;
      padding: 5px 15px;
      border-radius: 20px;
      font-size: 0.8rem;
      font-weight: bold;
      z-index: 1;
    }

    .price-section {
      text-align: center;
      margin: 20px 0;
    }

    .price {
      display: flex;
      align-items: baseline;
      justify-content: center;
      gap: 5px;
    }

    .currency {
      font-size: 1.5rem;
      color: #666;
    }

    .amount {
      font-size: 3rem;
      font-weight: bold;
      color: #333;
    }

    .period {
      font-size: 1rem;
      color: #666;
    }

    .features {
      margin: 20px 0;
    }

    .feature {
      display: flex;
      align-items: center;
      gap: 10px;
      margin: 10px 0;
      padding: 8px 0;
    }

    .feature mat-icon {
      color: #666;
    }

    .feature.included mat-icon {
      color: #4caf50;
    }

    .current-subscription-info {
      margin-top: 40px;
    }

    .subscription-details {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .plan-info h3 {
      margin: 0 0 10px 0;
      color: #333;
    }

    .plan-info p {
      margin: 5px 0;
      color: #666;
    }

    .status-active {
      color: #4caf50;
      font-weight: bold;
    }

    .status-expired {
      color: #f44336;
      font-weight: bold;
    }

    .status-cancelled {
      color: #ff9800;
      font-weight: bold;
    }

    .status-suspended {
      color: #9e9e9e;
      font-weight: bold;
    }

    @media (max-width: 768px) {
      .plans-grid {
        grid-template-columns: 1fr;
      }
      
      .subscription-details {
        flex-direction: column;
        gap: 20px;
        text-align: center;
      }
    }
  `]
})
export class SubscriptionPlansComponent implements OnInit {
  plans: SubscriptionPlan[] = [];
  currentSubscription: Subscription | null = null;
  isLoading = false;
  selectedPlanId: number | null = null;

  constructor(
    private subscriptionService: SubscriptionService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadPlans();
    this.loadCurrentSubscription();
  }

  loadPlans() {
    this.isLoading = true;
    this.subscriptionService.getSubscriptionPlans().subscribe({
      next: (response) => {
        this.plans = response.items || response;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading subscription plans:', error);
        this.snackBar.open('Failed to load subscription plans', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  loadCurrentSubscription() {
    this.subscriptionService.getUserSubscription().subscribe({
      next: (subscription) => {
        this.currentSubscription = subscription;
      },
      error: (error) => {
        // User might not have a subscription
        console.log('No current subscription found');
      }
    });
  }

  selectPlan(plan: SubscriptionPlan) {
    this.selectedPlanId = plan.id;
    this.isLoading = true;

    // In a real implementation, this would redirect to payment processing
    this.snackBar.open(`Redirecting to payment for ${plan.name} plan...`, 'Close', { duration: 3000 });
    
    // Simulate payment processing
    setTimeout(() => {
      this.isLoading = false;
      this.selectedPlanId = null;
      this.snackBar.open('Payment processing would be implemented here', 'Close', { duration: 3000 });
    }, 2000);
  }

  isCurrentPlan(plan: SubscriptionPlan): boolean {
    return this.currentSubscription?.plan?.id === plan.id;
  }

  getBillingCycleText(cycle: number): string {
    return cycle === 0 ? 'month' : 'year';
  }

  getStatusText(status: SubscriptionStatus): string {
    switch (status) {
      case SubscriptionStatus.Active:
        return 'Active';
      case SubscriptionStatus.Expired:
        return 'Expired';
      case SubscriptionStatus.Cancelled:
        return 'Cancelled';
      case SubscriptionStatus.Suspended:
        return 'Suspended';
      default:
        return 'Unknown';
    }
  }

  getStatusClass(status: SubscriptionStatus): string {
    switch (status) {
      case SubscriptionStatus.Active:
        return 'status-active';
      case SubscriptionStatus.Expired:
        return 'status-expired';
      case SubscriptionStatus.Cancelled:
        return 'status-cancelled';
      case SubscriptionStatus.Suspended:
        return 'status-suspended';
      default:
        return '';
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  manageSubscription() {
    this.router.navigate(['/subscription-management']);
  }
}
