export interface MerchandiseItem {
  id: number;
  channelId: number;
  channelName: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  imageUrl: string;
  stockQuantity: number;
  isActive: boolean;
  category: string;
  sizes: string[];
  colors: string[];
  createdAt: Date;
  updatedAt?: Date;
}

export interface MerchandiseItemCreate {
  channelId: number;
  name: string;
  description: string;
  price: number;
  currency: string;
  stockQuantity: number;
  isActive: boolean;
  category: string;
  sizes: string[];
  colors: string[];
}

export interface MerchandiseItemUpdate {
  name: string;
  description: string;
  price: number;
  currency: string;
  stockQuantity: number;
  isActive: boolean;
  category: string;
  sizes: string[];
  colors: string[];
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
  status: MerchandiseOrderStatus;
  orderedAt: Date;
  shippedAt?: Date;
  deliveredAt?: Date;
  trackingNumber?: string;
}

export interface MerchandiseOrderCreate {
  merchandiseItemId: number;
  customerName: string;
  customerEmail?: string;
  customerAddress?: string;
  quantity: number;
  size?: string;
  color?: string;
  paymentMethodId: string;
}

export interface MerchandiseOrderUpdate {
  status: MerchandiseOrderStatus;
  trackingNumber?: string;
}

export enum MerchandiseOrderStatus {
  Pending = 0,
  Confirmed = 1,
  Processing = 2,
  Shipped = 3,
  Delivered = 4,
  Cancelled = 5,
  Refunded = 6
}

export interface MerchandiseAnalytics {
  totalItems: number;
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  topSellingItems: Array<{
    itemId: number;
    itemName: string;
    totalSold: number;
    revenue: number;
  }>;
  ordersByStatus: Array<{
    status: MerchandiseOrderStatus;
    count: number;
  }>;
  revenueByMonth: Array<{
    month: string;
    revenue: number;
  }>;
}

export interface MerchandiseFilter {
  category?: string;
  isActive?: boolean;
  minPrice?: number;
  maxPrice?: number;
  search?: string;
}

export interface MerchandiseOrderFilter {
  status?: MerchandiseOrderStatus;
  startDate?: Date;
  endDate?: Date;
  search?: string;
}