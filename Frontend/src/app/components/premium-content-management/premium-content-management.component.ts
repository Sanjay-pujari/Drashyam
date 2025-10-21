import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { PremiumContentService } from '../../services/premium-content.service';
import { VideoService } from '../../services/video.service';
import { UserService } from '../../services/user.service';
import { 
  PremiumVideo, 
  PremiumVideoCreate, 
  PremiumVideoUpdate, 
  PremiumPurchase, 
  PremiumContentAnalytics,
  PagedResult,
  PremiumPurchaseStatus 
} from '../../models/premium-content.model';
import { Video } from '../../models/video.model';

@Component({
  selector: 'app-premium-content-management',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    DatePipe, 
    DecimalPipe,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatTabsModule,
    MatTableModule,
    MatPaginatorModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './premium-content-management.component.html',
  styleUrls: ['./premium-content-management.component.scss']
})
export class PremiumContentManagementComponent implements OnInit {
  Math = Math; // Make Math available in template
  PremiumPurchaseStatus = PremiumPurchaseStatus; // Make enum available in template
  
  // Forms
  premiumVideoForm: FormGroup;
  
  // Data
  premiumVideos: PremiumVideo[] = [];
  purchases: PremiumPurchase[] = [];
  availableVideos: Video[] = [];
  analytics: PremiumContentAnalytics | null = null;
  currentUserId: string = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  
  // UI State
  loading = false;
  activeTab = 0;
  selectedPremiumVideo: PremiumVideo | null = null;
  
  // Table columns
  displayedColumns: string[] = ['videoTitle', 'price', 'currency', 'isActive', 'createdAt', 'actions'];
  purchaseColumns: string[] = ['videoTitle', 'amount', 'currency', 'status', 'purchasedAt', 'actions'];

  constructor(
    private fb: FormBuilder,
    private premiumContentService: PremiumContentService,
    private videoService: VideoService,
    private userService: UserService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.premiumVideoForm = this.fb.group({
      videoId: ['', Validators.required],
      price: ['', [Validators.required, Validators.min(0.01)]],
      currency: ['USD', Validators.required]
    });
  }

  ngOnInit(): void {
    this.getCurrentUser();
    this.loadPremiumVideos();
    this.loadUserPurchases();
  }

  getCurrentUser(): void {
    this.userService.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUserId = user.id;
        this.loadAvailableVideos();
      },
      error: (error) => {
        console.error('Error getting current user:', error);
        this.snackBar.open('Error loading user data', 'Close', { duration: 3000 });
      }
    });
  }

  loadPremiumVideos(): void {
    this.loading = true;
    this.premiumContentService.getPremiumVideos(this.currentPage, this.pageSize)
      .subscribe({
        next: (result: PagedResult<PremiumVideo>) => {
          this.premiumVideos = result.items;
          this.totalCount = result.totalCount;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading premium videos:', error);
          this.snackBar.open('Error loading premium videos', 'Close', { duration: 3000 });
          this.loading = false;
        }
      });
  }

  loadAvailableVideos(): void {
    if (this.currentUserId) {
      this.videoService.getUserVideos(this.currentUserId, { page: 1, pageSize: 100 })
        .subscribe({
          next: (result) => {
            this.availableVideos = result.items;
          },
          error: (error) => {
            console.error('Error loading available videos:', error);
          }
        });
    }
  }

  loadUserPurchases(): void {
    this.premiumContentService.getUserPurchases(1, 50)
      .subscribe({
        next: (result: PagedResult<PremiumPurchase>) => {
          this.purchases = result.items;
        },
        error: (error) => {
          console.error('Error loading purchases:', error);
        }
      });
  }

  loadAnalytics(premiumVideoId: number): void {
    this.premiumContentService.getPremiumContentAnalytics(premiumVideoId)
      .subscribe({
        next: (analytics) => {
          this.analytics = analytics;
        },
        error: (error) => {
          console.error('Error loading analytics:', error);
        }
      });
  }

  onSubmit(): void {
    if (this.premiumVideoForm.valid) {
      const formValue = this.premiumVideoForm.value;
      const premiumVideoCreate: PremiumVideoCreate = {
        videoId: formValue.videoId,
        creatorId: '', // Will be set by backend from JWT
        price: formValue.price,
        currency: formValue.currency
      };

      this.loading = true;
      this.premiumContentService.createPremiumVideo(premiumVideoCreate)
        .subscribe({
          next: (premiumVideo) => {
            this.snackBar.open('Premium video created successfully', 'Close', { duration: 3000 });
            this.premiumVideoForm.reset();
            this.loadPremiumVideos();
            this.loading = false;
          },
          error: (error) => {
            console.error('Error creating premium video:', error);
            this.snackBar.open('Error creating premium video', 'Close', { duration: 3000 });
            this.loading = false;
          }
        });
    }
  }

  updatePremiumVideo(premiumVideo: PremiumVideo): void {
    const updateData: PremiumVideoUpdate = {
      creatorId: '', // Will be set by backend
      price: premiumVideo.price,
      currency: premiumVideo.currency,
      isActive: premiumVideo.isActive
    };

    this.loading = true;
    this.premiumContentService.updatePremiumVideo(premiumVideo.id, updateData)
      .subscribe({
        next: (updatedVideo) => {
          this.snackBar.open('Premium video updated successfully', 'Close', { duration: 3000 });
          this.loadPremiumVideos();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error updating premium video:', error);
          this.snackBar.open('Error updating premium video', 'Close', { duration: 3000 });
          this.loading = false;
        }
      });
  }

  deletePremiumVideo(id: number): void {
    if (confirm('Are you sure you want to delete this premium video?')) {
      this.loading = true;
      this.premiumContentService.deletePremiumVideo(id)
        .subscribe({
          next: () => {
            this.snackBar.open('Premium video deleted successfully', 'Close', { duration: 3000 });
            this.loadPremiumVideos();
            this.loading = false;
          },
          error: (error) => {
            console.error('Error deleting premium video:', error);
            this.snackBar.open('Error deleting premium video', 'Close', { duration: 3000 });
            this.loading = false;
          }
        });
    }
  }

  toggleActive(premiumVideo: PremiumVideo): void {
    premiumVideo.isActive = !premiumVideo.isActive;
    this.updatePremiumVideo(premiumVideo);
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadPremiumVideos();
  }

  onTabChange(index: number): void {
    this.activeTab = index;
    if (index === 2 && this.selectedPremiumVideo) {
      this.loadAnalytics(this.selectedPremiumVideo.id);
    }
  }

  selectPremiumVideo(premiumVideo: PremiumVideo): void {
    this.selectedPremiumVideo = premiumVideo;
    this.loadAnalytics(premiumVideo.id);
  }

  getStatusColor(status: PremiumPurchaseStatus): string {
    switch (status) {
      case PremiumPurchaseStatus.Completed:
        return 'green';
      case PremiumPurchaseStatus.Pending:
        return 'orange';
      case PremiumPurchaseStatus.Failed:
        return 'red';
      case PremiumPurchaseStatus.Refunded:
        return 'gray';
      case PremiumPurchaseStatus.Cancelled:
        return 'red';
      default:
        return 'gray';
    }
  }
}
