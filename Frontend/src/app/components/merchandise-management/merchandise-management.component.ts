import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatChipInputEvent } from '@angular/material/chips';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MerchandiseService } from '../../services/merchandise.service';
import { ChannelService } from '../../services/channel.service';
import { UserService } from '../../services/user.service';
import {
  MerchandiseItem,
  MerchandiseItemCreate,
  MerchandiseItemUpdate,
  MerchandiseOrder,
  MerchandiseOrderCreate,
  MerchandiseOrderUpdate,
  MerchandiseOrderStatus,
  MerchandiseAnalytics,
  MerchandiseFilter
} from '../../models/merchandise.model';
import { Channel } from '../../models/channel.model';
import { PagedResult } from '../../models/paged-result.model';

@Component({
  selector: 'app-merchandise-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatTableModule,
    MatPaginatorModule,
    MatDialogModule,
    MatSnackBarModule,
    MatChipsModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './merchandise-management.component.html',
  styleUrls: ['./merchandise-management.component.scss']
})
export class MerchandiseManagementComponent implements OnInit {
  loading = false;
  MerchandiseOrderStatus = MerchandiseOrderStatus;

  // Forms
  merchandiseForm: FormGroup;
  orderForm: FormGroup;

  // Data
  merchandiseItems: MerchandiseItem[] = [];
  orders: MerchandiseOrder[] = [];
  analytics: MerchandiseAnalytics | null = null;
  channels: Channel[] = [];
  currentUserId: string = '';

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  // Table columns
  displayedColumns: string[] = ['name', 'category', 'price', 'stockQuantity', 'isActive', 'createdAt', 'actions'];
  orderColumns: string[] = ['merchandiseName', 'customerName', 'amount', 'quantity', 'status', 'orderedAt', 'actions'];

  // Filter options
  categories = ['Clothing', 'Accessories', 'Electronics', 'Books', 'Home & Garden', 'Sports', 'Other'];
  currencies = ['USD', 'EUR', 'GBP', 'CAD', 'AUD'];

  constructor(
    private fb: FormBuilder,
    private merchandiseService: MerchandiseService,
    private channelService: ChannelService,
    private userService: UserService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.merchandiseForm = this.fb.group({
      channelId: ['', Validators.required],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(1000)],
      price: ['', [Validators.required, Validators.min(0.01)]],
      currency: ['USD', Validators.required],
      stockQuantity: ['', [Validators.required, Validators.min(0)]],
      isActive: [true],
      category: ['', Validators.required],
      sizes: [[]],
      colors: [[]]
    });

    this.orderForm = this.fb.group({
      merchandiseItemId: ['', Validators.required],
      customerName: ['', [Validators.required, Validators.maxLength(200)]],
      customerEmail: ['', [Validators.email, Validators.maxLength(200)]],
      customerAddress: ['', Validators.maxLength(500)],
      quantity: ['', [Validators.required, Validators.min(1)]],
      size: [''],
      color: [''],
      paymentMethodId: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.getCurrentUser();
    this.loadMerchandiseItems();
    this.loadOrders();
    this.loadAnalytics();
  }

  getCurrentUser(): void {
    this.userService.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUserId = user.id;
        this.loadChannels();
      },
      error: (error) => {
        console.error('Error getting current user:', error);
        this.snackBar.open('Error loading user data', 'Close', { duration: 3000 });
      }
    });
  }

  loadChannels(): void {
    if (this.currentUserId) {
      this.channelService.getUserChannels(this.currentUserId).subscribe({
        next: (channels) => {
          this.channels = channels;
        },
        error: (error) => {
          console.error('Error loading channels:', error);
        }
      });
    }
  }

  loadMerchandiseItems(): void {
    this.loading = true;
    this.merchandiseService.getMerchandiseItems().subscribe({
      next: (items) => {
        this.merchandiseItems = items;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading merchandise items:', error);
        this.snackBar.open('Error loading merchandise items', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  loadOrders(): void {
    this.merchandiseService.getMerchandiseOrders(this.currentPage, this.pageSize).subscribe({
      next: (result: PagedResult<MerchandiseOrder>) => {
        this.orders = result.items;
        this.totalCount = result.totalCount;
      },
      error: (error) => {
        console.error('Error loading orders:', error);
        this.snackBar.open('Error loading orders', 'Close', { duration: 3000 });
      }
    });
  }

  loadAnalytics(): void {
    this.merchandiseService.getMerchandiseAnalytics().subscribe({
      next: (analytics) => {
        this.analytics = analytics;
      },
      error: (error) => {
        console.error('Error loading analytics:', error);
      }
    });
  }

  onSubmitMerchandise(): void {
    if (this.merchandiseForm.valid) {
      const formValue = this.merchandiseForm.value;
      const merchandiseData: MerchandiseItemCreate = {
        channelId: formValue.channelId,
        name: formValue.name,
        description: formValue.description,
        price: formValue.price,
        currency: formValue.currency,
        stockQuantity: formValue.stockQuantity,
        isActive: formValue.isActive,
        category: formValue.category,
        sizes: formValue.sizes || [],
        colors: formValue.colors || []
      };

      this.merchandiseService.createMerchandiseItem(merchandiseData).subscribe({
        next: (item) => {
          this.snackBar.open('Merchandise item created successfully', 'Close', { duration: 3000 });
          this.merchandiseForm.reset();
          this.loadMerchandiseItems();
        },
        error: (error) => {
          console.error('Error creating merchandise item:', error);
          this.snackBar.open('Error creating merchandise item', 'Close', { duration: 3000 });
        }
      });
    }
  }

  onSubmitOrder(): void {
    if (this.orderForm.valid) {
      const formValue = this.orderForm.value;
      const orderData: MerchandiseOrderCreate = {
        merchandiseItemId: formValue.merchandiseItemId,
        customerName: formValue.customerName,
        customerEmail: formValue.customerEmail,
        customerAddress: formValue.customerAddress,
        quantity: formValue.quantity,
        size: formValue.size,
        color: formValue.color,
        paymentMethodId: formValue.paymentMethodId
      };

      this.merchandiseService.createMerchandiseOrder(orderData).subscribe({
        next: (order) => {
          this.snackBar.open('Order created successfully', 'Close', { duration: 3000 });
          this.orderForm.reset();
          this.loadOrders();
        },
        error: (error) => {
          console.error('Error creating order:', error);
          this.snackBar.open('Error creating order', 'Close', { duration: 3000 });
        }
      });
    }
  }

  updateOrderStatus(orderId: number, status: MerchandiseOrderStatus, trackingNumber?: string): void {
    const update: MerchandiseOrderUpdate = {
      status,
      trackingNumber
    };

    this.merchandiseService.updateMerchandiseOrderStatus(orderId, update).subscribe({
      next: (order) => {
        this.snackBar.open('Order status updated successfully', 'Close', { duration: 3000 });
        this.loadOrders();
      },
      error: (error) => {
        console.error('Error updating order status:', error);
        this.snackBar.open('Error updating order status', 'Close', { duration: 3000 });
      }
    });
  }

  deleteMerchandiseItem(id: number): void {
    if (confirm('Are you sure you want to delete this merchandise item?')) {
      this.merchandiseService.deleteMerchandiseItem(id).subscribe({
        next: () => {
          this.snackBar.open('Merchandise item deleted successfully', 'Close', { duration: 3000 });
          this.loadMerchandiseItems();
        },
        error: (error) => {
          console.error('Error deleting merchandise item:', error);
          this.snackBar.open('Error deleting merchandise item', 'Close', { duration: 3000 });
        }
      });
    }
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }

  // Utility methods
  getOrderStatusText(status: MerchandiseOrderStatus): string {
    return this.merchandiseService.getOrderStatusText(status);
  }

  getOrderStatusColor(status: MerchandiseOrderStatus): string {
    return this.merchandiseService.getOrderStatusColor(status);
  }

  formatCurrency(amount: number, currency: string = 'USD'): string {
    return this.merchandiseService.formatCurrency(amount, currency);
  }

  formatDate(date: Date | string): string {
    return this.merchandiseService.formatDate(date);
  }

  // Chip management for sizes and colors
  addSize(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value) {
      const sizes = this.merchandiseForm.get('sizes')?.value || [];
      sizes.push(value);
      this.merchandiseForm.get('sizes')?.setValue(sizes);
    }
    event.chipInput!.clear();
  }

  removeSize(size: string): void {
    const sizes = this.merchandiseForm.get('sizes')?.value || [];
    const index = sizes.indexOf(size);
    if (index >= 0) {
      sizes.splice(index, 1);
      this.merchandiseForm.get('sizes')?.setValue(sizes);
    }
  }

  addColor(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value) {
      const colors = this.merchandiseForm.get('colors')?.value || [];
      colors.push(value);
      this.merchandiseForm.get('colors')?.setValue(colors);
    }
    event.chipInput!.clear();
  }

  removeColor(color: string): void {
    const colors = this.merchandiseForm.get('colors')?.value || [];
    const index = colors.indexOf(color);
    if (index >= 0) {
      colors.splice(index, 1);
      this.merchandiseForm.get('colors')?.setValue(colors);
    }
  }
}