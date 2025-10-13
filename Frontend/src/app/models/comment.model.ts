import { User } from './user.model';

export interface Comment {
  id: number;
  content: string;
  videoId: number;
  userId: string;
  parentCommentId?: number;
  createdAt: string;
  updatedAt?: string;
  likeCount: number;
  replyCount: number;
  isEdited: boolean;
  user?: User;
  parentComment?: Comment;
  replies?: Comment[];
  isLiked: boolean;
}

export interface CommentCreate {
  content: string;
  videoId: number;
  parentCommentId?: number;
}

export interface CommentUpdate {
  content: string;
}
