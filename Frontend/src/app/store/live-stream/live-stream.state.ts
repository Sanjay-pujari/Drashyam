import { LiveStream } from '../../models/live-stream.model';

export interface LiveStreamState {
  liveStreams: LiveStream[];
  currentLiveStream: LiveStream | null;
  activeLiveStreams: LiveStream[];
  userLiveStreams: LiveStream[];
  loading: boolean;
  error: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

export const initialLiveStreamState: LiveStreamState = {
  liveStreams: [],
  currentLiveStream: null,
  activeLiveStreams: [],
  userLiveStreams: [],
  loading: false,
  error: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 20
};
