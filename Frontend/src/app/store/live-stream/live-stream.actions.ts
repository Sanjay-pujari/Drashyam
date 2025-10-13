import { createAction, props } from '@ngrx/store';
import { LiveStream } from '../../models/live-stream.model';

export const loadLiveStreams = createAction(
  '[LiveStream] Load Live Streams',
  props<{ page?: number; pageSize?: number }>()
);

export const loadLiveStreamsSuccess = createAction(
  '[LiveStream] Load Live Streams Success',
  props<{ liveStreams: LiveStream[]; totalCount: number; page: number; pageSize: number }>()
);

export const loadLiveStreamsFailure = createAction(
  '[LiveStream] Load Live Streams Failure',
  props<{ error: string }>()
);
