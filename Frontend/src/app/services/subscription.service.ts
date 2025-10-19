import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Subscription, SubscriptionPlan, SubscriptionCreate, SubscriptionUpdate } from '../models/subscription.model';
import { Channel } from '../models/channel.model';
import { PagedResult } from './video.service';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private apiUrl = `${environment.apiUrl}/api/subscriptions`;

  constructor(private http: HttpClient) {}

  getSubscriptionPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<SubscriptionPlan[]>(`${this.apiUrl}/plans`);
  }

  getSubscriptionPlanById(id: number): Observable<SubscriptionPlan> {
    return this.http.get<SubscriptionPlan>(`${this.apiUrl}/plans/${id}`);
  }

  getUserSubscription(): Observable<Subscription> {
    return this.http.get<Subscription>(`${this.apiUrl}/current`);
  }

  createSubscription(subscriptionData: SubscriptionCreate): Observable<Subscription> {
    return this.http.post<Subscription>(this.apiUrl, subscriptionData);
  }

  updateSubscription(id: number, subscriptionData: SubscriptionUpdate): Observable<Subscription> {
    return this.http.put<Subscription>(`${this.apiUrl}/${id}`, subscriptionData);
  }

  cancelSubscription(id: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/cancel`, {});
  }

  renewSubscription(id: number): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}/${id}/renew`, {});
  }

  upgradeSubscription(newPlanId: number): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}/upgrade`, { newPlanId });
  }

  downgradeSubscription(newPlanId: number): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}/downgrade`, { newPlanId });
  }

  checkSubscriptionStatus(): Observable<{ isActive: boolean; expiresAt?: string }> {
    return this.http.get<{ isActive: boolean; expiresAt?: string }>(`${this.apiUrl}/status`);
  }

  processPayment(paymentData: any): Observable<Subscription> {
    return this.http.post<Subscription>(`${this.apiUrl}/payment`, paymentData);
  }

  // Channel subscription methods
  getSubscribedChannels(filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Channel>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    // Use the channel service endpoint for subscribed channels
    return this.http.get<PagedResult<Channel>>(`${environment.apiUrl}/api/channel/subscribed`, { params });
  }

  subscribeToChannel(channelId: number): Observable<Channel> {
    return this.http.post<Channel>(`${environment.apiUrl}/api/channel/${channelId}/subscribe`, {});
  }

  unsubscribeFromChannel(channelId: number): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/api/channel/${channelId}/unsubscribe`, {});
  }

  isSubscribedToChannel(channelId: number): Observable<boolean> {
    return this.http.get<boolean>(`${environment.apiUrl}/api/channel/${channelId}/is-subscribed`);
  }
}
