import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { SubscriptionService, Subscription, SubscriptionStatus } from '../../services/subscription.service';
import { SubscriptionPlansComponent } from '../subscription-plans/subscription-plans.component';

@Component({
  selector: 'app-subscription-management',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './subscription-management.component.html',
  styleUrls: ['./subscription-management.component.scss']
})
export class SubscriptionManagementComponent implements OnInit {
  subscription: Subscription | null = null;
  isLoading = false;
  SubscriptionStatus = SubscriptionStatus;

  constructor(
    private subscriptionService: SubscriptionService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadSubscription();
  }

  loadSubscription() {
    this.isLoading = true;
    this.subscriptionService.getUserSubscription().subscribe({
      next: (subscription) => {
        this.subscription = subscription;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading subscription:', error);
        this.snackBar.open('Failed to load subscription details', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  changePlan() {
    this.router.navigate(['/subscription-plans']);
  }

  updatePaymentMethod() {
    this.snackBar.open('Payment method update would be implemented here', 'Close', { duration: 3000 });
  }

  cancelSubscription() {
    if (this.subscription) {
      this.subscriptionService.cancelSubscription(this.subscription.id).subscribe({
        next: () => {
          this.snackBar.open('Subscription cancelled successfully', 'Close', { duration: 3000 });
          this.loadSubscription();
        },
        error: (error) => {
          console.error('Error cancelling subscription:', error);
          this.snackBar.open('Failed to cancel subscription', 'Close', { duration: 3000 });
        }
      });
    }
  }

  reactivateSubscription() {
    if (this.subscription) {
      this.subscriptionService.renewSubscription(this.subscription.id).subscribe({
        next: (subscription) => {
          this.subscription = subscription;
          this.snackBar.open('Subscription reactivated successfully', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error reactivating subscription:', error);
          this.snackBar.open('Failed to reactivate subscription', 'Close', { duration: 3000 });
        }
      });
    }
  }

  browsePlans() {
    this.router.navigate(['/subscription-plans']);
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

  getBillingCycleText(cycle: number): string {
    return cycle === 0 ? 'month' : 'year';
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}
