import { bootstrapApplication } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Routes } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AppComponent } from './app/app.component';

// NgRx providers
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { environment } from './environments/environment';

// Reducers
import { videoReducer } from './app/store/video/video.reducer';
import { userReducer } from './app/store/user/user.reducer';
import { channelReducer } from './app/store/channel/channel.reducer';
import { commentReducer } from './app/store/comment/comment.reducer';
import { liveStreamReducer } from './app/store/live-stream/live-stream.reducer';
import { subscriptionReducer } from './app/store/subscription/subscription.reducer';

// Effects
import { VideoEffects } from './app/store/video/video.effects';
import { UserEffects } from './app/store/user/user.effects';
import { ChannelEffects } from './app/store/channel/channel.effects';
import { CommentEffects } from './app/store/comment/comment.effects';
import { LiveStreamEffects } from './app/store/live-stream/live-stream.effects';
import { SubscriptionEffects } from './app/store/subscription/subscription.effects';

// HTTP interceptors (functional)
import { authInterceptor } from './app/interceptors/auth.interceptor';
import { errorInterceptor } from './app/interceptors/error.interceptor';
import { authGuard } from './app/guards/auth.guard';

const routes: Routes = [
  { path: '', loadComponent: () => import('./app/components/home/home.component').then(m => m.HomeComponent) },
  { path: 'videos', loadComponent: () => import('./app/components/videos/videos.component').then(m => m.VideosComponent) },
  { path: 'videos/:id', loadComponent: () => import('./app/components/video-detail/video-detail.component').then(m => m.VideoDetailComponent) },
  { path: 'channels', loadComponent: () => import('./app/components/channels/channels.component').then(m => m.ChannelsComponent) },
  { path: 'channels/:id', loadComponent: () => import('./app/components/channel-detail/channel-detail.component').then(m => m.ChannelDetailComponent) },
  { path: 'channel/edit/:id', loadComponent: () => import('./app/components/channel-create/channel-create.component').then(m => m.ChannelCreateComponent), canActivate: [authGuard] },
  { path: 'login', loadComponent: () => import('./app/components/login/login.component').then(m => m.LoginComponent) },
  // Protected routes
  { path: 'favorites', loadComponent: () => import('./app/components/favorites/favorites.component').then(m => m.FavoritesComponent), canActivate: [authGuard] },
  { path: 'subscriptions', loadComponent: () => import('./app/components/subscriptions/subscriptions.component').then(m => m.SubscriptionsComponent), canActivate: [authGuard] },
  { path: 'upload', loadComponent: () => import('./app/components/video-upload/video-upload.component').then(m => m.VideoUploadComponent), canActivate: [authGuard] },
  { path: 'channel/create', loadComponent: () => import('./app/components/channel-create/channel-create.component').then(m => m.ChannelCreateComponent), canActivate: [authGuard] },
  { path: 'invites', loadComponent: () => import('./app/components/invite-management/invite-management.component').then(m => m.InviteManagementComponent), canActivate: [authGuard] },
  { path: 'referrals', loadComponent: () => import('./app/components/referral-management/referral-management.component').then(m => m.ReferralManagementComponent), canActivate: [authGuard] },
  { path: 'invite/:token', loadComponent: () => import('./app/components/invite-accept/invite-accept.component').then(m => m.InviteAcceptComponent) },
  // User content routes (protected)
  { path: 'liked-videos', loadComponent: () => import('./app/components/liked-videos/liked-videos.component').then(m => m.LikedVideosComponent), canActivate: [authGuard] },
  { path: 'history', loadComponent: () => import('./app/components/history/history.component').then(m => m.HistoryComponent), canActivate: [authGuard] },
  { path: 'watch-later', loadComponent: () => import('./app/components/watch-later/watch-later.component').then(m => m.WatchLaterComponent), canActivate: [authGuard] },
  { path: 'playlists', loadComponent: () => import('./app/components/playlists/playlists.component').then(m => m.PlaylistsComponent), canActivate: [authGuard] }
];

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    // If some legacy features still rely on module-based providers, bring them in via importProvidersFrom(...) here.
    provideStore({
      video: videoReducer,
      user: userReducer,
      channel: channelReducer,
      comment: commentReducer,
      liveStream: liveStreamReducer,
      subscription: subscriptionReducer
    }),
    provideEffects([VideoEffects, UserEffects, ChannelEffects, CommentEffects, LiveStreamEffects, SubscriptionEffects]),
    provideStoreDevtools({ maxAge: 25, logOnly: environment.production })
  ]
}).catch(err => console.error(err));
