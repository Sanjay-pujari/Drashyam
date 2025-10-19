import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { map, catchError, switchMap, withLatestFrom } from 'rxjs/operators';
import { VideoService } from '../../services/video.service';
import { AppState } from '../app.state';
import * as VideoActions from './video.actions';

@Injectable()
export class VideoEffects {
  constructor(
    private actions$: Actions,
    private store: Store<AppState>,
    private videoService: VideoService
  ) {}

  loadVideos$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VideoActions.loadVideos),
      switchMap(({ page = 1, pageSize = 20, search, category, sortBy = 'createdAt', sortOrder = 'desc' }) =>
        this.videoService.getVideos({ page, pageSize, search, category, sortBy, sortOrder }).pipe(
          map(response => VideoActions.loadVideosSuccess({
            videos: response.items,
            totalCount: response.totalCount,
            page: response.page,
            pageSize: response.pageSize
          })),
          catchError(error => of(VideoActions.loadVideosFailure({ error: error.message })))
        )
      )
    )
  );

  loadVideo$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VideoActions.loadVideo),
      switchMap(({ id }) =>
        this.videoService.getVideoById(id).pipe(
          map(video => VideoActions.loadVideoSuccess({ video })),
          catchError(error => of(VideoActions.loadVideoFailure({ error: error.message })))
        )
      )
    )
  );

  uploadVideo$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VideoActions.uploadVideo),
      switchMap(({ videoData }) =>
        this.videoService.uploadVideo(videoData).pipe(
          map(video => VideoActions.uploadVideoSuccess({ video })),
          catchError(error => of(VideoActions.uploadVideoFailure({ error: error.message })))
        )
      )
    )
  );

  likeVideo$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VideoActions.likeVideo),
      switchMap(({ videoId, likeType }) =>
        this.videoService.likeVideo(videoId, likeType).pipe(
          map(video => VideoActions.likeVideoSuccess({ video })),
          catchError(error => of(VideoActions.likeVideoFailure({ error: error.message })))
        )
      )
    )
  );

  recordVideoView$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VideoActions.recordVideoView),
      switchMap(({ videoId, watchDuration }) =>
        this.videoService.recordVideoView(videoId, watchDuration).pipe(
          map(video => VideoActions.recordVideoViewSuccess({ video })),
          catchError(error => of(VideoActions.recordVideoViewFailure({ error: error.message })))
        )
      )
    )
  );

  searchVideos$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VideoActions.searchVideos),
      switchMap(({ query, page = 1, pageSize = 20 }) =>
        this.videoService.searchVideos(query, { page, pageSize }).pipe(
          map(response => VideoActions.searchVideosSuccess({
            videos: response.items,
            totalCount: response.totalCount,
            page: response.page,
            pageSize: response.pageSize
          })),
          catchError(error => of(VideoActions.searchVideosFailure({ error: error.message })))
        )
      )
    )
  );

  loadTrendingVideos$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VideoActions.loadTrendingVideos),
      switchMap(({ page = 1, pageSize = 20 }) =>
        this.videoService.getTrendingVideos({ page, pageSize }).pipe(
          map(response => VideoActions.loadTrendingVideosSuccess({
            videos: response.items,
            totalCount: response.totalCount,
            page: response.page,
            pageSize: response.pageSize
          })),
          catchError(error => of(VideoActions.loadTrendingVideosFailure({ error: error.message })))
        )
      )
    )
  );

  loadRecommendedVideos$ = createEffect(() =>
    this.actions$.pipe(
      ofType(VideoActions.loadRecommendedVideos),
      switchMap(({ page = 1, pageSize = 20 }) =>
        this.videoService.getRecommendedVideos({ page, pageSize }).pipe(
          map(response => VideoActions.loadRecommendedVideosSuccess({
            videos: response.items,
            totalCount: response.totalCount,
            page: response.page,
            pageSize: response.pageSize
          })),
          catchError(error => of(VideoActions.loadRecommendedVideosFailure({ error: error.message })))
        )
      )
    )
  );
}
