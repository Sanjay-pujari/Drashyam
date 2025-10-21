import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CdkDragDrop, CdkDrag, moveItemInArray } from '@angular/cdk/drag-drop';
import { Subscription } from 'rxjs';
import { MerchandiseService } from '../../services/merchandise.service';
import { MerchandiseStore, MerchandiseStoreCreate, MerchandiseStoreUpdate, StorePlatform, STORE_PLATFORM_OPTIONS } from '../../models/merchandise.model';

@Component({
  selector: 'app-merchandise-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatDialogModule,
    MatChipsModule,
    MatTooltipModule,
    CdkDrag
  ],
  template: `
    <div class="merchandise-management-container">
      <div class="header">
        <h2>Merchandise Stores</h2>
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          Add Store
        </button>
      </div>

      <div class="content" *ngIf="!isLoading; else loading">
        <div class="empty-state" *ngIf="stores.length === 0">
          <mat-icon>store</mat-icon>
          <h3>No merchandise stores yet</h3>
          <p>Add your first store to start selling merchandise</p>
          <button mat-raised-button color="primary" (click)="openCreateDialog()">
            <mat-icon>add</mat-icon>
            Add Store
          </button>
        </div>

        <div class="stores-list" *ngIf="stores.length > 0">
          <div 
            class="store-card" 
            *ngFor="let store of stores; let i = index"
            cdkDrag
            [cdkDragData]="store"
            [class.dragging]="isDragging"
          >
            <div class="store-header">
              <div class="store-info">
                <div class="store-logo" *ngIf="store.logoUrl">
                  <img [src]="store.logoUrl" [alt]="store.storeName">
                </div>
                <div class="store-details">
                  <h3>{{ store.storeName }}</h3>
                  <p class="platform">{{ getPlatformLabel(store.platform) }}</p>
                  <p class="url" *ngIf="store.storeUrl">
                    <mat-icon>link</mat-icon>
                    <a [href]="store.storeUrl" target="_blank" rel="noopener noreferrer">
                      {{ store.storeUrl }}
                    </a>
                  </p>
                </div>
              </div>
              <div class="store-actions">
                <mat-slide-toggle 
                  [checked]="store.isActive" 
                  (change)="toggleStoreStatus(store.id, $event.checked)"
                  matTooltip="Toggle store visibility"
                >
                  {{ store.isActive ? 'Active' : 'Inactive' }}
                </mat-slide-toggle>
                <button mat-icon-button (click)="toggleFeatured(store.id, !store.isFeatured)" 
                        [matTooltip]="store.isFeatured ? 'Remove from featured' : 'Mark as featured'">
                  <mat-icon [class.featured]="store.isFeatured">star</mat-icon>
                </button>
                <button mat-icon-button (click)="openEditDialog(store)">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button color="warn" (click)="deleteStore(store.id)">
                  <mat-icon>delete</mat-icon>
                </button>
              </div>
            </div>
            
            <div class="store-description" *ngIf="store.description">
              <p>{{ store.description }}</p>
            </div>

            <div class="store-footer">
              <div class="store-meta">
                <span class="created-date">
                  <mat-icon>schedule</mat-icon>
                  Added {{ store.createdAt | date:'MMM d, y' }}
                </span>
                <span class="display-order" *ngIf="store.displayOrder > 0">
                  <mat-icon>sort</mat-icon>
                  Position {{ store.displayOrder + 1 }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <ng-template #loading>
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading merchandise stores...</p>
        </div>
      </ng-template>
    </div>

    <!-- Create/Edit Store Dialog -->
    <div class="store-dialog" *ngIf="showDialog">
      <div class="dialog-overlay" (click)="closeDialog()"></div>
      <div class="dialog-content">
        <div class="dialog-header">
          <h2>{{ editingStore ? 'Edit Store' : 'Add New Store' }}</h2>
          <button mat-icon-button (click)="closeDialog()">
            <mat-icon>close</mat-icon>
          </button>
        </div>
        
        <div class="dialog-body">
          <form [formGroup]="storeForm" (ngSubmit)="saveStore()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Store Name</mat-label>
              <input matInput formControlName="storeName" placeholder="Enter store name">
              <mat-error *ngIf="storeForm.get('storeName')?.hasError('required')">
                Store name is required
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Platform</mat-label>
              <mat-select formControlName="platform">
                <mat-option *ngFor="let option of platformOptions" [value]="option.value">
                  <mat-icon>{{ option.icon }}</mat-icon>
                  {{ option.label }}
                </mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Store URL</mat-label>
              <input matInput formControlName="storeUrl" placeholder="https://yourstore.com" type="url">
              <mat-error *ngIf="storeForm.get('storeUrl')?.hasError('required')">
                Store URL is required
              </mat-error>
              <mat-error *ngIf="storeForm.get('storeUrl')?.hasError('pattern')">
                Please enter a valid URL
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Description (Optional)</mat-label>
              <textarea matInput formControlName="description" rows="3" placeholder="Brief description of your store"></textarea>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Logo URL (Optional)</mat-label>
              <input matInput formControlName="logoUrl" placeholder="https://example.com/logo.png" type="url">
            </mat-form-field>

            <div class="form-options">
              <mat-slide-toggle formControlName="isActive">
                Store is active
              </mat-slide-toggle>
              <mat-slide-toggle formControlName="isFeatured">
                Featured store
              </mat-slide-toggle>
            </div>

            <div class="form-actions">
              <button mat-raised-button color="primary" type="submit" [disabled]="storeForm.invalid || isSaving">
                <mat-icon *ngIf="isSaving">hourglass_empty</mat-icon>
                <mat-icon *ngIf="!isSaving">save</mat-icon>
                {{ isSaving ? 'Saving...' : (editingStore ? 'Update Store' : 'Create Store') }}
              </button>
              <button mat-button type="button" (click)="closeDialog()">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .merchandise-management-container {
      max-width: 1000px;
      margin: 0 auto;
      padding: 20px;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
    }

    .header h2 {
      margin: 0;
      color: #333;
    }

    .stores-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .store-card {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      padding: 20px;
      transition: all 0.2s ease;
      cursor: move;
    }

    .store-card:hover {
      box-shadow: 0 4px 8px rgba(0,0,0,0.15);
    }

    .store-card.dragging {
      opacity: 0.5;
      transform: rotate(5deg);
    }

    .store-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 16px;
    }

    .store-info {
      display: flex;
      gap: 16px;
      flex: 1;
    }

    .store-logo {
      width: 48px;
      height: 48px;
      border-radius: 8px;
      overflow: hidden;
      background: #f5f5f5;
    }

    .store-logo img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .store-details h3 {
      margin: 0 0 4px 0;
      color: #333;
      font-size: 1.1rem;
    }

    .store-details .platform {
      margin: 0 0 8px 0;
      color: #666;
      font-size: 0.9rem;
    }

    .store-details .url {
      margin: 0;
      color: #1976d2;
      font-size: 0.9rem;
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .store-details .url a {
      color: inherit;
      text-decoration: none;
    }

    .store-details .url a:hover {
      text-decoration: underline;
    }

    .store-actions {
      display: flex;
      gap: 8px;
      align-items: center;
    }

    .store-actions mat-icon.featured {
      color: #ff9800;
    }

    .store-description {
      margin-bottom: 16px;
      color: #666;
      line-height: 1.5;
    }

    .store-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: 16px;
      border-top: 1px solid #eee;
    }

    .store-meta {
      display: flex;
      gap: 16px;
      color: #999;
      font-size: 0.9rem;
    }

    .store-meta span {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .empty-state {
      text-align: center;
      padding: 60px 20px;
    }

    .empty-state mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #ccc;
      margin-bottom: 20px;
    }

    .empty-state h3 {
      color: #666;
      margin-bottom: 10px;
    }

    .empty-state p {
      color: #999;
      margin-bottom: 30px;
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
    }

    .loading p {
      margin-top: 20px;
      color: #666;
    }

    /* Dialog Styles */
    .store-dialog {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      z-index: 1000;
    }

    .dialog-overlay {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0,0,0,0.5);
    }

    .dialog-content {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: white;
      border-radius: 8px;
      width: 90%;
      max-width: 500px;
      max-height: 90vh;
      overflow-y: auto;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px;
      border-bottom: 1px solid #eee;
    }

    .dialog-header h2 {
      margin: 0;
      color: #333;
    }

    .dialog-body {
      padding: 20px;
    }

    .form-options {
      display: flex;
      gap: 24px;
      margin: 20px 0;
    }

    .form-actions {
      display: flex;
      gap: 16px;
      justify-content: flex-end;
      margin-top: 20px;
      padding-top: 20px;
      border-top: 1px solid #eee;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    @media (max-width: 768px) {
      .store-header {
        flex-direction: column;
        gap: 16px;
      }

      .store-actions {
        align-self: flex-end;
      }

      .dialog-content {
        width: 95%;
        margin: 20px;
      }

      .form-options {
        flex-direction: column;
        gap: 16px;
      }

      .form-actions {
        flex-direction: column;
      }
    }
  `]
})
export class MerchandiseManagementComponent implements OnInit, OnDestroy {
  stores: MerchandiseStore[] = [];
  isLoading = false;
  isSaving = false;
  showDialog = false;
  editingStore: MerchandiseStore | null = null;
  isDragging = false;
  
  storeForm!: FormGroup;
  platformOptions = STORE_PLATFORM_OPTIONS;
  
  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private merchandiseService: MerchandiseService,
    private snackBar: MatSnackBar
  ) {
    this.storeForm = this.fb.group({
      storeName: ['', [Validators.required, Validators.maxLength(100)]],
      platform: [StorePlatform.Other, Validators.required],
      storeUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
      description: ['', [Validators.maxLength(500)]],
      logoUrl: ['', [Validators.pattern(/^https?:\/\/.+/)]],
      isActive: [true],
      isFeatured: [false]
    });
  }

  ngOnInit() {
    this.loadStores();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  loadStores() {
    this.isLoading = true;
    // Note: In a real app, you'd get the channelId from route params or service
    const channelId = 1; // This should come from the current channel context
    
    this.subscriptions.add(
      this.merchandiseService.getChannelStores(channelId).subscribe({
        next: (stores) => {
          this.stores = stores;
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.snackBar.open('Failed to load merchandise stores', 'Close', { duration: 3000 });
        }
      })
    );
  }

  openCreateDialog() {
    this.editingStore = null;
    this.storeForm.reset({
      platform: StorePlatform.Other,
      isActive: true,
      isFeatured: false
    });
    this.showDialog = true;
  }

  openEditDialog(store: MerchandiseStore) {
    this.editingStore = store;
    this.storeForm.patchValue({
      storeName: store.storeName,
      platform: store.platform,
      storeUrl: store.storeUrl,
      description: store.description || '',
      logoUrl: store.logoUrl || '',
      isActive: store.isActive,
      isFeatured: store.isFeatured
    });
    this.showDialog = true;
  }

  closeDialog() {
    this.showDialog = false;
    this.editingStore = null;
    this.storeForm.reset();
  }

  saveStore() {
    if (this.storeForm.invalid) return;

    this.isSaving = true;
    const formValue = this.storeForm.value;
    const channelId = 1; // This should come from the current channel context

    if (this.editingStore) {
      const updateData: MerchandiseStoreUpdate = {
        storeName: formValue.storeName,
        platform: formValue.platform,
        storeUrl: formValue.storeUrl,
        description: formValue.description,
        logoUrl: formValue.logoUrl,
        isActive: formValue.isActive,
        isFeatured: formValue.isFeatured
      };

      this.subscriptions.add(
        this.merchandiseService.updateStore(this.editingStore.id, updateData).subscribe({
          next: (updatedStore) => {
            const index = this.stores.findIndex(s => s.id === updatedStore.id);
            if (index !== -1) {
              this.stores[index] = updatedStore;
            }
            this.isSaving = false;
            this.snackBar.open('Store updated successfully', 'Close', { duration: 3000 });
            this.closeDialog();
          },
          error: (error) => {
            this.isSaving = false;
            this.snackBar.open('Failed to update store', 'Close', { duration: 3000 });
          }
        })
      );
    } else {
      const createData: MerchandiseStoreCreate = {
        storeName: formValue.storeName,
        platform: formValue.platform,
        storeUrl: formValue.storeUrl,
        description: formValue.description,
        logoUrl: formValue.logoUrl,
        isActive: formValue.isActive,
        isFeatured: formValue.isFeatured
      };

      this.subscriptions.add(
        this.merchandiseService.createStore(channelId, createData).subscribe({
          next: (newStore) => {
            this.stores.push(newStore);
            this.isSaving = false;
            this.snackBar.open('Store created successfully', 'Close', { duration: 3000 });
            this.closeDialog();
          },
          error: (error) => {
            this.isSaving = false;
            this.snackBar.open('Failed to create store', 'Close', { duration: 3000 });
          }
        })
      );
    }
  }

  deleteStore(storeId: number) {
    if (confirm('Are you sure you want to delete this store? This action cannot be undone.')) {
      this.subscriptions.add(
        this.merchandiseService.deleteStore(storeId).subscribe({
          next: () => {
            this.stores = this.stores.filter(s => s.id !== storeId);
            this.snackBar.open('Store deleted successfully', 'Close', { duration: 3000 });
          },
          error: (error) => {
            this.snackBar.open('Failed to delete store', 'Close', { duration: 3000 });
          }
        })
      );
    }
  }

  toggleStoreStatus(storeId: number, isActive: boolean) {
    this.subscriptions.add(
      this.merchandiseService.toggleStoreStatus(storeId, isActive).subscribe({
        next: () => {
          const store = this.stores.find(s => s.id === storeId);
          if (store) {
            store.isActive = isActive;
          }
          this.snackBar.open(`Store ${isActive ? 'activated' : 'deactivated'}`, 'Close', { duration: 2000 });
        },
        error: (error) => {
          this.snackBar.open('Failed to update store status', 'Close', { duration: 3000 });
        }
      })
    );
  }

  toggleFeatured(storeId: number, isFeatured: boolean) {
    this.subscriptions.add(
      this.merchandiseService.toggleStoreFeatured(storeId, isFeatured).subscribe({
        next: () => {
          const store = this.stores.find(s => s.id === storeId);
          if (store) {
            store.isFeatured = isFeatured;
          }
          this.snackBar.open(`Store ${isFeatured ? 'featured' : 'unfeatured'}`, 'Close', { duration: 2000 });
        },
        error: (error) => {
          this.snackBar.open('Failed to update store featured status', 'Close', { duration: 3000 });
        }
      })
    );
  }

  getPlatformLabel(platform: StorePlatform): string {
    const option = this.platformOptions.find(opt => opt.value === platform);
    return option ? option.label : platform;
  }

  onDragStarted() {
    this.isDragging = true;
  }

  onDragEnded() {
    this.isDragging = false;
  }

  onDrop(event: CdkDragDrop<MerchandiseStore[]>) {
    if (event.previousIndex !== event.currentIndex) {
      moveItemInArray(this.stores, event.previousIndex, event.currentIndex);
      
      // Update display order on server
      const storeIds = this.stores.map(s => s.id);
      const channelId = 1; // This should come from the current channel context
      
      this.subscriptions.add(
        this.merchandiseService.reorderStores(channelId, storeIds).subscribe({
          next: () => {
            this.snackBar.open('Store order updated', 'Close', { duration: 2000 });
          },
          error: (error) => {
            this.snackBar.open('Failed to update store order', 'Close', { duration: 3000 });
          }
        })
      );
    }
  }
}
