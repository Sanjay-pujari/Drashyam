export interface Video {
  id: number;
  title: string;
  description?: string;
  videoUrl: string;
  thumbnailUrl?: string;
  userId: string;
  channelId?: number;
  status: VideoStatus;
  type: VideoType;
  visibility: VideoVisibility;
  createdAt: string;
  publishedAt?: string;
  viewCount: number;
  likeCount: number;
  dislikeCount: number;
  commentCount: number;
  duration: string;
  fileSize: number;
  tags?: string;
  category?: string;
  isMonetized: boolean;
  revenue?: number;
  shareToken?: string;
  user?: User;
  channel?: Channel;
  isLiked?: boolean;
  isDisliked?: boolean;
}

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

export interface Channel {
  id: number;
  name: string;
  description?: string;
  userId: string;
  bannerUrl?: string;
  profilePictureUrl?: string;
  createdAt: string;
  subscriberCount: number;
  videoCount: number;
  totalViews: number;
  isVerified: boolean;
  isMonetized: boolean;
  type: ChannelType;
  maxVideos: number;
  currentVideoCount: number;
  customUrl?: string;
  websiteUrl?: string;
  socialLinks?: string;
  user?: User;
  isSubscribed: boolean;
}

export enum VideoStatus {
  Processing = 'Processing',
  Ready = 'Ready',
  Failed = 'Failed',
  Deleted = 'Deleted'
}

export enum VideoType {
  Uploaded = 'Uploaded',
  LiveStream = 'LiveStream',
  Short = 'Short'
}

export enum VideoVisibility {
  Public = 'Public',
  Unlisted = 'Unlisted',
  Private = 'Private'
}

export enum SubscriptionType {
  Free = 'Free',
  Premium = 'Premium',
  Pro = 'Pro'
}

export enum ChannelType {
  Personal = 'Personal',
  Business = 'Business',
  Brand = 'Brand'
}
