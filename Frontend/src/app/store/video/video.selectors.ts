import { createSelector } from '@ngrx/store';
import { AppState } from '../app.state';

export const selectVideoState = (state: AppState) => state.video;

export const selectVideos = createSelector(
  selectVideoState,
  (state) => state.videos
);

export const selectCurrentVideo = createSelector(
  selectVideoState,
  (state) => state.currentVideo
);

export const selectVideoLoading = createSelector(
  selectVideoState,
  (state) => state.loading
);

export const selectVideoError = createSelector(
  selectVideoState,
  (state) => state.error
);

export const selectVideoTotalCount = createSelector(
  selectVideoState,
  (state) => state.totalCount
);

export const selectVideoCurrentPage = createSelector(
  selectVideoState,
  (state) => state.currentPage
);

export const selectVideoPageSize = createSelector(
  selectVideoState,
  (state) => state.pageSize
);

export const selectVideoSearchQuery = createSelector(
  selectVideoState,
  (state) => state.searchQuery
);

export const selectVideoSelectedCategory = createSelector(
  selectVideoState,
  (state) => state.selectedCategory
);
