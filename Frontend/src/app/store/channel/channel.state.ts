import { Channel } from '../../models/channel.model';

export interface ChannelState {
  channels: Channel[];
  currentChannel: Channel | null;
  userChannels: Channel[];
  subscribedChannels: Channel[];
  loading: boolean;
  error: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

export const initialChannelState: ChannelState = {
  channels: [],
  currentChannel: null,
  userChannels: [],
  subscribedChannels: [],
  loading: false,
  error: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 20
};
