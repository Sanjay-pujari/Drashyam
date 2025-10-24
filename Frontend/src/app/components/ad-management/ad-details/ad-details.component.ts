import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatStepperModule } from '@angular/material/stepper';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { MonetizationService, AdCampaignDto, AdDto } from '../../../services/monetization.service';

export interface AdDetailsData {
  type: 'campaign' | 'ad';
  data: AdCampaignDto | AdDto;
}

@Component({
  selector: 'app-ad-details',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTabsModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatBadgeModule,
    MatDividerModule,
    MatListModule,
    MatGridListModule,
    MatStepperModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSlideToggleModule,
    MatCheckboxModule,
    MatRadioModule,
    MatMenuModule
  ],
  templateUrl: './ad-details.component.html',
  styleUrls: ['./ad-details.component.scss']
})
export class AdDetailsComponent implements OnInit, OnDestroy {
  campaign!: AdCampaignDto;
  ad!: AdDto;
  isLoading = false;
  
  // Analytics data
  campaignAnalytics = {
    impressions: 0,
    clicks: 0,
    conversions: 0,
    ctr: 0,
    cpc: 0,
    cpm: 0,
    spend: 0,
    remainingBudget: 0
  };
  
  adAnalytics = {
    impressions: 0,
    clicks: 0,
    views: 0,
    ctr: 0,
    cpc: 0,
    cpm: 0,
    spend: 0
  };
  
  // Performance metrics
  performanceMetrics = {
    dailyImpressions: [],
    dailyClicks: [],
    dailySpend: [],
    topKeywords: [],
    audienceDemographics: [],
    deviceBreakdown: [],
    timeOfDayPerformance: []
  };
  
  // Table data
  adTableData: AdDto[] = [];
  performanceTableData: any[] = [];
  
  private destroy$ = new Subject<void>();

  constructor(
    private dialogRef: MatDialogRef<AdDetailsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AdDetailsData,
    @Inject(MonetizationService) private monetizationService: MonetizationService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadData(): void {
    if (this.data.type === 'campaign') {
      this.campaign = this.data.data as AdCampaignDto;
      this.loadCampaignAnalytics();
      this.loadCampaignAds();
    } else {
      this.ad = this.data.data as AdDto;
      this.loadAdAnalytics();
    }
  }

  private loadCampaignAnalytics(): void {
    this.isLoading = true;
    // Simulate API call
    setTimeout(() => {
      this.campaignAnalytics = {
        impressions: Math.floor(Math.random() * 10000) + 1000,
        clicks: Math.floor(Math.random() * 500) + 50,
        conversions: Math.floor(Math.random() * 50) + 5,
        ctr: Math.random() * 5 + 1,
        cpc: Math.random() * 2 + 0.5,
        cpm: Math.random() * 10 + 2,
        spend: this.campaign.spent,
        remainingBudget: this.campaign.budget - this.campaign.spent
      };
      this.isLoading = false;
    }, 1000);
  }

  private loadCampaignAds(): void {
    this.adTableData = this.campaign.ads || [];
  }

  private loadAdAnalytics(): void {
    this.isLoading = true;
    // Simulate API call
    setTimeout(() => {
      this.adAnalytics = {
        impressions: Math.floor(Math.random() * 5000) + 500,
        clicks: Math.floor(Math.random() * 250) + 25,
        views: Math.floor(Math.random() * 1000) + 100,
        ctr: Math.random() * 8 + 2,
        cpc: this.ad.costPerClick,
        cpm: Math.random() * 15 + 5,
        spend: Math.random() * 100 + 10
      };
      this.isLoading = false;
    }, 1000);
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return 'primary';
      case 'paused': return 'warn';
      case 'completed': return 'accent';
      case 'draft': return 'basic';
      default: return 'basic';
    }
  }

  getTypeColor(type: string): string {
    switch (type.toLowerCase()) {
      case 'video': return 'primary';
      case 'banner': return 'accent';
      case 'overlay': return 'warn';
      default: return 'basic';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  formatNumber(num: number): string {
    return new Intl.NumberFormat('en-US').format(num);
  }

  formatPercentage(num: number): string {
    return `${num.toFixed(2)}%`;
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }

  getBudgetProgress(): number {
    if (this.campaign) {
      return (this.campaign.spent / this.campaign.budget) * 100;
    }
    return 0;
  }

  getBudgetStatus(): string {
    const progress = this.getBudgetProgress();
    if (progress >= 90) return 'warning';
    if (progress >= 75) return 'caution';
    return 'good';
  }

  getPerformanceGrade(): string {
    if (this.data.type === 'campaign') {
      const ctr = this.campaignAnalytics.ctr;
      if (ctr >= 3) return 'A';
      if (ctr >= 2) return 'B';
      if (ctr >= 1) return 'C';
      return 'D';
    } else {
      const ctr = this.adAnalytics.ctr;
      if (ctr >= 5) return 'A';
      if (ctr >= 3) return 'B';
      if (ctr >= 1) return 'C';
      return 'D';
    }
  }

  getPerformanceColor(): string {
    const grade = this.getPerformanceGrade();
    switch (grade) {
      case 'A': return 'primary';
      case 'B': return 'accent';
      case 'C': return 'warn';
      case 'D': return 'warn';
      default: return 'basic';
    }
  }

  editCampaign(): void {
    // Open edit dialog
    this.snackBar.open('Edit campaign functionality coming soon', 'Close', { duration: 3000 });
  }

  editAd(): void {
    // Open edit dialog
    this.snackBar.open('Edit ad functionality coming soon', 'Close', { duration: 3000 });
  }

  pauseCampaign(): void {
    this.snackBar.open('Campaign paused', 'Close', { duration: 3000 });
  }

  resumeCampaign(): void {
    this.snackBar.open('Campaign resumed', 'Close', { duration: 3000 });
  }

  pauseAd(): void {
    this.snackBar.open('Ad paused', 'Close', { duration: 3000 });
  }

  resumeAd(): void {
    this.snackBar.open('Ad resumed', 'Close', { duration: 3000 });
  }

  duplicateCampaign(): void {
    this.snackBar.open('Campaign duplicated', 'Close', { duration: 3000 });
  }

  duplicateAd(): void {
    this.snackBar.open('Ad duplicated', 'Close', { duration: 3000 });
  }

  exportData(): void {
    this.snackBar.open('Data exported successfully', 'Close', { duration: 3000 });
  }

  close(): void {
    this.dialogRef.close();
  }
}
