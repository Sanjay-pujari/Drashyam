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
  { path: 'dashboard', redirectTo: '', pathMatch: 'full' },
  { path: 'videos', loadComponent: () => import('./app/components/videos/videos.component').then(m => m.VideosComponent) },
  { path: 'videos/:id', loadComponent: () => import('./app/components/video-detail/video-detail.component').then(m => m.VideoDetailComponent) },
  { path: 'watch/:token', loadComponent: () => import('./app/components/video-detail/video-detail.component').then(m => m.VideoDetailComponent) },
  { path: 'channels', loadComponent: () => import('./app/components/channels/channels.component').then(m => m.ChannelsComponent) },
  { path: 'channels/:id', loadComponent: () => import('./app/components/channel-detail/channel-detail.component').then(m => m.ChannelDetailComponent) },
  { path: 'channel/edit/:id', loadComponent: () => import('./app/components/channel-create/channel-create.component').then(m => m.ChannelCreateComponent), canActivate: [authGuard] },
  { path: 'login', loadComponent: () => import('./app/components/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./app/components/register/register.component').then(m => m.RegisterComponent) },
  // Protected routes
  { path: 'favorites', loadComponent: () => import('./app/components/favorites/favorites.component').then(m => m.FavoritesComponent), canActivate: [authGuard] },
  { path: 'upload', loadComponent: () => import('./app/components/video-upload/video-upload.component').then(m => m.VideoUploadComponent), canActivate: [authGuard] },
  { path: 'record', loadComponent: () => import('./app/components/video-record-upload/video-record-upload.component').then(m => m.VideoRecordUploadComponent), canActivate: [authGuard] },
  { path: 'channel/create', loadComponent: () => import('./app/components/channel-create/channel-create.component').then(m => m.ChannelCreateComponent), canActivate: [authGuard] },
  { path: 'invites', loadComponent: () => import('./app/components/invite-management/invite-management.component').then(m => m.InviteManagementComponent), canActivate: [authGuard] },
  { path: 'referrals', loadComponent: () => import('./app/components/referral-management/referral-management.component').then(m => m.ReferralManagementComponent), canActivate: [authGuard] },
  { path: 'invite/:token', loadComponent: () => import('./app/components/invite-accept/invite-accept.component').then(m => m.InviteAcceptComponent) },
  // User content routes (protected)
  { path: 'liked-videos', loadComponent: () => import('./app/components/liked-videos/liked-videos.component').then(m => m.LikedVideosComponent), canActivate: [authGuard] },
  { path: 'history', loadComponent: () => {
    console.log('Loading history component...');
    return import('./app/components/history/history.component').then(m => {
      console.log('History component loaded successfully');
      return m.HistoryComponent;
    });
  }, canActivate: [authGuard] },
  { path: 'watch-later', loadComponent: () => import('./app/components/watch-later/watch-later.component').then(m => m.WatchLaterComponent), canActivate: [authGuard] },
  { path: 'playlists', loadComponent: () => import('./app/components/playlists/playlists.component').then(m => m.PlaylistsComponent), canActivate: [authGuard] },
  { path: 'playlists/:id', loadComponent: () => import('./app/components/playlist-details/playlist-details.component').then(m => m.PlaylistDetailsComponent), canActivate: [authGuard] },
  { path: 'my-channels', loadComponent: () => import('./app/components/my-channels/my-channels.component').then(m => m.MyChannelsComponent), canActivate: [authGuard] },
  { path: 'subscriptions', loadComponent: () => import('./app/components/subscriptions/subscriptions.component').then(m => m.SubscriptionsComponent), canActivate: [authGuard] },
  { path: 'settings', loadComponent: () => import('./app/components/settings/settings.component').then(m => m.SettingsComponent), canActivate: [authGuard] },
  { path: 'ad-campaigns', loadComponent: () => import('./app/components/manage-campaigns/manage-campaigns.component').then(m => m.ManageCampaignsComponent), canActivate: [authGuard] },
  { path: 'my-videos', loadComponent: () => import('./app/components/video-management/video-management.component').then(m => m.VideoManagementComponent), canActivate: [authGuard] },
  { path: 'premium-content', loadComponent: () => import('./app/components/premium-content-management/premium-content-management.component').then(m => m.PremiumContentManagementComponent), canActivate: [authGuard] },
  { path: 'merchandise', loadComponent: () => import('./app/components/merchandise-management/merchandise-management.component').then(m => m.MerchandiseManagementComponent), canActivate: [authGuard] }
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
