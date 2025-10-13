import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { CommentService } from '../../services/comment.service';
import * as CommentActions from './comment.actions';

@Injectable()
export class CommentEffects {
  constructor(
    private actions$: Actions,
    private commentService: CommentService
  ) {}

  loadComments$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CommentActions.loadComments),
      switchMap(({ videoId, page = 1, pageSize = 20 }) =>
        this.commentService.getVideoComments(videoId, { page, pageSize }).pipe(
          map(response => CommentActions.loadCommentsSuccess({
            comments: response.items,
            totalCount: response.totalCount,
            page: response.page,
            pageSize: response.pageSize
          })),
          catchError(error => of(CommentActions.loadCommentsFailure({ error: error.message })))
        )
      )
    )
  );
}
