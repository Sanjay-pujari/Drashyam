import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { Subscription, lastValueFrom } from 'rxjs';
import { AnalyticsDashboardService, AnalyticsSummary, TimeSeriesData, TopVideoAnalytics, RevenueAnalytics, GeographicAnalytics, DeviceAnalytics, EngagementAnalytics, VideoAnalytics, ChannelComparison } from '../../services/analytics-dashboard.service';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatSnackBarModule,
    FormsModule
  ],
  templateUrl: './analytics-dashboard.component.html',
  styleUrls: ['./analytics-dashboard.component.scss']
})
export class AnalyticsDashboardComponent implements OnInit, OnDestroy {
  // Data properties
  summary: AnalyticsSummary | null = null;
  timeSeriesData: TimeSeriesData[] = [];
  topVideos: TopVideoAnalytics[] = [];
  revenueAnalytics: RevenueAnalytics | null = null;
  geographicData: GeographicAnalytics[] = [];
  deviceData: DeviceAnalytics[] = [];
  engagementAnalytics: EngagementAnalytics | null = null;
  videoAnalytics: VideoAnalytics[] = [];
  channelComparison: ChannelComparison[] = [];

  // UI state
  isLoading = false;
  selectedTab = 0;
  dateRange = '7d'; // 7d, 30d, 90d, 1y, custom
  customStartDate: Date | null = null;
  customEndDate: Date | null = null;
  selectedChannel: number | null = null;
  selectedCountry: string | null = null;
  selectedDeviceType: string | null = null;

  // Date range options
  dateRangeOptions = [
    { value: '7d', label: 'Last 7 days' },
    { value: '30d', label: 'Last 30 days' },
    { value: '90d', label: 'Last 90 days' },
    { value: '1y', label: 'Last year' },
    { value: 'custom', label: 'Custom range' }
  ];

  // Chart options
  chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top' as const
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  private subscriptions: Subscription[] = [];

  constructor(
    private analyticsService: AnalyticsDashboardService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadAnalyticsData();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadAnalyticsData(): void {
    this.isLoading = true;
    const { startDate, endDate } = this.getDateRange();

    // Load all analytics data in parallel
    const summaryObs = this.analyticsService.getSummary(startDate, endDate);
    const timeSeriesObs = this.analyticsService.getTimeSeriesData(startDate, endDate);
    const topVideosObs = this.analyticsService.getTopVideos(10, startDate, endDate);
    const revenueObs = this.analyticsService.getRevenueAnalytics(startDate, endDate);
    const geographicObs = this.analyticsService.getGeographicData(startDate, endDate);
    const deviceObs = this.analyticsService.getDeviceData(startDate, endDate);
    const engagementObs = this.analyticsService.getEngagementMetrics(startDate, endDate);
    const videoObs = this.analyticsService.getVideoAnalyticsList(startDate, endDate);
    const channelObs = this.analyticsService.getChannelComparison(startDate, endDate);

    // Subscribe to all observables
    const summarySub = summaryObs.subscribe({
      next: (data) => this.summary = data,
      error: (error) => this.handleError('Failed to load analytics summary', error)
    });

    const timeSeriesSub = timeSeriesObs.subscribe({
      next: (data) => this.timeSeriesData = data,
      error: (error) => this.handleError('Failed to load time series data', error)
    });

    const topVideosSub = topVideosObs.subscribe({
      next: (data) => this.topVideos = data,
      error: (error) => this.handleError('Failed to load top videos', error)
    });

    const revenueSub = revenueObs.subscribe({
      next: (data) => this.revenueAnalytics = data,
      error: (error) => this.handleError('Failed to load revenue analytics', error)
    });

    const geographicSub = geographicObs.subscribe({
      next: (data) => this.geographicData = data,
      error: (error) => this.handleError('Failed to load geographic data', error)
    });

    const deviceSub = deviceObs.subscribe({
      next: (data) => this.deviceData = data,
      error: (error) => this.handleError('Failed to load device data', error)
    });

    const engagementSub = engagementObs.subscribe({
      next: (data) => this.engagementAnalytics = data,
      error: (error) => this.handleError('Failed to load engagement metrics', error)
    });

    const videoSub = videoObs.subscribe({
      next: (data) => this.videoAnalytics = data,
      error: (error) => this.handleError('Failed to load video analytics', error)
    });

    const channelSub = channelObs.subscribe({
      next: (data) => this.channelComparison = data,
      error: (error) => this.handleError('Failed to load channel comparison', error)
    });

    this.subscriptions.push(
      summarySub, timeSeriesSub, topVideosSub, revenueSub,
      geographicSub, deviceSub, engagementSub, videoSub, channelSub
    );

    // Wait for all requests to complete
    Promise.all([
      lastValueFrom(summaryObs),
      lastValueFrom(timeSeriesObs),
      lastValueFrom(topVideosObs),
      lastValueFrom(revenueObs),
      lastValueFrom(geographicObs),
      lastValueFrom(deviceObs),
      lastValueFrom(engagementObs),
      lastValueFrom(videoObs),
      lastValueFrom(channelObs)
    ]).finally(() => {
      this.isLoading = false;
    });
  }

  onDateRangeChange(): void {
    this.loadAnalyticsData();
  }

  onCustomDateChange(): void {
    if (this.dateRange === 'custom' && this.customStartDate && this.customEndDate) {
      this.loadAnalyticsData();
    }
  }

  onFilterChange(): void {
    this.loadAnalyticsData();
  }

  exportReport(): void {
    const { startDate, endDate } = this.getDateRange();
    if (!startDate || !endDate) {
      this.snackBar.open('Please select a valid date range', 'Close', { duration: 3000 });
      return;
    }

    this.analyticsService.exportAnalyticsReport(startDate, endDate, 'csv')
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `analytics-report-${startDate}-to-${endDate}.csv`;
          link.click();
          window.URL.revokeObjectURL(url);
          this.snackBar.open('Report exported successfully', 'Close', { duration: 3000 });
        },
        error: (error) => this.handleError('Failed to export report', error)
      });
  }

  refreshData(): void {
    this.loadAnalyticsData();
  }

  private getDateRange(): { startDate?: string; endDate?: string } {
    if (this.dateRange === 'custom') {
      return {
        startDate: this.customStartDate?.toISOString().split('T')[0],
        endDate: this.customEndDate?.toISOString().split('T')[0]
      };
    }

    const endDate = new Date();
    const startDate = new Date();

    switch (this.dateRange) {
      case '7d':
        startDate.setDate(endDate.getDate() - 7);
        break;
      case '30d':
        startDate.setDate(endDate.getDate() - 30);
        break;
      case '90d':
        startDate.setDate(endDate.getDate() - 90);
        break;
      case '1y':
        startDate.setFullYear(endDate.getFullYear() - 1);
        break;
    }

    return {
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0]
    };
  }

  private handleError(message: string, error: any): void {
    console.error(message, error);
    this.snackBar.open(message, 'Close', { duration: 5000 });
  }

  // Helper methods for template
  formatNumber(value: number | undefined | null): string {
    return this.analyticsService.formatNumber(value);
  }

  formatCurrency(value: number | undefined | null): string {
    return this.analyticsService.formatCurrency(value);
  }

  formatPercentage(value: number | undefined | null): string {
    return this.analyticsService.formatPercentage(value);
  }

  formatDuration(seconds: number | undefined | null): string {
    return this.analyticsService.formatDuration(seconds);
  }

  getEngagementTrend(): 'up' | 'down' | 'stable' {
    if (!this.engagementAnalytics) return 'stable';
    // This would need historical data to calculate trend
    return 'stable';
  }

  getRevenueTrend(): 'up' | 'down' | 'stable' {
    if (!this.revenueAnalytics) return 'stable';
    // This would need historical data to calculate trend
    return 'stable';
  }

  getTopCountry(): string {
    if (!this.geographicData.length) return 'N/A';
    return this.geographicData[0].country;
  }

  getTopDevice(): string {
    if (!this.deviceData.length) return 'N/A';
    return this.deviceData[0].deviceType;
  }

  getTotalRevenue(): number {
    return this.revenueAnalytics?.totalRevenue || 0;
  }

  getAdRevenue(): number {
    return this.revenueAnalytics?.adRevenue || 0;
  }

  getSubscriptionRevenue(): number {
    return this.revenueAnalytics?.subscriptionRevenue || 0;
  }

  getPremiumRevenue(): number {
    return this.revenueAnalytics?.premiumContentRevenue || 0;
  }

  getMerchandiseRevenue(): number {
    return this.revenueAnalytics?.merchandiseRevenue || 0;
  }

  getDonationRevenue(): number {
    return this.revenueAnalytics?.donationRevenue || 0;
  }
}
