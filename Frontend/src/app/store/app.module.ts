import { NgModule } from '@angular/core';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { environment } from '../../environments/environment';

// Reducers
import { videoReducer } from './video/video.reducer';
import { userReducer } from './user/user.reducer';
import { channelReducer } from './channel/channel.reducer';
import { commentReducer } from './comment/comment.reducer';
import { liveStreamReducer } from './live-stream/live-stream.reducer';
import { subscriptionReducer } from './subscription/subscription.reducer';

// Effects
import { VideoEffects } from './video/video.effects';
import { UserEffects } from './user/user.effects';
import { ChannelEffects } from './channel/channel.effects';
import { CommentEffects } from './comment/comment.effects';
import { LiveStreamEffects } from './live-stream/live-stream.effects';
import { SubscriptionEffects } from './subscription/subscription.effects';

@NgModule({
  imports: [
    StoreModule.forRoot({
      video: videoReducer,
      user: userReducer,
      channel: channelReducer,
      comment: commentReducer,
      liveStream: liveStreamReducer,
      subscription: subscriptionReducer
    }),
    EffectsModule.forRoot([
      VideoEffects,
      UserEffects,
      ChannelEffects,
      CommentEffects,
      LiveStreamEffects,
      SubscriptionEffects
    ]),
    StoreDevtoolsModule.instrument({
      maxAge: 25,
      logOnly: environment.production
    })
  ]
})
export class AppStoreModule {}
