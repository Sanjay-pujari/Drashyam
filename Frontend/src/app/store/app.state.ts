import { VideoState } from './video/video.state';
import { UserState } from './user/user.state';
import { ChannelState } from './channel/channel.state';
import { CommentState } from './comment/comment.state';
import { LiveStreamState } from './live-stream/live-stream.state';
import { SubscriptionState } from './subscription/subscription.state';

export interface AppState {
  video: VideoState;
  user: UserState;
  channel: ChannelState;
  comment: CommentState;
  liveStream: LiveStreamState;
  subscription: SubscriptionState;
}
