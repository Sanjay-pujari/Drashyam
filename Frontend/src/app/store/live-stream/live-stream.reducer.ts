import { createReducer, on } from '@ngrx/store';
import { LiveStreamState, initialLiveStreamState } from './live-stream.state';
import * as LiveStreamActions from './live-stream.actions';

export const liveStreamReducer = createReducer(
  initialLiveStreamState,
  
  on(LiveStreamActions.loadLiveStreams, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(LiveStreamActions.loadLiveStreamsSuccess, (state, { liveStreams, totalCount, page, pageSize }) => ({
    ...state,
    liveStreams,
    totalCount,
    currentPage: page,
    pageSize,
    loading: false,
    error: null
  })),
  
  on(LiveStreamActions.loadLiveStreamsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
