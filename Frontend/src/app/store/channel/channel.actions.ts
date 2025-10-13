import { createAction, props } from '@ngrx/store';
import { Channel } from '../../models/channel.model';

export const loadChannels = createAction(
  '[Channel] Load Channels',
  props<{ page?: number; pageSize?: number }>()
);

export const loadChannelsSuccess = createAction(
  '[Channel] Load Channels Success',
  props<{ channels: Channel[]; totalCount: number; page: number; pageSize: number }>()
);

export const loadChannelsFailure = createAction(
  '[Channel] Load Channels Failure',
  props<{ error: string }>()
);

export const loadChannel = createAction(
  '[Channel] Load Channel',
  props<{ id: number }>()
);

export const loadChannelSuccess = createAction(
  '[Channel] Load Channel Success',
  props<{ channel: Channel }>()
);

export const loadChannelFailure = createAction(
  '[Channel] Load Channel Failure',
  props<{ error: string }>()
);
