import { User } from './user.model';

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
  updatedAt?: string;
  plan?: SubscriptionPlan;
  user?: User;
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

export enum SubscriptionStatus {
  Active = 'Active',
  Expired = 'Expired',
  Cancelled = 'Cancelled',
  Suspended = 'Suspended'
}

export enum BillingCycle {
  Monthly = 'Monthly',
  Yearly = 'Yearly'
}
