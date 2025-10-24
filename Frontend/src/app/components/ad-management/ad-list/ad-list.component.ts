import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { MonetizationService, AdCampaignDto, AdDto } from '../../../services/monetization.service';
import { AdFormComponent } from '../ad-form/ad-form.component';
import { AdDetailsComponent } from '../ad-details/ad-details.component';

@Component({
  selector: 'app-ad-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatMenuModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './ad-list.component.html',
  styleUrls: ['./ad-list.component.scss']
})
export class AdListComponent implements OnInit, OnDestroy {
  campaigns: AdCampaignDto[] = [];
  filteredCampaigns: AdCampaignDto[] = [];
  allAds: AdDto[] = [];
  filteredAds: AdDto[] = [];
  
  isLoading = false;
  searchTerm = '';
  statusFilter = '';
  typeFilter = '';
  dateRange = { start: null as Date | null, end: null as Date | null };
  
  // Pagination
  pageSize = 10;
  pageIndex = 0;
  totalItems = 0;
  
  // View mode
  viewMode: 'campaigns' | 'ads' = 'campaigns';
  
  // Table columns
  campaignColumns = ['name', 'status', 'budget', 'spent', 'startDate', 'endDate', 'actions'];
  adColumns = ['content', 'type', 'costPerClick', 'costPerView', 'duration', 'position', 'actions'];
  
  private destroy$ = new Subject<void>();

  constructor(
    @Inject(MonetizationService) private monetizationService: MonetizationService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadCampaigns();
    this.setupSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    // Setup search debouncing
    // This would be implemented with a search input in the template
  }

  loadCampaigns(): void {
    this.isLoading = true;
    this.monetizationService.getAdCampaigns().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (campaigns: AdCampaignDto[]) => {
        this.campaigns = campaigns;
        this.filteredCampaigns = campaigns;
        this.extractAllAds();
        this.applyFilters();
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading campaigns:', error);
        this.snackBar.open('Error loading campaigns', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  private extractAllAds(): void {
    this.allAds = this.campaigns.flatMap(campaign => campaign.ads || []);
    this.filteredAds = this.allAds;
  }

  applyFilters(): void {
    if (this.viewMode === 'campaigns') {
      let filtered = [...this.campaigns];
      
      // Search filter
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase();
        filtered = filtered.filter((campaign: AdCampaignDto) => 
          campaign.name.toLowerCase().includes(term) ||
          campaign.advertiserId.toLowerCase().includes(term)
        );
      }
      
      // Status filter
      if (this.statusFilter) {
        filtered = filtered.filter((campaign: AdCampaignDto) => 
          campaign.status.toLowerCase() === this.statusFilter.toLowerCase()
        );
      }
      
      // Date range filter
      if (this.dateRange.start && this.dateRange.end) {
        filtered = filtered.filter((campaign: AdCampaignDto) => {
          const startDate = new Date(campaign.startDate);
          const endDate = new Date(campaign.endDate);
          return startDate >= this.dateRange.start! && endDate <= this.dateRange.end!;
        });
      }
      
      this.filteredCampaigns = filtered;
      this.totalItems = this.filteredCampaigns.length;
    } else {
      let filtered = [...this.allAds];
      
      // Search filter
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase();
        filtered = filtered.filter((ad: AdDto) => 
          ad.content.toLowerCase().includes(term) ||
          ad.type.toLowerCase().includes(term)
        );
      }
      
      // Type filter
      if (this.typeFilter) {
        filtered = filtered.filter((ad: AdDto) => 
          ad.type.toLowerCase() === this.typeFilter.toLowerCase()
        );
      }
      
      this.filteredAds = filtered;
      this.totalItems = this.filteredAds.length;
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  getCurrentPageData(): any[] {
    const startIndex = this.pageIndex * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    
    if (this.viewMode === 'campaigns') {
      return this.filteredCampaigns.slice(startIndex, endIndex);
    } else {
      return this.filteredAds.slice(startIndex, endIndex);
    }
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'campaigns' ? 'ads' : 'campaigns';
    this.pageIndex = 0;
    this.applyFilters();
  }

  createCampaign(): void {
    const dialogRef = this.dialog.open(AdFormComponent, {
      width: '800px',
      data: { mode: 'create', type: 'campaign' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCampaigns();
        this.snackBar.open('Campaign created successfully', 'Close', { duration: 3000 });
      }
    });
  }

  createAd(campaignId?: number): void {
    const dialogRef = this.dialog.open(AdFormComponent, {
      width: '800px',
      data: { mode: 'create', type: 'ad', campaignId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCampaigns();
        this.snackBar.open('Ad created successfully', 'Close', { duration: 3000 });
      }
    });
  }

  editCampaign(campaign: AdCampaignDto): void {
    const dialogRef = this.dialog.open(AdFormComponent, {
      width: '800px',
      data: { mode: 'edit', type: 'campaign', campaign }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCampaigns();
        this.snackBar.open('Campaign updated successfully', 'Close', { duration: 3000 });
      }
    });
  }

  editAd(ad: AdDto): void {
    const dialogRef = this.dialog.open(AdFormComponent, {
      width: '800px',
      data: { mode: 'edit', type: 'ad', ad }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCampaigns();
        this.snackBar.open('Ad updated successfully', 'Close', { duration: 3000 });
      }
    });
  }

  viewCampaign(campaign: AdCampaignDto): void {
    const dialogRef = this.dialog.open(AdDetailsComponent, {
      width: '900px',
      data: { type: 'campaign', data: campaign }
    });
  }

  viewAd(ad: AdDto): void {
    const dialogRef = this.dialog.open(AdDetailsComponent, {
      width: '900px',
      data: { type: 'ad', data: ad }
    });
  }

  deleteCampaign(campaign: AdCampaignDto): void {
    if (confirm(`Are you sure you want to delete campaign "${campaign.name}"?`)) {
      // Implement delete logic
      this.snackBar.open('Campaign deleted successfully', 'Close', { duration: 3000 });
      this.loadCampaigns();
    }
  }

  deleteAd(ad: AdDto): void {
    if (confirm(`Are you sure you want to delete this ad?`)) {
      // Implement delete logic
      this.snackBar.open('Ad deleted successfully', 'Close', { duration: 3000 });
      this.loadCampaigns();
    }
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

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.statusFilter = '';
    this.typeFilter = '';
    this.dateRange = { start: null, end: null };
    this.pageIndex = 0;
    this.applyFilters();
  }
}
