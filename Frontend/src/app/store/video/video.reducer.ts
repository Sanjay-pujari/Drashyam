import { createReducer, on } from '@ngrx/store';
import { VideoState, initialVideoState } from './video.state';
import * as VideoActions from './video.actions';

export const videoReducer = createReducer(
  initialVideoState,
  
  // Load Videos
  on(VideoActions.loadVideos, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(VideoActions.loadVideosSuccess, (state, { videos, totalCount, page, pageSize }) => ({
    ...state,
    videos,
    totalCount,
    currentPage: page,
    pageSize,
    loading: false,
    error: null
  })),
  
  on(VideoActions.loadVideosFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Load Video by ID
  on(VideoActions.loadVideo, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(VideoActions.loadVideoSuccess, (state, { video }) => ({
    ...state,
    currentVideo: video,
    loading: false,
    error: null
  })),
  
  on(VideoActions.loadVideoFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Upload Video
  on(VideoActions.uploadVideo, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(VideoActions.uploadVideoSuccess, (state, { video }) => ({
    ...state,
    videos: [video, ...state.videos],
    loading: false,
    error: null
  })),
  
  on(VideoActions.uploadVideoFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Like Video
  on(VideoActions.likeVideo, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(VideoActions.likeVideoSuccess, (state, { video }) => ({
    ...state,
    currentVideo: video,
    videos: state.videos.map(v => v.id === video.id ? video : v),
    loading: false,
    error: null
  })),
  
  on(VideoActions.likeVideoFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Record Video View
  on(VideoActions.recordVideoView, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(VideoActions.recordVideoViewSuccess, (state, { video }) => ({
    ...state,
    currentVideo: video,
    videos: state.videos.map(v => v.id === video.id ? video : v),
    loading: false,
    error: null
  })),
  
  on(VideoActions.recordVideoViewFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Search Videos
  on(VideoActions.searchVideos, (state, { query }) => ({
    ...state,
    searchQuery: query,
    loading: true,
    error: null
  })),
  
  on(VideoActions.searchVideosSuccess, (state, { videos, totalCount, page, pageSize }) => ({
    ...state,
    videos,
    totalCount,
    currentPage: page,
    pageSize,
    loading: false,
    error: null
  })),
  
  on(VideoActions.searchVideosFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Trending Videos
  on(VideoActions.loadTrendingVideos, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(VideoActions.loadTrendingVideosSuccess, (state, { videos, totalCount, page, pageSize }) => ({
    ...state,
    videos,
    totalCount,
    currentPage: page,
    pageSize,
    loading: false,
    error: null
  })),
  
  on(VideoActions.loadTrendingVideosFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Recommended Videos
  on(VideoActions.loadRecommendedVideos, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  
  on(VideoActions.loadRecommendedVideosSuccess, (state, { videos, totalCount, page, pageSize }) => ({
    ...state,
    videos,
    totalCount,
    currentPage: page,
    pageSize,
    loading: false,
    error: null
  })),
  
  on(VideoActions.loadRecommendedVideosFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Clear Current Video
  on(VideoActions.clearCurrentVideo, (state) => ({
    ...state,
    currentVideo: null
  }))
);
