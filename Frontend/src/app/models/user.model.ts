export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  bio?: string;
  profilePictureUrl?: string;
  createdAt: string;
  lastLoginAt?: string;
  isActive: boolean;
  subscriptionType: SubscriptionType;
  subscriptionExpiresAt?: string;
  channelCount: number;
  videoCount: number;
  subscriberCount: number;
  followingCount: number;
}

export interface UserRegistration {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface UserLogin {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface UserUpdate {
  firstName?: string;
  lastName?: string;
  bio?: string;
}

export enum SubscriptionType {
  Free = 'Free',
  Premium = 'Premium',
  Pro = 'Pro'
}
