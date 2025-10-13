import { createAction, props } from '@ngrx/store';
import { Comment } from '../../models/comment.model';

export const loadComments = createAction(
  '[Comment] Load Comments',
  props<{ videoId: number; page?: number; pageSize?: number }>()
);

export const loadCommentsSuccess = createAction(
  '[Comment] Load Comments Success',
  props<{ comments: Comment[]; totalCount: number; page: number; pageSize: number }>()
);

export const loadCommentsFailure = createAction(
  '[Comment] Load Comments Failure',
  props<{ error: string }>()
);
