import { createReducer, on } from '@ngrx/store';
import { ChannelState, initialChannelState } from './channel.state';
import * as ChannelActions from './channel.actions';

export const channelReducer = createReducer(
  initialChannelState,
  
  on(ChannelActions.loadChannels, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(ChannelActions.loadChannelsSuccess, (state, { channels, totalCount, page, pageSize }) => ({
    ...state,
    channels,
    totalCount,
    currentPage: page,
    pageSize,
    loading: false,
    error: null
  })),
  
  on(ChannelActions.loadChannelsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  on(ChannelActions.loadChannel, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(ChannelActions.loadChannelSuccess, (state, { channel }) => ({
    ...state,
    currentChannel: channel,
    loading: false,
    error: null
  })),
  
  on(ChannelActions.loadChannelFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
