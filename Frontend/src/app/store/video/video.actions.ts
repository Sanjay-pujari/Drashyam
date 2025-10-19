import { createAction, props } from '@ngrx/store';
import { Video } from '../../models/video.model';

// Load Videos
export const loadVideos = createAction(
  '[Video] Load Videos',
  props<{ page?: number; pageSize?: number; search?: string; category?: string; sortBy?: string; sortOrder?: string }>()
);

export const loadVideosSuccess = createAction(
  '[Video] Load Videos Success',
  props<{ videos: Video[]; totalCount: number; page: number; pageSize: number }>()
);

export const loadVideosFailure = createAction(
  '[Video] Load Videos Failure',
  props<{ error: string }>()
);

// Load Video by ID
export const loadVideo = createAction(
  '[Video] Load Video',
  props<{ id: number }>()
);

export const loadVideoSuccess = createAction(
  '[Video] Load Video Success',
  props<{ video: Video }>()
);

export const loadVideoFailure = createAction(
  '[Video] Load Video Failure',
  props<{ error: string }>()
);

// Upload Video
export const uploadVideo = createAction(
  '[Video] Upload Video',
  props<{ videoData: FormData }>()
);

export const uploadVideoSuccess = createAction(
  '[Video] Upload Video Success',
  props<{ video: Video }>()
);

export const uploadVideoFailure = createAction(
  '[Video] Upload Video Failure',
  props<{ error: string }>()
);

// Like/Dislike Video
export const likeVideo = createAction(
  '[Video] Like Video',
  props<{ videoId: number; likeType: 'like' | 'dislike' }>()
);

export const likeVideoSuccess = createAction(
  '[Video] Like Video Success',
  props<{ video: Video }>()
);

export const likeVideoFailure = createAction(
  '[Video] Like Video Failure',
  props<{ error: string }>()
);

// Record Video View
export const recordVideoView = createAction(
  '[Video] Record Video View',
  props<{ videoId: number; watchDuration: number }>()
);

export const recordVideoViewSuccess = createAction(
  '[Video] Record Video View Success',
  props<{ video: Video }>()
);

export const recordVideoViewFailure = createAction(
  '[Video] Record Video View Failure',
  props<{ error: string }>()
);

// Search Videos
export const searchVideos = createAction(
  '[Video] Search Videos',
  props<{ query: string; page?: number; pageSize?: number }>()
);

export const searchVideosSuccess = createAction(
  '[Video] Search Videos Success',
  props<{ videos: Video[]; totalCount: number; page: number; pageSize: number }>()
);

export const searchVideosFailure = createAction(
  '[Video] Search Videos Failure',
  props<{ error: string }>()
);

// Get Trending Videos
export const loadTrendingVideos = createAction(
  '[Video] Load Trending Videos',
  props<{ page?: number; pageSize?: number }>()
);

export const loadTrendingVideosSuccess = createAction(
  '[Video] Load Trending Videos Success',
  props<{ videos: Video[]; totalCount: number; page: number; pageSize: number }>()
);

export const loadTrendingVideosFailure = createAction(
  '[Video] Load Trending Videos Failure',
  props<{ error: string }>()
);

// Get Recommended Videos
export const loadRecommendedVideos = createAction(
  '[Video] Load Recommended Videos',
  props<{ page?: number; pageSize?: number }>()
);

export const loadRecommendedVideosSuccess = createAction(
  '[Video] Load Recommended Videos Success',
  props<{ videos: Video[]; totalCount: number; page: number; pageSize: number }>()
);

export const loadRecommendedVideosFailure = createAction(
  '[Video] Load Recommended Videos Failure',
  props<{ error: string }>()
);

// Clear Current Video
export const clearCurrentVideo = createAction(
  '[Video] Clear Current Video'
);
