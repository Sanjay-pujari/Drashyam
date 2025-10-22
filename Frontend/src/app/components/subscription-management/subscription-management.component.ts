import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { QuotaService, QuotaStatus, SubscriptionBenefits } from '../../services/quota.service';

@Component({
  selector: 'app-subscription-management',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatChipsModule
  ],
  template: `
    <div class="subscription-management">
      <div class="header">
        <h1>Subscription Management</h1>
        <p>Manage your subscription and view usage</p>
      </div>

      <div class="content" *ngIf="quotaStatus && subscriptionBenefits">
        <!-- Current Plan -->
        <mat-card class="current-plan-card">
          <div class="plan-header">
            <div class="plan-info">
              <h2>{{ quotaStatus.subscriptionType }} Plan</h2>
              <p>Current subscription tier</p>
            </div>
            <div class="plan-actions">
              <button mat-raised-button color="primary" (click)="upgradePlan()" 
                      *ngIf="subscriptionBenefits.nextUpgradePlan">
                <mat-icon>upgrade</mat-icon>
                Upgrade Plan
              </button>
            </div>
          </div>
        </mat-card>

        <!-- Usage Overview -->
        <div class="usage-grid">
          <!-- Storage Usage -->
          <mat-card class="usage-card">
            <div class="usage-header">
              <mat-icon>storage</mat-icon>
              <h3>Storage</h3>
            </div>
            <div class="usage-content">
              <div class="usage-stats">
                <span class="used">{{ formatBytes(quotaStatus.storageUsed) }}</span>
                <span class="separator">/</span>
                <span class="limit">{{ formatBytes(quotaStatus.storageLimit) }}</span>
              </div>
              <mat-progress-bar 
                [value]="quotaStatus.storageUsagePercentage"
                [color]="getUsageColor(quotaStatus.storageUsagePercentage)">
              </mat-progress-bar>
              <div class="usage-percentage">
                {{ quotaStatus.storageUsagePercentage | number:'1.1-1' }}% used
              </div>
            </div>
          </mat-card>

          <!-- Video Usage -->
          <mat-card class="usage-card">
            <div class="usage-header">
              <mat-icon>video_library</mat-icon>
              <h3>Videos</h3>
            </div>
            <div class="usage-content">
              <div class="usage-stats">
                <span class="used">{{ quotaStatus.videosUploaded }}</span>
                <span class="separator">/</span>
                <span class="limit">{{ quotaStatus.videoLimit }}</span>
              </div>
              <mat-progress-bar 
                [value]="quotaStatus.videoUsagePercentage"
                [color]="getUsageColor(quotaStatus.videoUsagePercentage)">
              </mat-progress-bar>
              <div class="usage-percentage">
                {{ quotaStatus.videoUsagePercentage | number:'1.1-1' }}% used
              </div>
            </div>
          </mat-card>

          <!-- Channel Usage -->
          <mat-card class="usage-card">
            <div class="usage-header">
              <mat-icon>account_circle</mat-icon>
              <h3>Channels</h3>
            </div>
            <div class="usage-content">
              <div class="usage-stats">
                <span class="used">{{ quotaStatus.channelsCreated }}</span>
                <span class="separator">/</span>
                <span class="limit">{{ quotaStatus.channelLimit }}</span>
              </div>
              <mat-progress-bar 
                [value]="quotaStatus.channelUsagePercentage"
                [color]="getUsageColor(quotaStatus.channelUsagePercentage)">
              </mat-progress-bar>
              <div class="usage-percentage">
                {{ quotaStatus.channelUsagePercentage | number:'1.1-1' }}% used
              </div>
            </div>
          </mat-card>
        </div>

        <!-- Plan Benefits -->
        <mat-card class="benefits-card">
          <h3>Current Plan Benefits</h3>
          <div class="benefits-list">
            <div class="benefit-item" *ngFor="let benefit of subscriptionBenefits.benefits">
              <mat-icon>check_circle</mat-icon>
              <span>{{ benefit }}</span>
            </div>
          </div>
        </mat-card>

        <!-- Upgrade Benefits -->
        <mat-card class="upgrade-card" *ngIf="subscriptionBenefits.nextUpgradePlan">
          <h3>Upgrade to {{ subscriptionBenefits.nextUpgradePlan.name }}</h3>
          <div class="benefits-list">
            <div class="benefit-item" *ngFor="let benefit of subscriptionBenefits.upgradeBenefits">
              <mat-icon>star</mat-icon>
              <span>{{ benefit }}</span>
            </div>
          </div>
          <div class="upgrade-actions">
            <button mat-raised-button color="primary" (click)="upgradePlan()">
              <mat-icon>upgrade</mat-icon>
              Upgrade Now
            </button>
          </div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .subscription-management {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .header {
      margin-bottom: 30px;
    }

    .header h1 {
      margin: 0 0 8px 0;
      color: #333;
    }

    .header p {
      margin: 0;
      color: #666;
    }

    .current-plan-card {
      margin-bottom: 30px;
    }

    .plan-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .plan-info h2 {
      margin: 0 0 4px 0;
      color: #333;
    }

    .plan-info p {
      margin: 0;
      color: #666;
    }

    .usage-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
      margin-bottom: 30px;
    }

    .usage-card {
      padding: 20px;
    }

    .usage-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 16px;
    }

    .usage-header mat-icon {
      color: #666;
    }

    .usage-header h3 {
      margin: 0;
      color: #333;
    }

    .usage-stats {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 8px;
    }

    .usage-stats .used {
      font-weight: 600;
      color: #333;
    }

    .usage-stats .separator {
      color: #666;
    }

    .usage-stats .limit {
      color: #666;
    }

    .usage-percentage {
      text-align: right;
      font-size: 0.9rem;
      color: #666;
      margin-top: 4px;
    }

    .benefits-card, .upgrade-card {
      margin-bottom: 20px;
    }

    .benefits-card h3, .upgrade-card h3 {
      margin: 0 0 16px 0;
      color: #333;
    }

    .benefits-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .benefit-item {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .benefit-item mat-icon {
      color: #4caf50;
      font-size: 1.2rem;
    }

    .upgrade-actions {
      margin-top: 20px;
      text-align: center;
    }

    @media (max-width: 768px) {
      .usage-grid {
        grid-template-columns: 1fr;
      }
      
      .plan-header {
        flex-direction: column;
        gap: 16px;
        text-align: center;
      }
    }
  `]
})
export class SubscriptionManagementComponent implements OnInit, OnDestroy {
  quotaStatus: QuotaStatus | null = null;
  subscriptionBenefits: SubscriptionBenefits | null = null;
  private subscriptions: Subscription[] = [];

  constructor(
    private quotaService: QuotaService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadSubscriptionData();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadSubscriptionData() {
    // Load quota status
    const quotaSub = this.quotaService.getQuotaStatus().subscribe({
      next: (status) => {
        this.quotaStatus = status;
      },
      error: (error) => {
        console.error('Error loading quota status:', error);
        this.snackBar.open('Error loading quota status', 'Close', { duration: 3000 });
      }
    });

    // Load subscription benefits
    const benefitsSub = this.quotaService.getSubscriptionBenefits().subscribe({
      next: (benefits) => {
        this.subscriptionBenefits = benefits;
      },
      error: (error) => {
        console.error('Error loading subscription benefits:', error);
        this.snackBar.open('Error loading subscription benefits', 'Close', { duration: 3000 });
      }
    });

    this.subscriptions.push(quotaSub, benefitsSub);
  }

  formatBytes(bytes: number): string {
    return this.quotaService.formatBytes(bytes);
  }

  getUsageColor(percentage: number): string {
    return this.quotaService.getUsageColor(percentage);
  }

  upgradePlan() {
    // Navigate to subscription plans page
    this.router.navigate(['/subscription-plans']);
  }
}