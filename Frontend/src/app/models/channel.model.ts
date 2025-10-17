import { User } from './user.model';

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
  websiteUrl?: string;
  socialLinks?: string;
}

export enum ChannelType {
  Personal = 0,
  Business = 1,
  Brand = 2
}
