import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MerchandiseItem {
  id: number;
  name: string;
  description: string;
  price: number;
  currency: string;
  imageUrl?: string;
  isActive: boolean;
  channelId: number;
  createdAt: string;
  updatedAt: string;
}

export interface MerchandiseOrder {
  id: number;
  merchandiseItemId: number;
  merchandiseName: string;
  customerId: string;
  customerName: string;
  customerEmail?: string;
  customerAddress?: string;
  amount: number;
  currency: string;
  quantity: number;
  size?: string;
  color?: string;
  paymentIntentId: string;
  status: string;
  orderedAt: string;
  shippedAt?: string;
  deliveredAt?: string;
  trackingNumber?: string;
  notes?: string;
  totalAmount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface MerchandiseAnalytics {
  totalItems: number;
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  topSellingItems: Array<{
    itemId: number;
    itemName: string;
    salesCount: number;
    revenue: number;
  }>;
  recentOrders: MerchandiseOrder[];
}

@Injectable({
  providedIn: 'root'
})
export class MonetizationService {
  private apiUrl = `${environment.apiUrl}/api/monetization`;

  constructor(private http: HttpClient) { }

  // Merchandise Management
  getChannelMerchandise(channelId: number): Observable<MerchandiseItem[]> {
    return this.http.get<MerchandiseItem[]>(`${this.apiUrl}/channels/${channelId}/merchandise`);
  }

  getMerchandiseDetails(merchandiseId: number): Observable<MerchandiseItem> {
    return this.http.get<MerchandiseItem>(`${this.apiUrl}/merchandise/${merchandiseId}`);
  }

  getMerchandiseAnalytics(startDate?: Date, endDate?: Date): Observable<MerchandiseAnalytics> {
    let params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    
    return this.http.get<MerchandiseAnalytics>(`${this.apiUrl}/merchandise/analytics`, { params });
  }

  // Merchandise Orders
  getMerchandiseOrders(page: number = 1, pageSize: number = 20): Observable<{ items: MerchandiseOrder[], totalCount: number }> {
    return this.http.get<{ items: MerchandiseOrder[], totalCount: number }>(`${this.apiUrl}/orders`, {
      params: { page: page.toString(), pageSize: pageSize.toString() }
    });
  }

  getMerchandiseOrder(orderId: number): Observable<MerchandiseOrder> {
    return this.http.get<MerchandiseOrder>(`${this.apiUrl}/orders/${orderId}`);
  }

  updateMerchandiseOrder(orderId: number, update: Partial<MerchandiseOrder>): Observable<MerchandiseOrder> {
    return this.http.put<MerchandiseOrder>(`${this.apiUrl}/orders/${orderId}`, update);
  }
}
