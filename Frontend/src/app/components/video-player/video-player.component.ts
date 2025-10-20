import { Component, OnInit, OnDestroy, AfterViewInit, Input, Output, EventEmitter, ViewChild, ElementRef, Inject, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommentsComponent } from '../comments/comments.component';
import { Store } from '@ngrx/store';
import { Observable, Subscription, interval } from 'rxjs';
import { Video } from '../../models/video.model';
import { AppState } from '../../store/app.state';
import { selectCurrentUser } from '../../store/user/user.selectors';
import { selectVideoById } from '../../store/video/video.selectors';
import { recordVideoView, recordVideoViewSuccess, likeVideo } from '../../store/video/video.actions';
import { VideoService } from '../../services/video.service';
import { WatchLaterService } from '../../services/watch-later.service';
import { PlaylistService } from '../../services/playlist.service';
import { User } from '../../models/user.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { AddToPlaylistDialogComponent } from '../add-to-playlist-dialog/add-to-playlist-dialog.component';
// Declare global videojs
declare var videojs: any;

@Component({
    selector: 'app-video-player',
    standalone: true,
    imports: [CommonModule, MatIconModule, MatProgressSpinnerModule, CommentsComponent],
    templateUrl: './video-player.component.html',
    styleUrls: ['./video-player.component.scss']
})
export class VideoPlayerComponent implements OnInit, AfterViewInit, OnDestroy, OnChanges {
  @Input() video: Video | null = null;
  @Input() autoplay = false;
  @Output() videoEnded = new EventEmitter<void>();
  @Output() videoPaused = new EventEmitter<void>();
  @Output() videoPlaying = new EventEmitter<void>();

  @ViewChild('videoElement', { static: false }) videoElement!: ElementRef<HTMLVideoElement>;

  player: any = null;
  isPlaying = false;
  currentTime = 0;
  duration = 0;
  volume = 1;
  isMuted = false;
  isFullscreen = false;
  isLoading = true;
  hasError = false;
  errorMessage = '';
  watchStartTime = 0;
  private watchTimer?: Subscription;

  currentUser$: Observable<User | null>;
  isInWatchLater = false;
  playlists: any[] = [];
  videoPlaylistStatus: { [playlistId: number]: boolean } = {};

  constructor(
    @Inject(Store) private store: Store<AppState>,
    private videoService: VideoService,
    private watchLaterService: WatchLaterService,
    private playlistService: PlaylistService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.currentUser$ = this.store.select(selectCurrentUser);
  }

  ngOnInit() {
    // Subscribe to video updates from the store
    if (this.video?.id) {
      this.store.select(selectVideoById, { id: this.video.id }).subscribe(updatedVideo => {
        if (updatedVideo && this.video && updatedVideo.id === this.video!.id) {
          this.video = updatedVideo;
          // Re-check watch later status when video updates
          this.checkWatchLaterStatus();
        }
      });
    }

    // Load playlists and check watch later status
    this.loadPlaylists();
    this.checkWatchLaterStatus();

    // Record view when user navigates away
    window.addEventListener('beforeunload', () => {
      this.recordView();
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['video'] && this.video) {
      // Video changed, re-check watch later status and playlist status
      this.checkWatchLaterStatus();
      this.loadPlaylists(); // This will also trigger checkVideoPlaylistStatus
    }
  }

  ngAfterViewInit() {
    // Initialize player after view is fully loaded
    if (this.video) {
      // Use setTimeout to ensure DOM is ready
      setTimeout(() => {
        this.initializePlayer();
      }, 100);
    }
  }

  ngOnDestroy() {
    this.recordView();
    
    if (this.player) {
      this.player.dispose();
      this.player = null;
    }
    
    if (this.watchTimer) {
      this.watchTimer.unsubscribe();
      this.watchTimer = undefined;
    }
    
    // Remove event listener
    window.removeEventListener('beforeunload', () => {
      this.recordView();
    });
  }

  private initializePlayer() {
    if (!this.video) return;

    // Wait for the DOM element to be available
    if (!this.videoElement?.nativeElement) {
      this.isLoading = false;
      return;
    }

    // Check if videojs is available
    if (typeof videojs === 'undefined') {
      this.isLoading = false;
      return;
    }
    
    // Initialize Video.js player
    this.player = videojs(this.videoElement.nativeElement, {
      controls: true,
      responsive: true,
      fluid: true,
      preload: 'metadata',
      playbackRates: [0.5, 1, 1.25, 1.5, 2],
      userActions: { hotkeys: true }
    });

    // Set video source
    
    // Check if the video URL is a placeholder or invalid
    let videoUrl = this.video.videoUrl;
    if (!videoUrl || videoUrl.includes('example.com') || videoUrl.includes('placeholder')) {
      // You can set a fallback video URL here or show an error message
    }
    
    this.player.src({
      src: videoUrl,
      type: this.detectMimeType(videoUrl)
    });

    // Add error handling
    this.player.on('error', (error: any) => {
      this.isLoading = false;
      this.hasError = true;
      this.errorMessage = 'Failed to load video. Please check the video URL or try again later.';
    });

    // Player event listeners
    this.player.ready(() => {
      this.isLoading = false;
      this.duration = this.player.duration();
      this.watchStartTime = Date.now();
    });

    this.player.on('play', () => {
      this.isPlaying = true;
      this.videoPlaying.emit();
      this.startWatchTimer();
      // Record initial view when video starts playing
      this.recordInitialView();
    });

    this.player.on('pause', () => {
      this.isPlaying = false;
      this.videoPaused.emit();
      this.stopWatchTimer();
      this.recordView();
    });

    this.player.on('ended', () => {
      this.isPlaying = false;
      this.videoEnded.emit();
      this.stopWatchTimer();
      this.recordView();
    });

    this.player.on('timeupdate', () => {
      this.currentTime = this.player.currentTime();
    });

    this.player.on('volumechange', () => {
      this.volume = this.player.volume();
      this.isMuted = this.player.muted();
    });

    this.player.on('fullscreenchange', () => {
      this.isFullscreen = this.player.isFullscreen();
    });

    // Autoplay if specified
    if (this.autoplay) {
      this.player.play();
    }
  }

  private startWatchTimer() {
    this.watchTimer = interval(10000).subscribe(() => {
      // Record view every 10 seconds of watching
      if (this.isPlaying && this.currentTime > 0) {
        this.recordView();
      }
    });
  }

  private stopWatchTimer() {
    if (this.watchTimer) {
      this.watchTimer.unsubscribe();
      this.watchTimer = undefined;
    }
  }

  private recordInitialView() {
    if (!this.video) {
      return;
    }

    
    // Record initial view with minimal duration
    this.videoService.recordVideoView(this.video.id, 1).subscribe({
      next: (updatedVideo) => {
        // Update store without triggering another API call
        this.store.dispatch(recordVideoViewSuccess({ video: updatedVideo }));
      },
      error: (error) => {
      }
    });
  }

  private recordView() {
    if (!this.video) {
      return;
    }

    const watchDuration = (Date.now() - this.watchStartTime) / 1000;
    if (watchDuration > 5) { // Only record if watched for more than 5 seconds
      // Record view using the video service endpoint (this also creates history entries)
      const seconds = Math.round(watchDuration);
      this.videoService.recordVideoView(this.video.id, seconds).subscribe({
        next: (updatedVideo) => {
          // Update the video in the store without re-calling the API
          this.store.dispatch(recordVideoViewSuccess({ video: updatedVideo }));
        },
        error: (error) => {
        }
      });
      
      // Reset the start time for potential future recordings
      this.watchStartTime = Date.now();
    }
  }

  private detectMimeType(url: string): string {
    const lower = (url || '').toLowerCase();
    if (lower.endsWith('.mp4')) return 'video/mp4';
    if (lower.endsWith('.webm')) return 'video/webm';
    if (lower.endsWith('.ogg') || lower.endsWith('.ogv')) return 'video/ogg';
    return 'video/mp4';
  }



  play() {
    if (this.player) {
      this.player.play();
    }
  }

  pause() {
    if (this.player) {
      this.player.pause();
    }
  }

  togglePlay() {
    if (this.isPlaying) {
      this.pause();
    } else {
      this.play();
    }
  }

  seekTo(time: number) {
    if (this.player) {
      this.player.currentTime(time);
    }
  }

  setVolume(volume: number) {
    if (this.player) {
      this.player.volume(volume);
    }
  }

  toggleMute() {
    if (this.player) {
      this.player.muted(!this.isMuted);
    }
  }

  toggleFullscreen() {
    if (this.player) {
      if (this.isFullscreen) {
        this.player.exitFullscreen();
      } else {
        this.player.requestFullscreen();
      }
    }
  }

  setPlaybackRate(rate: number) {
    if (this.player) {
      this.player.playbackRate(rate);
    }
  }

  likeVideo() {
    if (!this.video) return;

    const likeType = this.video.isLiked ? 'dislike' : 'like';
    this.store.dispatch(likeVideo({ 
      videoId: this.video.id, 
      likeType: likeType as 'like' | 'dislike' 
    }));

    // Show immediate feedback
    if (this.video.isLiked) {
      this.snackBar.open('Like removed', 'Close', { duration: 2000 });
    } else {
      this.snackBar.open('Video liked!', 'Close', { duration: 2000 });
    }
  }

  dislikeVideo() {
    if (!this.video) return;

    const likeType = this.video.isDisliked ? 'like' : 'dislike';
    this.store.dispatch(likeVideo({ 
      videoId: this.video.id, 
      likeType: likeType as 'like' | 'dislike' 
    }));

    // Show immediate feedback
    if (this.video.isDisliked) {
      this.snackBar.open('Dislike removed', 'Close', { duration: 2000 });
    } else {
      this.snackBar.open('Video disliked', 'Close', { duration: 2000 });
    }
  }

  shareVideo() {
    if (!this.video) return;

    this.videoService.generateShareLink(this.video.id).subscribe({
      next: (response) => {
        const shareUrl = `${window.location.origin}/watch/${response.shareToken}`;
        this.copyToClipboard(shareUrl);
      },
      error: (error) => {
        this.snackBar.open('Failed to generate share link', 'Close', { duration: 3000 });
      }
    });
  }

  private copyToClipboard(text: string) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).then(() => {
        this.snackBar.open('Share link copied to clipboard!', 'Close', { duration: 3000 });
      }).catch(err => {
        this.fallbackCopyToClipboard(text);
      });
    } else {
      this.fallbackCopyToClipboard(text);
    }
  }

  private fallbackCopyToClipboard(text: string) {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    textArea.style.top = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    
    try {
      document.execCommand('copy');
      this.snackBar.open('Share link copied to clipboard!', 'Close', { duration: 3000 });
    } catch (err) {
      this.snackBar.open('Share link: ' + text, 'Close', { duration: 5000 });
    }
    
    document.body.removeChild(textArea);
  }

  saveVideo() {
    if (!this.video) return;

    // Open the Add to Playlist dialog
    const dialogRef = this.dialog.open(AddToPlaylistDialogComponent, {
      width: '500px',
      data: { 
        videoId: this.video.id,
        videoPlaylistStatus: this.videoPlaylistStatus
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Refresh playlist status after changes
        this.checkVideoPlaylistStatus();
      }
    });
  }

  addToWatchLater() {
    if (!this.video) return;

    this.watchLaterService.addToWatchLater(this.video.id).subscribe({
      next: () => {
        this.isInWatchLater = true;
        this.snackBar.open('Added to watch later', 'Close', { duration: 3000 });
      },
      error: (error) => {
        this.snackBar.open('Failed to add to watch later', 'Close', { duration: 3000 });
      }
    });
  }

  removeFromWatchLater() {
    if (!this.video) return;

    this.watchLaterService.removeFromWatchLater(this.video.id).subscribe({
      next: () => {
        this.isInWatchLater = false;
        this.snackBar.open('Removed from watch later', 'Close', { duration: 3000 });
      },
      error: (error) => {
        this.snackBar.open('Failed to remove from watch later', 'Close', { duration: 3000 });
      }
    });
  }

  toggleWatchLater() {
    if (this.isInWatchLater) {
      this.removeFromWatchLater();
    } else {
      this.addToWatchLater();
    }
  }

  addToPlaylist(playlistId: number) {
    if (!this.video) return;

    this.playlistService.addVideoToPlaylist(playlistId, { videoId: this.video.id }).subscribe({
      next: () => {
        this.snackBar.open('Added to playlist', 'Close', { duration: 3000 });
      },
      error: (error) => {
        this.snackBar.open('Failed to add to playlist', 'Close', { duration: 3000 });
      }
    });
  }

  loadPlaylists() {
    this.playlistService.getPlaylists(1, 20).subscribe({
      next: (result) => {
        this.playlists = result.items || [];
        // Check which playlists contain this video
        this.checkVideoPlaylistStatus();
      },
      error: (error) => {
      }
    });
  }

  checkWatchLaterStatus() {
    if (!this.video) return;

    this.watchLaterService.isVideoInWatchLater(this.video.id).subscribe({
      next: (isInWatchLater) => {
        this.isInWatchLater = isInWatchLater;
      },
      error: (error) => {
      }
    });
  }

  checkVideoPlaylistStatus() {
    if (!this.video || this.playlists.length === 0) return;

    // Reset status
    this.videoPlaylistStatus = {};

    // Check each playlist
    this.playlists.forEach(playlist => {
      this.playlistService.getPlaylistVideos(playlist.id, 1, 1000).subscribe({
        next: (result) => {
          const isInPlaylist = result.items?.some(video => video.videoId === this.video?.id) || false;
          this.videoPlaylistStatus[playlist.id] = isInPlaylist;
        },
        error: (error) => {
          this.videoPlaylistStatus[playlist.id] = false;
        }
      });
    });
  }

  private cleanup() {
    this.stopWatchTimer();
    if (this.player) {
      this.player.dispose();
    }
  }

  formatTime(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    } else {
      return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }
  }
}
