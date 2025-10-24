import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, interval } from 'rxjs';
import { LiveStreamService } from '../../services/live-stream.service';
import { SignalRService } from '../../services/signalr.service';

export interface StreamAnalytics {
  streamId: string;
  viewerCount: number;
  peakViewerCount: number;
  totalViews: number;
  averageWatchTime: number;
  engagementRate: number;
  chatMessages: number;
  reactions: number;
  shares: number;
  revenue: number;
  timestamp: Date;
}

export interface ViewerDemographics {
  ageGroups: { [key: string]: number };
  genders: { [key: string]: number };
  locations: { [key: string]: number };
  devices: { [key: string]: number };
}

export interface StreamMetrics {
  bitrate: number;
  fps: number;
  resolution: string;
  cpuUsage: number;
  memoryUsage: number;
  networkLatency: number;
  droppedFrames: number;
  bufferHealth: number;
}

@Component({
  selector: 'app-stream-analytics-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stream-analytics-dashboard.component.html',
  styleUrls: ['./stream-analytics-dashboard.component.scss']
})
export class StreamAnalyticsDashboardComponent implements OnInit, OnDestroy {
  @Input() streamId: string = '';
  @Input() isLive: boolean = false;
  @Input() autoRefresh: boolean = true;
  @Input() refreshInterval: number = 5000; // 5 seconds
  @Output() analyticsUpdated = new EventEmitter<StreamAnalytics>();

  // Analytics data
  analytics: StreamAnalytics | null = null;
  demographics: ViewerDemographics | null = null;
  metrics: StreamMetrics | null = null;
  historicalData: StreamAnalytics[] = [];
  
  // UI state
  selectedTab: 'overview' | 'demographics' | 'metrics' | 'revenue' = 'overview';
  timeRange: '1h' | '6h' | '24h' | '7d' = '1h';
  showRealTime = true;
  isExpanded = false;
  
  // Charts data
  viewerChartData: any[] = [];
  engagementChartData: any[] = [];
  revenueChartData: any[] = [];
  
  // Real-time updates
  private destroy$ = new Subject<void>();
  private refreshInterval$: any;

  constructor(
    private liveStreamService: LiveStreamService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.setupSignalRConnection();
    this.loadAnalytics();
    
    if (this.autoRefresh) {
      this.startAutoRefresh();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stopAutoRefresh();
  }

  private setupSignalRConnection(): void {
    // Listen for real-time analytics updates
    this.signalRService.onStreamAnalytics()
      .pipe(takeUntil(this.destroy$))
      .subscribe(analytics => {
        if (analytics.streamId === this.streamId) {
          this.updateAnalytics(analytics);
        }
      });

    // Listen for viewer count updates
    this.signalRService.onViewerCountUpdate()
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        if (update.streamKey === this.streamId) {
          this.updateViewerCount(update.viewerCount);
        }
      });
  }

  private async loadAnalytics(): Promise<void> {
    try {
      // Load current analytics
      const analytics = await this.liveStreamService.getStreamAnalytics(this.streamId).toPromise();
      if (analytics) {
        this.updateAnalytics(analytics);
      }

      // Load historical data
      const historical = await this.liveStreamService.getStreamAnalyticsHistory(this.streamId, this.timeRange).toPromise();
      if (historical) {
        this.historicalData = historical;
        this.updateCharts();
      }

      // Load demographics
      const demographics = await this.liveStreamService.getStreamDemographics(this.streamId).toPromise();
      if (demographics) {
        this.demographics = demographics;
      }

      // Load technical metrics
      const metrics = await this.liveStreamService.getStreamMetrics(this.streamId).toPromise();
      if (metrics) {
        this.metrics = metrics;
      }
    } catch (error) {
      console.error('Error loading analytics:', error);
    }
  }

  private updateAnalytics(analytics: StreamAnalytics): void {
    this.analytics = analytics;
    this.analyticsUpdated.emit(analytics);
    
    // Add to historical data
    this.historicalData.push(analytics);
    
    // Keep only last 100 data points
    if (this.historicalData.length > 100) {
      this.historicalData = this.historicalData.slice(-100);
    }
    
    this.updateCharts();
  }

  private updateViewerCount(viewerCount: number): void {
    if (this.analytics) {
      this.analytics.viewerCount = viewerCount;
      this.analytics.peakViewerCount = Math.max(this.analytics.peakViewerCount, viewerCount);
    }
  }

  private updateCharts(): void {
    this.updateViewerChart();
    this.updateEngagementChart();
    this.updateRevenueChart();
  }

  private updateViewerChart(): void {
    this.viewerChartData = this.historicalData.map(data => ({
      time: data.timestamp,
      viewers: data.viewerCount,
      peak: data.peakViewerCount
    }));
  }

  private updateEngagementChart(): void {
    this.engagementChartData = this.historicalData.map(data => ({
      time: data.timestamp,
      engagement: data.engagementRate,
      chat: data.chatMessages,
      reactions: data.reactions,
      shares: data.shares
    }));
  }

  private updateRevenueChart(): void {
    this.revenueChartData = this.historicalData.map(data => ({
      time: data.timestamp,
      revenue: data.revenue
    }));
  }

  private startAutoRefresh(): void {
    this.refreshInterval$ = interval(this.refreshInterval)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadAnalytics();
      });
  }

  private stopAutoRefresh(): void {
    if (this.refreshInterval$) {
      this.refreshInterval$.unsubscribe();
    }
  }

  // UI Controls
  selectTab(tab: 'overview' | 'demographics' | 'metrics' | 'revenue'): void {
    this.selectedTab = tab;
  }

  changeTimeRange(range: '1h' | '6h' | '24h' | '7d'): void {
    this.timeRange = range;
    this.loadAnalytics();
  }

  toggleRealTime(): void {
    this.showRealTime = !this.showRealTime;
    if (this.showRealTime) {
      this.startAutoRefresh();
    } else {
      this.stopAutoRefresh();
    }
  }

  toggleExpanded(): void {
    this.isExpanded = !this.isExpanded;
  }

  refreshAnalytics(): void {
    this.loadAnalytics();
  }

  exportAnalytics(): void {
    if (!this.analytics) return;

    const exportData = {
      streamId: this.streamId,
      analytics: this.analytics,
      demographics: this.demographics,
      metrics: this.metrics,
      historicalData: this.historicalData,
      exportedAt: new Date().toISOString()
    };

    const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `stream-analytics-${this.streamId}-${new Date().toISOString().split('T')[0]}.json`;
    a.click();
    URL.revokeObjectURL(url);
  }

  // Utility methods
  formatNumber(value: number): string {
    if (value >= 1000000) {
      return `${(value / 1000000).toFixed(1)}M`;
    } else if (value >= 1000) {
      return `${(value / 1000).toFixed(1)}K`;
    } else {
      return value.toString();
    }
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  }

  formatDuration(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);
    
    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    } else {
      return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }
  }

  getEngagementColor(rate: number): string {
    if (rate >= 80) return '#4CAF50';
    if (rate >= 60) return '#8BC34A';
    if (rate >= 40) return '#FFC107';
    if (rate >= 20) return '#FF9800';
    return '#F44336';
  }

  getHealthStatus(): 'excellent' | 'good' | 'fair' | 'poor' {
    if (!this.metrics) return 'fair';
    
    const { cpuUsage, memoryUsage, networkLatency, bufferHealth } = this.metrics;
    
    if (cpuUsage < 50 && memoryUsage < 70 && networkLatency < 100 && bufferHealth > 90) {
      return 'excellent';
    } else if (cpuUsage < 70 && memoryUsage < 85 && networkLatency < 200 && bufferHealth > 80) {
      return 'good';
    } else if (cpuUsage < 85 && memoryUsage < 95 && networkLatency < 500 && bufferHealth > 70) {
      return 'fair';
    } else {
      return 'poor';
    }
  }

  getHealthColor(): string {
    const status = this.getHealthStatus();
    switch (status) {
      case 'excellent': return '#4CAF50';
      case 'good': return '#8BC34A';
      case 'fair': return '#FFC107';
      case 'poor': return '#F44336';
      default: return '#9E9E9E';
    }
  }

  getTopLocation(): string {
    if (!this.demographics?.locations) return 'Unknown';
    
    const locations = Object.entries(this.demographics.locations);
    if (locations.length === 0) return 'Unknown';
    
    return locations.reduce((a, b) => a[1] > b[1] ? a : b)[0];
  }

  getTopDevice(): string {
    if (!this.demographics?.devices) return 'Unknown';
    
    const devices = Object.entries(this.demographics.devices);
    if (devices.length === 0) return 'Unknown';
    
    return devices.reduce((a, b) => a[1] > b[1] ? a : b)[0];
  }

  getAverageAge(): number {
    if (!this.demographics?.ageGroups) return 0;
    
    const ageGroups = Object.entries(this.demographics.ageGroups);
    if (ageGroups.length === 0) return 0;
    
    let totalAge = 0;
    let totalCount = 0;
    
    ageGroups.forEach(([ageRange, count]) => {
      const [min, max] = ageRange.split('-').map(Number);
      const avgAge = (min + max) / 2;
      totalAge += avgAge * count;
      totalCount += count;
    });
    
    return totalCount > 0 ? Math.round(totalAge / totalCount) : 0;
  }

  getSegmentColor(key: string): string {
    const colors: { [key: string]: string } = {
      'Male': '#2196F3',
      'Female': '#E91E63',
      'Other': '#9C27B0',
      'Mobile': '#4CAF50',
      'Desktop': '#FF9800',
      'Tablet': '#9C27B0'
    };
    return colors[key] || '#607D8B';
  }

  // Helper methods for template calculations
  getViewerChartHeight(point: any): string {
    if (!this.viewerChartData.length) return '0%';
    const maxViewers = Math.max(...this.viewerChartData.map(p => p.viewers));
    return `${(point.viewers / maxViewers) * 100}%`;
  }

  getAgeGroupBarWidth(ageGroup: any): string {
    if (!this.demographics?.ageGroups) return '0%';
    const values = Object.values(this.demographics.ageGroups);
    const maxValue = Math.max(...values);
    return `${(ageGroup.value / maxValue) * 100}%`;
  }

  getRevenueBarHeight(data: any): string {
    if (!this.revenueChartData.length) return '0%';
    const maxRevenue = Math.max(...this.revenueChartData.map(d => d.revenue));
    return `${(data.revenue / maxRevenue) * 100}%`;
  }

  getLatencyHeight(): string {
    const latency = this.metrics?.networkLatency || 0;
    return `${Math.min(latency / 10, 100)}%`;
  }
}
