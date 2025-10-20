export interface PremiumVideo {
  id: number;
  videoId: number;
  price: number;
  currency: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface PremiumVideoCreate {
  videoId: number;
  price: number;
  currency: string;
}

export interface PremiumVideoUpdate {
  price: number;
  currency: string;
}

