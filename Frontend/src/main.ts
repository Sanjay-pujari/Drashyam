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

// HTTP interceptor (functional)
import { authInterceptor } from './app/interceptors/auth.interceptor';

const routes: Routes = [
  { path: '', loadComponent: () => import('./app/components/home/home.component').then(m => m.HomeComponent) },
  { path: 'videos', loadComponent: () => import('./app/components/videos/videos.component').then(m => m.VideosComponent) },
  { path: 'channels', loadComponent: () => import('./app/components/channels/channels.component').then(m => m.ChannelsComponent) },
  { path: 'login', loadComponent: () => import('./app/components/login/login.component').then(m => m.LoginComponent) },
  { path: 'favorites', loadComponent: () => import('./app/components/favorites/favorites.component').then(m => m.FavoritesComponent) },
  { path: 'subscriptions', loadComponent: () => import('./app/components/subscriptions/subscriptions.component').then(m => m.SubscriptionsComponent) }
];

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptor])),
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
