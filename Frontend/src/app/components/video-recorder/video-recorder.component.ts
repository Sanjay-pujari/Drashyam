import { Component, OnInit, OnDestroy, ViewChild, ElementRef, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { VideoService } from '../../services/video.service';

@Component({
  selector: 'app-video-recorder',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatCardModule],
  templateUrl: './video-recorder.component.html',
  styleUrls: ['./video-recorder.component.scss']
})
export class VideoRecorderComponent implements OnInit, OnDestroy {
  @ViewChild('videoElement', { static: false }) videoElement!: ElementRef<HTMLVideoElement>;
  @ViewChild('canvasElement', { static: false }) canvasElement!: ElementRef<HTMLCanvasElement>;
  
  @Output() recordingComplete = new EventEmitter<Blob>();
  @Output() recordingCanceled = new EventEmitter<void>();

  private mediaRecorder: MediaRecorder | null = null;
  private recordedChunks: Blob[] = [];
  private stream: MediaStream | null = null;

  isRecording = false;
  isPaused = false;
  recordingTime = 0;
  maxRecordingTime = 300; // 5 minutes max
  private recordingTimer: any;
  
  hasPermission = false;
  isInitializing = false;
  error: string | null = null;
  
  // Recording constraints
  private readonly constraints: MediaStreamConstraints = {
    video: {
      width: { ideal: 1920, min: 1280 },
      height: { ideal: 1080, min: 720 },
      facingMode: 'user',
      frameRate: { ideal: 30, min: 15 }
    },
    audio: {
      echoCancellation: true,
      noiseSuppression: true,
      autoGainControl: true
    }
  };

  constructor(
    private snackBar: MatSnackBar,
    private videoService: VideoService
  ) {}

  ngOnInit() {
    this.initializeCamera();
  }

  ngOnDestroy() {
    this.stopRecording();
    this.stopCamera();
  }

  async initializeCamera() {
    this.isInitializing = true;
    this.error = null;

    try {
      this.stream = await navigator.mediaDevices.getUserMedia(this.constraints);
      this.hasPermission = true;
      
      if (this.videoElement) {
        this.videoElement.nativeElement.srcObject = this.stream;
      }
    } catch (err: any) {
      console.error('Error accessing camera:', err);
      this.error = this.getErrorMessage(err);
      this.hasPermission = false;
    } finally {
      this.isInitializing = false;
    }
  }

  private getErrorMessage(error: any): string {
    if (error.name === 'NotAllowedError') {
      return 'Camera access denied. Please allow camera access and refresh the page.';
    } else if (error.name === 'NotFoundError') {
      return 'No camera found. Please connect a camera and try again.';
    } else if (error.name === 'NotSupportedError') {
      return 'Video recording is not supported in this browser.';
    } else {
      return 'Failed to access camera. Please try again.';
    }
  }

  startRecording() {
    if (!this.stream) {
      this.snackBar.open('Camera not available', 'Close', { duration: 3000 });
      return;
    }

    try {
      this.recordedChunks = [];
      this.mediaRecorder = new MediaRecorder(this.stream, {
        mimeType: this.getSupportedMimeType()
      });

      this.mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          this.recordedChunks.push(event.data);
        }
      };

      this.mediaRecorder.onstop = () => {
        const blob = new Blob(this.recordedChunks, { type: 'video/webm' });
        this.recordingComplete.emit(blob);
      };

      this.mediaRecorder.start(1000); // Collect data every second
      this.isRecording = true;
      this.recordingTime = 0;
      this.startTimer();
      
      this.snackBar.open('Recording started', 'Close', { duration: 2000 });
    } catch (error) {
      console.error('Error starting recording:', error);
      this.snackBar.open('Failed to start recording', 'Close', { duration: 3000 });
    }
  }

  pauseRecording() {
    if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
      this.mediaRecorder.pause();
      this.isPaused = true;
      this.stopTimer();
      this.snackBar.open('Recording paused', 'Close', { duration: 2000 });
    }
  }

  resumeRecording() {
    if (this.mediaRecorder && this.mediaRecorder.state === 'paused') {
      this.mediaRecorder.resume();
      this.isPaused = false;
      this.startTimer();
      this.snackBar.open('Recording resumed', 'Close', { duration: 2000 });
    }
  }

  stopRecording() {
    if (this.mediaRecorder && this.mediaRecorder.state !== 'inactive') {
      this.mediaRecorder.stop();
      this.isRecording = false;
      this.isPaused = false;
      this.stopTimer();
      this.snackBar.open('Recording stopped', 'Close', { duration: 2000 });
    }
  }

  cancelRecording() {
    // Stop recording without processing the video
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.stop();
    }
    
    // Stop timer and reset state
    this.stopTimer();
    this.isRecording = false;
    this.isPaused = false;
    this.recordingTime = 0;
    this.recordedChunks = [];
    
    // Emit canceled event
    this.recordingCanceled.emit();
  }

  private startTimer() {
    this.recordingTimer = setInterval(() => {
      if (this.recordingTime < this.maxRecordingTime) {
        this.recordingTime++;
      } else {
        this.stopRecording();
        this.snackBar.open('Maximum recording time reached', 'Close', { duration: 3000 });
      }
    }, 1000);
  }

  private stopTimer() {
    if (this.recordingTimer) {
      clearInterval(this.recordingTimer);
      this.recordingTimer = null;
    }
  }

  private stopCamera() {
    if (this.stream) {
      this.stream.getTracks().forEach(track => track.stop());
      this.stream = null;
    }
  }

  private getSupportedMimeType(): string {
    const types = [
      'video/webm;codecs=vp9',
      'video/webm;codecs=vp8',
      'video/webm',
      'video/mp4'
    ];

    for (const type of types) {
      if (MediaRecorder.isTypeSupported(type)) {
        return type;
      }
    }

    return 'video/webm'; // Fallback
  }

  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  retryCamera() {
    this.initializeCamera();
  }
}
