import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  MerchandiseItem, 
  MerchandiseItemCreate, 
  MerchandiseItemUpdate,
  MerchandiseOrder,
  MerchandiseOrderCreate,
  MerchandiseOrderUpdate,
  MerchandiseAnalytics,
  MerchandiseFilter,
  MerchandiseOrderFilter,
  MerchandiseOrderStatus
} from '../models/merchandise.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class MerchandiseService {
  private apiUrl = `${environment.apiUrl}/monetization`;

  constructor(private http: HttpClient) {}

  // Merchandise Items
  getMerchandiseItems(filter: MerchandiseFilter = {}): Observable<MerchandiseItem[]> {
    let params = new HttpParams();
    
    if (filter.category) params = params.set('category', filter.category);
    if (filter.isActive !== undefined) params = params.set('isActive', filter.isActive.toString());
    if (filter.minPrice !== undefined) params = params.set('minPrice', filter.minPrice.toString());
    if (filter.maxPrice !== undefined) params = params.set('maxPrice', filter.maxPrice.toString());
    if (filter.search) params = params.set('search', filter.search);

    return this.http.get<MerchandiseItem[]>(`${this.apiUrl}/merchandise`, { params });
  }

  getMerchandiseItem(id: number): Observable<MerchandiseItem> {
    return this.http.get<MerchandiseItem>(`${this.apiUrl}/merchandise/${id}`);
  }

  createMerchandiseItem(item: MerchandiseItemCreate): Observable<MerchandiseItem> {
    return this.http.post<MerchandiseItem>(`${this.apiUrl}/merchandise`, item);
  }

  updateMerchandiseItem(id: number, item: MerchandiseItemUpdate): Observable<MerchandiseItem> {
    return this.http.put<MerchandiseItem>(`${this.apiUrl}/merchandise/${id}`, item);
  }

  deleteMerchandiseItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/merchandise/${id}`);
  }

  // Merchandise Orders
  getMerchandiseOrders(page: number = 1, pageSize: number = 20): Observable<PagedResult<MerchandiseOrder>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<MerchandiseOrder>>(`${this.apiUrl}/merchandise/orders`, { params });
  }

  getMerchandiseOrder(id: number): Observable<MerchandiseOrder> {
    return this.http.get<MerchandiseOrder>(`${this.apiUrl}/merchandise/orders/${id}`);
  }

  createMerchandiseOrder(order: MerchandiseOrderCreate): Observable<MerchandiseOrder> {
    return this.http.post<MerchandiseOrder>(`${this.apiUrl}/merchandise/orders`, order);
  }

  updateMerchandiseOrderStatus(id: number, update: MerchandiseOrderUpdate): Observable<MerchandiseOrder> {
    return this.http.put<MerchandiseOrder>(`${this.apiUrl}/merchandise/orders/${id}/status`, update);
  }

  getCustomerOrders(page: number = 1, pageSize: number = 20): Observable<PagedResult<MerchandiseOrder>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<MerchandiseOrder>>(`${this.apiUrl}/merchandise/orders/customer`, { params });
  }

  // Analytics
  getMerchandiseAnalytics(): Observable<MerchandiseAnalytics> {
    return this.http.get<MerchandiseAnalytics>(`${this.apiUrl}/merchandise/analytics`);
  }

  // Utility methods
  getOrderStatusText(status: MerchandiseOrderStatus): string {
    switch (status) {
      case MerchandiseOrderStatus.Pending:
        return 'Pending';
      case MerchandiseOrderStatus.Confirmed:
        return 'Confirmed';
      case MerchandiseOrderStatus.Processing:
        return 'Processing';
      case MerchandiseOrderStatus.Shipped:
        return 'Shipped';
      case MerchandiseOrderStatus.Delivered:
        return 'Delivered';
      case MerchandiseOrderStatus.Cancelled:
        return 'Cancelled';
      case MerchandiseOrderStatus.Refunded:
        return 'Refunded';
      default:
        return 'Unknown';
    }
  }

  getOrderStatusColor(status: MerchandiseOrderStatus): string {
    switch (status) {
      case MerchandiseOrderStatus.Pending:
        return 'orange';
      case MerchandiseOrderStatus.Confirmed:
        return 'blue';
      case MerchandiseOrderStatus.Processing:
        return 'purple';
      case MerchandiseOrderStatus.Shipped:
        return 'green';
      case MerchandiseOrderStatus.Delivered:
        return 'green';
      case MerchandiseOrderStatus.Cancelled:
        return 'red';
      case MerchandiseOrderStatus.Refunded:
        return 'red';
      default:
        return 'gray';
    }
  }

  formatCurrency(amount: number, currency: string = 'USD'): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency
    }).format(amount);
  }

  formatDate(date: Date | string): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}