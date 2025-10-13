import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { ChannelService } from '../../services/channel.service';
import * as ChannelActions from './channel.actions';

@Injectable()
export class ChannelEffects {
  constructor(
    private actions$: Actions,
    private channelService: ChannelService
  ) {}

  loadChannels$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ChannelActions.loadChannels),
      switchMap(({ page = 1, pageSize = 20 }) =>
        this.channelService.getChannels({ page, pageSize }).pipe(
          map(response => ChannelActions.loadChannelsSuccess({
            channels: response.items,
            totalCount: response.totalCount,
            page: response.page,
            pageSize: response.pageSize
          })),
          catchError(error => of(ChannelActions.loadChannelsFailure({ error: error.message })))
        )
      )
    )
  );

  loadChannel$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ChannelActions.loadChannel),
      switchMap(({ id }) =>
        this.channelService.getChannelById(id).pipe(
          map(channel => ChannelActions.loadChannelSuccess({ channel })),
          catchError(error => of(ChannelActions.loadChannelFailure({ error: error.message })))
        )
      )
    )
  );
}
