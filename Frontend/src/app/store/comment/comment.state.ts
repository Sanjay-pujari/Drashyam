import { Comment } from '../../models/comment.model';

export interface CommentState {
  comments: Comment[];
  currentVideoComments: Comment[];
  loading: boolean;
  error: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

export const initialCommentState: CommentState = {
  comments: [],
  currentVideoComments: [],
  loading: false,
  error: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 20
};
