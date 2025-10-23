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
  hlsUrl?: string;
  rtmpUrl?: string;
  status: LiveStreamStatus;
  scheduledStartTime: string;
  actualStartTime?: string;
  startTime?: Date;
  endTime?: string;
  viewerCount: number;
  maxViewers: number;
  peakViewerCount?: number;
  thumbnailUrl?: string;
  isMonetized: boolean;
  isRecording?: boolean;
  recordingUrl?: string;
  category?: string;
  tags?: string;
  isPublic?: boolean;
  createdAt: string;
  updatedAt?: string;
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
