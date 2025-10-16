import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, ViewChild, ElementRef, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Store } from '@ngrx/store';
import { Observable, Subscription, interval } from 'rxjs';
import { Video } from '../../models/video.model';
import { AppState } from '../../store/app.state';
import { selectCurrentUser } from '../../store/user/user.selectors';
import { recordVideoView, likeVideo } from '../../store/video/video.actions';
import { VideoService } from '../../services/video.service';
import { User } from '../../models/user.model';

declare var videojs: any;

@Component({
    selector: 'app-video-player',
    standalone: true,
    imports: [CommonModule, MatIconModule, MatProgressSpinnerModule],
    templateUrl: './video-player.component.html',
    styleUrls: ['./video-player.component.scss']
})
export class VideoPlayerComponent implements OnInit, OnDestroy {
  @Input() video: Video | null = null;
  @Input() autoplay = false;
  @Output() videoEnded = new EventEmitter<void>();
  @Output() videoPaused = new EventEmitter<void>();
  @Output() videoPlaying = new EventEmitter<void>();

  @ViewChild('videoElement', { static: false }) videoElement!: ElementRef;

  player: any;
  isPlaying = false;
  currentTime = 0;
  duration = 0;
  volume = 1;
  isMuted = false;
  isFullscreen = false;
  isLoading = true;
  watchStartTime = 0;
  private watchTimer?: Subscription;
  private viewRecorded = false;

  currentUser$: Observable<User | null>;

  constructor(
    @Inject(Store) private store: Store<AppState>,
    private videoService: VideoService
  ) {
    this.currentUser$ = this.store.select(selectCurrentUser);
  }

  ngOnInit() {
    if (this.video) {
      this.initializePlayer();
    }
  }

  ngOnDestroy() {
    this.cleanup();
  }

  private initializePlayer() {
    if (!this.video) return;

    // Initialize Video.js player
    this.player = videojs(this.videoElement.nativeElement, {
      controls: true,
      responsive: true,
      fluid: true,
      playbackRates: [0.5, 1, 1.25, 1.5, 2],
      plugins: {
        hotkeys: {
          volumeStep: 0.1,
          seekStep: 5,
          enableModifiersForNumbers: false
        }
      }
    });

    // Set video source
    this.player.src({
      src: this.video.videoUrl,
      type: 'video/mp4'
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
      this.video.viewCount++;
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
  }

  dislikeVideo() {
    if (!this.video) return;

    const likeType = this.video.isDisliked ? 'like' : 'dislike';
    this.store.dispatch(likeVideo({ 
      videoId: this.video.id, 
      likeType: likeType as 'like' | 'dislike' 
    }));
  }

  shareVideo() {
    if (!this.video) return;

    this.videoService.generateShareLink(this.video.id).subscribe({
      next: (response) => {
        const shareUrl = `${window.location.origin}/watch/${response.shareToken}`;
        this.copyToClipboard(shareUrl);
      },
      error: (error) => console.error('Error generating share link:', error)
    });
  }

  private copyToClipboard(text: string) {
    navigator.clipboard.writeText(text).then(() => {
      // Show success message
      console.log('Share link copied to clipboard');
    }).catch(err => {
      console.error('Failed to copy share link:', err);
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
