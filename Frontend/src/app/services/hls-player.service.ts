import { Injectable } from '@angular/core';
import Hls from 'hls.js';

export interface StreamQuality {
  name: string;
  width: number;
  height: number;
  bitrate: number;
  hlsUrl: string;
}

export interface StreamStatus {
  isLive: boolean;
  status: string;
  startTime: Date;
  viewerCount: number;
  hlsUrl: string;
  rtmpUrl: string;
  currentQuality: StreamQuality;
}

export interface StreamMetrics {
  bitrate: number;
  fps: number;
  resolution: number;
  cpuUsage: number;
  memoryUsage: number;
  networkLatency: number;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class HlsPlayerService {
  private hls: Hls | null = null;
  private videoElement: HTMLVideoElement | null = null;
  private currentStreamKey: string | null = null;
  private qualityLevels: StreamQuality[] = [];
  private isPlaying = false;
  private isBuffering = false;

  constructor() {}

  initializePlayer(videoElement: HTMLVideoElement): void {
    this.videoElement = videoElement;
    
    if (Hls.isSupported()) {
      this.hls = new Hls({
        enableWorker: true,
        lowLatencyMode: true,
        backBufferLength: 90,
        maxBufferLength: 30,
        maxMaxBufferLength: 120,
        liveSyncDurationCount: 3,
        liveMaxLatencyDurationCount: 5,
        liveDurationInfinity: true,
        highBufferWatchdogPeriod: 2,
        nudgeOffset: 0.1,
        nudgeMaxRetry: 3,
        maxFragLookUpTolerance: 0.25,
        liveBackBufferLength: 0,
        maxLiveSyncPlaybackRate: 1.2,
        liveSyncDuration: 0,
        liveMaxLatencyDuration: 0,
        maxBufferHole: 0.5,
        maxSeekHole: 2,
        seekHoleNudgeDuration: 0.01,
        maxFragLookUpTolerance: 0.25,
        liveDurationInfinity: true,
        liveBackBufferLength: 0,
        maxLiveSyncPlaybackRate: 1.2,
        liveSyncDuration: 0,
        liveMaxLatencyDuration: 0,
        maxBufferHole: 0.5,
        maxSeekHole: 2,
        seekHoleNudgeDuration: 0.01,
        maxFragLookUpTolerance: 0.25,
        liveDurationInfinity: true,
        liveBackBufferLength: 0,
        maxLiveSyncPlaybackRate: 1.2,
        liveSyncDuration: 0,
        liveMaxLatencyDuration: 0,
        maxBufferHole: 0.5,
        maxSeekHole: 2,
        seekHoleNudgeDuration: 0.01
      });

      this.setupEventListeners();
    } else if (this.videoElement.canPlayType('application/vnd.apple.mpegurl')) {
      // Native HLS support (Safari)
      this.setupNativeHls();
    } else {
      throw new Error('HLS is not supported in this browser');
    }
  }

  loadStream(streamKey: string, hlsUrl: string): Promise<void> {
    return new Promise((resolve, reject) => {
      if (!this.hls || !this.videoElement) {
        reject(new Error('Player not initialized'));
        return;
      }

      this.currentStreamKey = streamKey;
      
      this.hls.loadSource(hlsUrl);
      this.hls.attachMedia(this.videoElement);

      this.hls.on(Hls.Events.MANIFEST_PARSED, () => {
        console.log('Manifest parsed, starting playback');
        this.videoElement?.play();
        resolve();
      });

      this.hls.on(Hls.Events.ERROR, (event, data) => {
        console.error('HLS error:', data);
        if (data.fatal) {
          switch (data.type) {
            case Hls.ErrorTypes.NETWORK_ERROR:
              console.error('Fatal network error, trying to recover...');
              this.hls?.startLoad();
              break;
            case Hls.ErrorTypes.MEDIA_ERROR:
              console.error('Fatal media error, trying to recover...');
              this.hls?.recoverMediaError();
              break;
            default:
              console.error('Fatal error, cannot recover');
              this.hls?.destroy();
              reject(new Error('Fatal HLS error'));
              break;
          }
        }
      });
    });
  }

  play(): void {
    if (this.videoElement) {
      this.videoElement.play();
      this.isPlaying = true;
    }
  }

  pause(): void {
    if (this.videoElement) {
      this.videoElement.pause();
      this.isPlaying = false;
    }
  }

  stop(): void {
    if (this.hls) {
      this.hls.destroy();
      this.hls = null;
    }
    if (this.videoElement) {
      this.videoElement.pause();
      this.videoElement.currentTime = 0;
    }
    this.isPlaying = false;
    this.currentStreamKey = null;
  }

  setVolume(volume: number): void {
    if (this.videoElement) {
      this.videoElement.volume = Math.max(0, Math.min(1, volume));
    }
  }

  getVolume(): number {
    return this.videoElement?.volume || 0;
  }

  setMuted(muted: boolean): void {
    if (this.videoElement) {
      this.videoElement.muted = muted;
    }
  }

  isMuted(): boolean {
    return this.videoElement?.muted || false;
  }

  setQuality(quality: StreamQuality): void {
    if (this.hls && this.hls.levels.length > 0) {
      const levelIndex = this.hls.levels.findIndex(level => 
        level.width === quality.width && level.height === quality.height
      );
      
      if (levelIndex !== -1) {
        this.hls.currentLevel = levelIndex;
        console.log(`Switched to quality: ${quality.name}`);
      }
    }
  }

  getAvailableQualities(): StreamQuality[] {
    if (this.hls && this.hls.levels.length > 0) {
      return this.hls.levels.map((level, index) => ({
        name: `${level.height}p`,
        width: level.width,
        height: level.height,
        bitrate: level.bitrate,
        hlsUrl: this.hls?.url || ''
      }));
    }
    return this.qualityLevels;
  }

  getCurrentQuality(): StreamQuality | null {
    if (this.hls && this.hls.levels.length > 0 && this.hls.currentLevel !== -1) {
      const level = this.hls.levels[this.hls.currentLevel];
      return {
        name: `${level.height}p`,
        width: level.width,
        height: level.height,
        bitrate: level.bitrate,
        hlsUrl: this.hls.url || ''
      };
    }
    return null;
  }

  getStreamStatus(): StreamStatus {
    return {
      isLive: this.isPlaying,
      status: this.isPlaying ? 'Live' : 'Stopped',
      startTime: new Date(),
      viewerCount: 0,
      hlsUrl: this.hls?.url || '',
      rtmpUrl: '',
      currentQuality: this.getCurrentQuality() || this.qualityLevels[0] || {
        name: 'Auto',
        width: 1920,
        height: 1080,
        bitrate: 5000000,
        hlsUrl: ''
      }
    };
  }

  getStreamMetrics(): StreamMetrics {
    if (this.hls && this.hls.levels.length > 0 && this.hls.currentLevel !== -1) {
      const level = this.hls.levels[this.hls.currentLevel];
      return {
        bitrate: level.bitrate,
        fps: level.fps || 30,
        resolution: level.width * level.height,
        cpuUsage: 0,
        memoryUsage: 0,
        networkLatency: 0,
        timestamp: new Date()
      };
    }
    
    return {
      bitrate: 0,
      fps: 0,
      resolution: 0,
      cpuUsage: 0,
      memoryUsage: 0,
      networkLatency: 0,
      timestamp: new Date()
    };
  }

  isStreamHealthy(): boolean {
    return this.hls !== null && !this.hls.destroyed;
  }

  private setupEventListeners(): void {
    if (!this.hls || !this.videoElement) return;

    this.hls.on(Hls.Events.LEVEL_LOADED, (event, data) => {
      console.log('Level loaded:', data);
    });

    this.hls.on(Hls.Events.LEVEL_SWITCHED, (event, data) => {
      console.log('Level switched to:', data.level);
    });

    this.hls.on(Hls.Events.FRAG_LOADED, (event, data) => {
      console.log('Fragment loaded:', data.frag.url);
    });

    this.hls.on(Hls.Events.BUFFER_STALLED, () => {
      console.log('Buffer stalled');
      this.isBuffering = true;
    });

    this.hls.on(Hls.Events.BUFFER_APPENDED, () => {
      if (this.isBuffering) {
        console.log('Buffer recovered');
        this.isBuffering = false;
      }
    });

    this.videoElement.addEventListener('loadstart', () => {
      console.log('Video load started');
    });

    this.videoElement.addEventListener('loadeddata', () => {
      console.log('Video data loaded');
    });

    this.videoElement.addEventListener('canplay', () => {
      console.log('Video can play');
    });

    this.videoElement.addEventListener('waiting', () => {
      console.log('Video waiting for data');
      this.isBuffering = true;
    });

    this.videoElement.addEventListener('playing', () => {
      console.log('Video playing');
      this.isBuffering = false;
    });

    this.videoElement.addEventListener('pause', () => {
      console.log('Video paused');
    });

    this.videoElement.addEventListener('ended', () => {
      console.log('Video ended');
    });

    this.videoElement.addEventListener('error', (event) => {
      console.error('Video error:', event);
    });
  }

  private setupNativeHls(): void {
    if (!this.videoElement) return;

    this.videoElement.addEventListener('loadstart', () => {
      console.log('Native HLS load started');
    });

    this.videoElement.addEventListener('loadeddata', () => {
      console.log('Native HLS data loaded');
    });

    this.videoElement.addEventListener('canplay', () => {
      console.log('Native HLS can play');
    });

    this.videoElement.addEventListener('waiting', () => {
      console.log('Native HLS waiting for data');
      this.isBuffering = true;
    });

    this.videoElement.addEventListener('playing', () => {
      console.log('Native HLS playing');
      this.isBuffering = false;
    });
  }

  destroy(): void {
    this.stop();
    if (this.videoElement) {
      this.videoElement.removeEventListener('loadstart', () => {});
      this.videoElement.removeEventListener('loadeddata', () => {});
      this.videoElement.removeEventListener('canplay', () => {});
      this.videoElement.removeEventListener('waiting', () => {});
      this.videoElement.removeEventListener('playing', () => {});
      this.videoElement.removeEventListener('pause', () => {});
      this.videoElement.removeEventListener('ended', () => {});
      this.videoElement.removeEventListener('error', () => {});
    }
  }
}
