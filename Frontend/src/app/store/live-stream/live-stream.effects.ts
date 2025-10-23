import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { LiveStreamService } from '../../services/live-stream.service';
import * as LiveStreamActions from './live-stream.actions';

@Injectable()
export class LiveStreamEffects {
  constructor(
    private actions$: Actions,
    private liveStreamService: LiveStreamService
  ) {}

  loadLiveStreams$ = createEffect(() =>
    this.actions$.pipe(
      ofType(LiveStreamActions.loadLiveStreams),
      switchMap(({ page = 1, pageSize = 20 }) =>
        this.liveStreamService.getActiveLiveStreams({ page, pageSize }).pipe(
          map((response: any) => LiveStreamActions.loadLiveStreamsSuccess({
            liveStreams: response.items,
            totalCount: response.totalCount,
            page: response.page,
            pageSize: response.pageSize
          })),
          catchError(error => of(LiveStreamActions.loadLiveStreamsFailure({ error: error.message })))
        )
      )
    )
  );
}
