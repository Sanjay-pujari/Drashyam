import { Video } from '../../models/video.model';

export interface VideoState {
  videos: Video[];
  currentVideo: Video | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
  searchQuery: string;
  selectedCategory: string | null;
  sortBy: string;
  sortOrder: string;
}

export const initialVideoState: VideoState = {
  videos: [],
  currentVideo: null,
  loading: false,
  error: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 20,
  searchQuery: '',
  selectedCategory: null,
  sortBy: 'createdAt',
  sortOrder: 'desc'
};
