import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Angular Material
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';

// Third-party modules
import { NgxSpinnerModule } from 'ngx-spinner';

// App components
import { AppComponent } from './app.component';
import { HeaderComponent } from './components/header/header.component';
import { VideoPlayerComponent } from './components/video-player/video-player.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { HomeComponent } from './components/home/home.component';

// NgRx Store (standalone providers)
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { environment } from '../environments/environment';

// Reducers
import { videoReducer } from './store/video/video.reducer';
import { userReducer } from './store/user/user.reducer';
import { channelReducer } from './store/channel/channel.reducer';
import { commentReducer } from './store/comment/comment.reducer';
import { liveStreamReducer } from './store/live-stream/live-stream.reducer';
import { subscriptionReducer } from './store/subscription/subscription.reducer';

// Effects
import { VideoEffects } from './store/video/video.effects';
import { UserEffects } from './store/user/user.effects';
import { ChannelEffects } from './store/channel/channel.effects';
import { CommentEffects } from './store/comment/comment.effects';
import { LiveStreamEffects } from './store/live-stream/live-stream.effects';
import { SubscriptionEffects } from './store/subscription/subscription.effects';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    SidebarComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule.forRoot([]),
    
    // Angular Material
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    
    // Third-party modules
    NgxSpinnerModule,

    // Standalone components
    VideoPlayerComponent,
    HomeComponent
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
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
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }