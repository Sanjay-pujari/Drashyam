import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Subscription {
  id: number;
  userId: string;
  subscriptionPlanId: number;
  startDate: string;
  endDate: string;
  status: SubscriptionStatus;
  amount: number;
  paymentMethodId?: string;
  createdAt: string;
  plan?: SubscriptionPlan;
  user?: any;
  autoRenew?: boolean;
}

export interface SubscriptionPlan {
  id: number;
  name: string;
  description?: string;
  price: number;
  billingCycle: BillingCycle;
  maxChannels: number;
  maxVideosPerChannel: number;
  maxStorageGB: number;
  hasAds: boolean;
  hasAnalytics: boolean;
  hasMonetization: boolean;
  hasLiveStreaming: boolean;
  isActive: boolean;
}

export interface SubscriptionCreate {
  subscriptionPlanId: number;
  paymentMethodId: string;
}

export interface SubscriptionUpdate {
  subscriptionPlanId?: number;
  paymentMethodId?: string;
}

export interface PaymentResult {
  success: boolean;
  paymentIntentId: string;
  clientSecret?: string;
  amount: number;
  currency: string;
  status: string;
}

export enum SubscriptionStatus {
  Active = 0,
  Expired = 1,
  Cancelled = 2,
  Suspended = 3
}

export enum BillingCycle {
  Monthly = 0,
  Yearly = 1
}

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private apiUrl = `${environment.apiUrl}/api/subscription`;

  constructor(private http: HttpClient) {}

  // Subscription Management
  createSubscription(subscription: SubscriptionCreate): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}`, subscription);
  }

  getSubscription(id: number): Observable<Subscription> {
    return this.http.get<Subscription>(`${this.apiUrl}/${id}`);
  }

  getUserSubscription(): Observable<Subscription> {
    return this.http.get<Subscription>(`${this.apiUrl}/user`);
  }

  updateSubscription(id: number, subscription: SubscriptionUpdate): Observable<Subscription> {
    return this.http.put<Subscription>(`${this.apiUrl}/${id}`, subscription);
  }

  cancelSubscription(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/cancel`, {});
  }

  renewSubscription(id: number): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}/${id}/renew`, {});
  }

  // Subscription Plans
  getSubscriptionPlans(page: number = 1, pageSize: number = 20): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/plans?page=${page}&pageSize=${pageSize}`);
  }

  getSubscriptionPlan(id: number): Observable<SubscriptionPlan> {
    return this.http.get<SubscriptionPlan>(`${this.apiUrl}/plans/${id}`);
  }

  // Subscription Changes
  upgradeSubscription(newPlanId: number): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}/upgrade`, { newPlanId });
  }

  downgradeSubscription(newPlanId: number): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}/downgrade`, { newPlanId });
  }

  // Status Checks
  checkSubscriptionStatus(): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/status`);
  }

  // Payment Processing
  processPayment(paymentData: any): Observable<PaymentResult> {
    return this.http.post<PaymentResult>(`${this.apiUrl}/payment`, paymentData);
  }

  createPaymentIntent(amount: number, currency: string = 'USD'): Observable<PaymentResult> {
    return this.http.post<PaymentResult>(`${this.apiUrl}/payment-intent`, {
      amount,
      currency
    });
  }

  // Analytics
  getExpiringSubscriptions(daysAhead: number = 7, page: number = 1, pageSize: number = 20): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/expiring?daysAhead=${daysAhead}&page=${page}&pageSize=${pageSize}`);
  }

  // Channel Subscriptions
  getSubscribedChannels(options?: { page?: number; pageSize?: number }): Observable<any> {
    const params = new URLSearchParams();
    if (options?.page) params.append('page', options.page.toString());
    if (options?.pageSize) params.append('pageSize', options.pageSize.toString());
    
    const queryString = params.toString();
    const url = queryString ? `${this.apiUrl}/channels?${queryString}` : `${this.apiUrl}/channels`;
    return this.http.get<any>(url);
  }

  unsubscribeFromChannel(channelId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/channels/${channelId}/unsubscribe`, {});
  }

  getNotificationPreference(channelId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/channels/${channelId}/notifications`);
  }
}
