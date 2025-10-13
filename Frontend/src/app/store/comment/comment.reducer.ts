import { createReducer, on } from '@ngrx/store';
import { CommentState, initialCommentState } from './comment.state';
import * as CommentActions from './comment.actions';

export const commentReducer = createReducer(
  initialCommentState,
  
  on(CommentActions.loadComments, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(CommentActions.loadCommentsSuccess, (state, { comments, totalCount, page, pageSize }) => ({
    ...state,
    currentVideoComments: comments,
    totalCount,
    currentPage: page,
    pageSize,
    loading: false,
    error: null
  })),
  
  on(CommentActions.loadCommentsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
