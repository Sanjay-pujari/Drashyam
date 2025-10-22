export interface Video {
  id: number;
  title: string;
  description?: string;
  videoUrl: string;
  thumbnailUrl?: string;
  userId: string;
  userName?: string;
  userProfilePicture?: string;
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
  Draft = 0,
  Processing = 1,
  Published = 2,
  Private = 3,
  Unlisted = 4,
  Deleted = 5
}

export enum VideoType {
  Regular = 0,
  Short = 1,
  Live = 2,
  Premier = 3
}

export enum VideoVisibility {
  Public = 0,
  Private = 1,
  Unlisted = 2
}

export enum SubscriptionType {
  Free = 'Free',
  Premium = 'Premium',
  Pro = 'Pro'
}

export enum ChannelType {
  Personal = 0,
  Business = 1,
  Creator = 2,
  Brand = 3
}

export interface ChannelCreate {
  name: string;
  description?: string;
  type: ChannelType;
  customUrl?: string;
  websiteUrl?: string;
  socialLinks?: string;
}

export interface ChannelUpdate {
  name?: string;
  description?: string;
  type?: ChannelType;
  customUrl?: string;
  websiteUrl?: string;
  socialLinks?: string;
}