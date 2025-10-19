import { Component, OnInit, OnDestroy, AfterViewInit, Input, Output, EventEmitter, ViewChild, ElementRef, Inject } from '@angular/core';
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
import { recordVideoView, likeVideo } from '../../store/video/video.actions';
import { VideoService } from '../../services/video.service';
import { User } from '../../models/user.model';
import { MatSnackBar } from '@angular/material/snack-bar';
// Declare global videojs
declare var videojs: any;

@Component({
    selector: 'app-video-player',
    standalone: true,
    imports: [CommonModule, MatIconModule, MatProgressSpinnerModule, CommentsComponent],
    templateUrl: './video-player.component.html',
    styleUrls: ['./video-player.component.scss']
})
export class VideoPlayerComponent implements OnInit, AfterViewInit, OnDestroy {
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
  private viewRecorded = false;

  currentUser$: Observable<User | null>;

  constructor(
    @Inject(Store) private store: Store<AppState>,
    private videoService: VideoService,
    private snackBar: MatSnackBar
  ) {
    this.currentUser$ = this.store.select(selectCurrentUser);
  }

  ngOnInit() {
    // Subscribe to video updates from the store
    if (this.video?.id) {
      this.store.select(selectVideoById, { id: this.video.id }).subscribe(updatedVideo => {
        if (updatedVideo && this.video && updatedVideo.id === this.video!.id) {
          this.video = updatedVideo;
        }
      });
    }

    // Record view when user navigates away
    window.addEventListener('beforeunload', () => {
      this.recordView();
    });
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
      console.error('Video element not found');
      this.isLoading = false;
      return;
    }

    // Check if videojs is available
    if (typeof videojs === 'undefined') {
      console.error('Video.js is not loaded');
      this.isLoading = false;
      return;
    }
    
    // Initialize Video.js player
    this.player = videojs(this.videoElement.nativeElement, {
      controls: true,
      responsive: true,
      fluid: true,
      playbackRates: [0.5, 1, 1.25, 1.5, 2]
    });

    // Set video source
    console.log('Setting video source:', this.video.videoUrl);
    
    // Check if the video URL is a placeholder or invalid
    let videoUrl = this.video.videoUrl;
    if (!videoUrl || videoUrl.includes('example.com') || videoUrl.includes('placeholder')) {
      console.warn('Using placeholder video URL, video may not load properly');
      // You can set a fallback video URL here or show an error message
    }
    
    this.player.src({
      src: videoUrl,
      type: 'video/mp4'
    });

    // Add error handling
    this.player.on('error', (error: any) => {
      console.error('Video player error:', error);
      console.error('Video URL:', this.video?.videoUrl);
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
    this.watchTimer = interval(1000).subscribe(() => {
      // Record view every 30 seconds
      if (this.currentTime > 0 && this.currentTime % 30 === 0) {
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

  private recordView() {
    if (!this.video || this.viewRecorded) {
      return;
    }

    const watchDuration = (Date.now() - this.watchStartTime) / 1000;
    if (watchDuration > 10) { // Only record if watched for more than 10 seconds
      this.store.dispatch(recordVideoView({ 
        videoId: this.video.id, 
        watchDuration 
      }));
      this.viewRecorded = true;
      // Don't update local count - let the store handle it
    }
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
        console.error('Error generating share link:', error);
        this.snackBar.open('Failed to generate share link', 'Close', { duration: 3000 });
      }
    });
  }

  private copyToClipboard(text: string) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).then(() => {
        this.snackBar.open('Share link copied to clipboard!', 'Close', { duration: 3000 });
      }).catch(err => {
        console.error('Failed to copy share link:', err);
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
      console.error('Fallback copy failed:', err);
      this.snackBar.open('Share link: ' + text, 'Close', { duration: 5000 });
    }
    
    document.body.removeChild(textArea);
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
