export interface MerchandiseStore {
  id: number;
  channelId: number;
  storeName: string;
  platform: StorePlatform;
  storeUrl: string;
  description?: string;
  logoUrl?: string;
  isActive: boolean;
  isFeatured: boolean;
  displayOrder: number;
  createdAt: string;
  updatedAt?: string;
}

export interface MerchandiseStoreCreate {
  storeName: string;
  platform: StorePlatform;
  storeUrl: string;
  description?: string;
  logoUrl?: string;
  isActive?: boolean;
  isFeatured?: boolean;
  displayOrder?: number;
}

export interface MerchandiseStoreUpdate {
  storeName?: string;
  platform?: StorePlatform;
  storeUrl?: string;
  description?: string;
  logoUrl?: string;
  isActive?: boolean;
  isFeatured?: boolean;
  displayOrder?: number;
}

export enum StorePlatform {
  Shopify = 'Shopify',
  Etsy = 'Etsy',
  Amazon = 'Amazon',
  Teespring = 'Teespring',
  Redbubble = 'Redbubble',
  Spreadshirt = 'Spreadshirt',
  Zazzle = 'Zazzle',
  Custom = 'Custom',
  Other = 'Other'
}

export const STORE_PLATFORM_OPTIONS = [
  { value: StorePlatform.Shopify, label: 'Shopify', icon: 'store' },
  { value: StorePlatform.Etsy, label: 'Etsy', icon: 'shopping_bag' },
  { value: StorePlatform.Amazon, label: 'Amazon', icon: 'shopping_cart' },
  { value: StorePlatform.Teespring, label: 'Teespring', icon: 'shirt' },
  { value: StorePlatform.Redbubble, label: 'Redbubble', icon: 'palette' },
  { value: StorePlatform.Spreadshirt, label: 'Spreadshirt', icon: 'print' },
  { value: StorePlatform.Zazzle, label: 'Zazzle', icon: 'design_services' },
  { value: StorePlatform.Custom, label: 'Custom Store', icon: 'storefront' },
  { value: StorePlatform.Other, label: 'Other', icon: 'more_horiz' }
];
