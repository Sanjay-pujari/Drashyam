export interface VideoNotification {
  id: number;
  userId: string;
  videoId: number;
  videoTitle: string;
  type: NotificationType;
  message: string;
  isRead: boolean;
  createdAt: Date;
  timestamp: Date;
}

export enum NotificationType {
  Like = 'Like',
  Comment = 'Comment',
  Share = 'Share',
  Subscribe = 'Subscribe',
  Upload = 'Upload',
  System = 'System'
}
