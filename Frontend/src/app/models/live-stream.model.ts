import { User } from './user.model';
import { Channel } from './channel.model';

export interface LiveStream {
  id: number;
  title: string;
  description?: string;
  userId: string;
  channelId?: number;
  streamKey: string;
  streamUrl: string;
  status: LiveStreamStatus;
  scheduledStartTime: string;
  actualStartTime?: string;
  endTime?: string;
  viewerCount: number;
  maxViewers: number;
  thumbnailUrl?: string;
  isMonetized: boolean;
  category?: string;
  tags?: string;
  createdAt: string;
  user?: User;
  channel?: Channel;
}

export interface LiveStreamCreate {
  title: string;
  description?: string;
  channelId?: number;
  scheduledStartTime: string;
  category?: string;
  tags?: string;
  isMonetized: boolean;
}

export interface LiveStreamUpdate {
  title?: string;
  description?: string;
  scheduledStartTime?: string;
  category?: string;
  tags?: string;
  isMonetized?: boolean;
}

export enum LiveStreamStatus {
  Scheduled = 'Scheduled',
  Live = 'Live',
  Ended = 'Ended',
  Cancelled = 'Cancelled'
}
