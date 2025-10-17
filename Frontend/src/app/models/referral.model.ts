export interface Referral {
  id: number;
  referrerId: string;
  referrerName: string;
  referredUserId: string;
  referredUserName: string;
  createdAt: string;
  rewardedAt?: string;
  status: ReferralStatus;
  referralCode?: string;
  rewardAmount?: number;
  rewardType?: string;
  referralPoints?: number;
}

export interface CreateReferral {
  referredUserId: string;
  referralCode?: string;
}

export interface ReferralStats {
  totalReferrals: number;
  completedReferrals: number;
  pendingReferrals: number;
  totalRewards: number;
  pendingRewards: number;
  conversionRate: number;
}

export interface ReferralReward {
  id: number;
  userId: string;
  userName: string;
  referralId: string;
  amount: number;
  rewardType: string;
  createdAt: string;
  claimedAt?: string;
  expiresAt?: string;
  status: RewardStatus;
  description?: string;
}

export interface ClaimReward {
  rewardId: number;
}

export interface ReferralCode {
  code: string;
  createdAt: string;
  expiresAt?: string;
  usageCount: number;
  maxUsage: number;
  isActive: boolean;
}

export interface CreateReferralCode {
  code?: string;
  maxUsage?: number;
  expiresAt?: string;
  rewardAmount?: number;
  rewardType?: string;
}

export enum ReferralStatus {
  Pending = 'Pending',
  Completed = 'Completed',
  Rewarded = 'Rewarded',
  Cancelled = 'Cancelled'
}

export enum RewardStatus {
  Pending = 'Pending',
  Claimed = 'Claimed',
  Expired = 'Expired',
  Cancelled = 'Cancelled'
}
