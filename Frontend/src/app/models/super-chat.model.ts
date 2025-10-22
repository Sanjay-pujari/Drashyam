export interface SuperChat {
  id: number;
  liveStreamId: number;
  donorName: string;
  donorMessage?: string;
  amount: number;
  currency: string;
  status: string;
  createdAt: string;
  processedAt?: string;
  paymentIntentId?: string;
  displayDuration: number;
  donorAvatar?: string;
  isAnonymous: boolean;
}

export interface SuperChatRequest {
  liveStreamId: number;
  donorName: string;
  donorMessage?: string;
  amount: number;
  currency: string;
  paymentMethodId: string;
  isAnonymous: boolean;
}

export interface SuperChatDisplay {
  id: number;
  donorName: string;
  donorMessage?: string;
  amount: number;
  currency: string;
  createdAt: string;
  donorAvatar?: string;
  isAnonymous: boolean;
  displayDuration: number;
}

export interface SuperChatTier {
  amount: number;
  color: string;
  duration: number;
  label: string;
}

export const SUPER_CHAT_TIERS: SuperChatTier[] = [
  { amount: 1, color: '#4CAF50', duration: 2, label: 'Green' },
  { amount: 5, color: '#2196F3', duration: 3, label: 'Blue' },
  { amount: 10, color: '#FF9800', duration: 4, label: 'Orange' },
  { amount: 25, color: '#9C27B0', duration: 6, label: 'Purple' },
  { amount: 50, color: '#F44336', duration: 8, label: 'Red' },
  { amount: 100, color: '#FFD700', duration: 10, label: 'Gold' }
];
