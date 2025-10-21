export interface PremiumVideo {
  id: number;
  videoId: number;
  videoTitle: string;
  videoThumbnailUrl: string;
  creatorName: string;
  price: number;
  currency: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface PremiumVideoCreate {
  videoId: number;
  creatorId: string;
  price: number;
  currency: string;
}

export interface PremiumVideoUpdate {
  creatorId?: string;
  price?: number;
  currency?: string;
  isActive?: boolean;
}

export interface PremiumPurchase {
  id: number;
  premiumVideoId: number;
  userId: string;
  videoTitle: string;
  amount: number;
  currency: string;
  paymentIntentId: string;
  status: PremiumPurchaseStatus;
  purchasedAt: string;
  completedAt?: string;
  refundedAt?: string;
}

export interface PremiumPurchaseCreate {
  premiumVideoId: number;
  userId: string;
  paymentIntentId: string;
}

export interface PremiumContentAnalytics {
  premiumVideoId: number;
  totalPurchases: number;
  totalRevenue: number;
  averagePrice: number;
  startDate?: string;
  endDate?: string;
}

export enum PremiumPurchaseStatus {
  Pending = 'Pending',
  Completed = 'Completed',
  Failed = 'Failed',
  Refunded = 'Refunded',
  Cancelled = 'Cancelled'
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
