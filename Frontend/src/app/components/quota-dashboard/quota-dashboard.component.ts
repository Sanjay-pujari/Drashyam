import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { QuotaService, QuotaStatus, QuotaWarning, SubscriptionBenefits } from '../../services/quota.service';

@Component({
  selector: 'app-quota-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatProgressBarModule,
    MatChipsModule
  ],
  template: `
    <div class="quota-dashboard">
      <div class="header">
        <h1>Usage & Quotas</h1>
        <p>Monitor your account usage and subscription benefits</p>
      </div>

      <div class="content" *ngIf="quotaStatus && subscriptionBenefits">
        <!-- Current Plan -->
        <mat-card class="plan-card">
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

        <!-- Quota Warnings -->
        <mat-card class="warnings-card" *ngIf="quotaWarnings?.hasWarnings">
          <div class="warnings-header">
            <mat-icon color="warn">warning</mat-icon>
            <h3>Quota Warnings</h3>
          </div>
          <div class="warnings-content">
            <p *ngFor="let warning of quotaWarnings?.warnings">{{ warning }}</p>
            <p class="recommended-action">{{ quotaWarnings?.recommendedAction }}</p>
          </div>
        </mat-card>

        <!-- Usage Statistics -->
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

        <!-- Subscription Benefits -->
        <mat-card class="benefits-card">
          <div class="benefits-header">
            <h3>Current Plan Benefits</h3>
          </div>
          <div class="benefits-content">
            <div class="benefits-list">
              <div class="benefit-item" *ngFor="let benefit of subscriptionBenefits.benefits">
                <mat-icon [color]="getFeatureColor(benefit)">{{ getFeatureIcon(benefit) }}</mat-icon>
                <span>{{ benefit }}</span>
              </div>
            </div>
          </div>
        </mat-card>

        <!-- Upgrade Benefits -->
        <mat-card class="upgrade-card" *ngIf="subscriptionBenefits.nextUpgradePlan">
          <div class="upgrade-header">
            <h3>Upgrade to {{ subscriptionBenefits.nextUpgradePlan.name }}</h3>
            <p>Get more storage, features, and capabilities</p>
          </div>
          <div class="upgrade-content">
            <div class="upgrade-benefits">
              <div class="benefit-item" *ngFor="let benefit of subscriptionBenefits.upgradeBenefits">
                <mat-icon color="primary">add_circle</mat-icon>
                <span>{{ benefit }}</span>
              </div>
            </div>
            <div class="upgrade-actions">
              <button mat-raised-button color="primary" (click)="upgradePlan()">
                <mat-icon>upgrade</mat-icon>
                Upgrade Now
              </button>
            </div>
          </div>
        </mat-card>
      </div>

      <!-- Loading State -->
      <div class="loading" *ngIf="!quotaStatus">
        <mat-icon>refresh</mat-icon>
        <p>Loading quota information...</p>
      </div>
    </div>
  `,
  styles: [`
    .quota-dashboard {
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

    .plan-card {
      margin-bottom: 20px;
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

    .warnings-card {
      margin-bottom: 20px;
      border-left: 4px solid #f44336;
    }

    .warnings-header {
      display: flex;
      align-items: center;
      margin-bottom: 12px;
    }

    .warnings-header mat-icon {
      margin-right: 8px;
    }

    .warnings-header h3 {
      margin: 0;
      color: #f44336;
    }

    .warnings-content p {
      margin: 0 0 8px 0;
      color: #666;
    }

    .recommended-action {
      font-weight: 500;
      color: #333 !important;
    }

    .usage-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
      margin-bottom: 20px;
    }

    .usage-card {
      padding: 20px;
    }

    .usage-header {
      display: flex;
      align-items: center;
      margin-bottom: 16px;
    }

    .usage-header mat-icon {
      margin-right: 8px;
      color: #666;
    }

    .usage-header h3 {
      margin: 0;
      color: #333;
    }

    .usage-stats {
      display: flex;
      align-items: center;
      margin-bottom: 12px;
    }

    .usage-stats .used {
      font-size: 1.5rem;
      font-weight: 600;
      color: #333;
    }

    .usage-stats .separator {
      margin: 0 8px;
      color: #666;
    }

    .usage-stats .limit {
      color: #666;
    }

    .usage-percentage {
      text-align: right;
      font-size: 0.9rem;
      color: #666;
      margin-top: 8px;
    }

    .benefits-card, .upgrade-card {
      margin-bottom: 20px;
    }

    .benefits-header, .upgrade-header {
      margin-bottom: 16px;
    }

    .benefits-header h3, .upgrade-header h3 {
      margin: 0 0 8px 0;
      color: #333;
    }

    .upgrade-header p {
      margin: 0;
      color: #666;
    }

    .benefits-list, .upgrade-benefits {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 12px;
    }

    .benefit-item {
      display: flex;
      align-items: center;
      padding: 8px 0;
    }

    .benefit-item mat-icon {
      margin-right: 12px;
      font-size: 20px;
    }

    .benefit-item span {
      color: #333;
    }

    .upgrade-actions {
      margin-top: 20px;
      text-align: center;
    }

    .loading {
      text-align: center;
      padding: 40px;
    }

    .loading mat-icon {
      font-size: 3rem;
      width: 3rem;
      height: 3rem;
      color: #ccc;
      margin-bottom: 16px;
    }

    .loading p {
      color: #666;
      margin: 0;
    }
  `]
})
export class QuotaDashboardComponent implements OnInit, OnDestroy {
  quotaStatus: QuotaStatus | null = null;
  quotaWarnings: QuotaWarning | null = null;
  subscriptionBenefits: SubscriptionBenefits | null = null;
  loading = true;

  private subscriptions: Subscription[] = [];

  constructor(
    private quotaService: QuotaService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadQuotaData();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private loadQuotaData() {
    this.loading = true;

    // Load quota status
    const statusSub = this.quotaService.getQuotaStatus().subscribe({
      next: (status) => {
        this.quotaStatus = status;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading quota status:', error);
        this.snackBar.open('Failed to load quota information', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });

    // Load quota warnings
    const warningsSub = this.quotaService.getQuotaWarnings().subscribe({
      next: (warnings) => {
        this.quotaWarnings = warnings;
      },
      error: (error) => {
        console.error('Error loading quota warnings:', error);
      }
    });

    // Load subscription benefits
    const benefitsSub = this.quotaService.getSubscriptionBenefits().subscribe({
      next: (benefits) => {
        this.subscriptionBenefits = benefits;
      },
      error: (error) => {
        console.error('Error loading subscription benefits:', error);
      }
    });

    this.subscriptions.push(statusSub, warningsSub, benefitsSub);
  }

  formatBytes(bytes: number): string {
    return this.quotaService.formatBytes(bytes);
  }

  getUsageColor(percentage: number): string {
    return this.quotaService.getUsageColor(percentage);
  }

  getFeatureIcon(benefit: string): string {
    if (benefit.includes('channels')) return 'account_circle';
    if (benefit.includes('videos')) return 'video_library';
    if (benefit.includes('storage')) return 'storage';
    if (benefit.includes('Ad-free')) return 'block';
    if (benefit.includes('analytics')) return 'analytics';
    if (benefit.includes('Monetization')) return 'attach_money';
    if (benefit.includes('streaming')) return 'live_tv';
    return 'check_circle';
  }

  getFeatureColor(benefit: string): string {
    if (benefit.includes('Ad-free')) return 'primary';
    if (benefit.includes('analytics')) return 'accent';
    if (benefit.includes('Monetization')) return 'warn';
    if (benefit.includes('streaming')) return 'primary';
    return 'primary';
  }

  upgradePlan() {
    this.router.navigate(['/subscriptions']);
    this.snackBar.open('Redirecting to subscription plans...', 'Close', { duration: 2000 });
  }
}
