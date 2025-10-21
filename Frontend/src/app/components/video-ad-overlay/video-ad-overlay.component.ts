import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { VideoAd } from '../../services/video-ad.service';

@Component({
  selector: 'app-video-ad-overlay',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatProgressBarModule],
  template: `
    <div class="ad-overlay" *ngIf="ad && isVisible">
      <div class="ad-container">
        <!-- Ad Header -->
        <div class="ad-header">
          <div class="ad-label">
            <mat-icon>ads_click</mat-icon>
            Advertisement
          </div>
          <div class="ad-timer" *ngIf="!canSkip">
            {{ remainingTime }}s
          </div>
        </div>

        <!-- Ad Content -->
        <div class="ad-content" (click)="onAdClick()">
          <div class="ad-media" *ngIf="ad.thumbnailUrl">
            <img [src]="ad.thumbnailUrl" [alt]="'Advertisement'" class="ad-image">
          </div>
          <div class="ad-text" [innerHTML]="ad.content"></div>
        </div>

        <!-- Ad Controls -->
        <div class="ad-controls">
          <div class="ad-progress">
            <mat-progress-bar 
              mode="determinate" 
              [value]="progressValue"
              class="ad-progress-bar">
            </mat-progress-bar>
          </div>
          
          <div class="ad-buttons">
            <button 
              mat-button 
              color="primary" 
              (click)="onAdClick()"
              class="ad-click-button">
              <mat-icon>open_in_new</mat-icon>
              Learn More
            </button>
            
            <button 
              mat-button 
              *ngIf="canSkip"
              (click)="onSkipAd()"
              class="skip-button">
              <mat-icon>skip_next</mat-icon>
              Skip Ad ({{ remainingTime }}s)
            </button>
          </div>
        </div>

        <!-- Ad Close Button (for non-skippable ads) -->
        <button 
          mat-icon-button 
          class="close-button"
          (click)="onCloseAd()"
          *ngIf="!canSkip && ad.type !== 'pre-roll'">
          <mat-icon>close</mat-icon>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .ad-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.9);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .ad-container {
      background: white;
      border-radius: 12px;
      max-width: 90%;
      max-height: 90%;
      width: 600px;
      overflow: hidden;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
    }

    .ad-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 16px;
      background: #f5f5f5;
      border-bottom: 1px solid #e0e0e0;
    }

    .ad-label {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 14px;
      font-weight: 500;
      color: #666;
    }

    .ad-timer {
      background: #ff4444;
      color: white;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: bold;
    }

    .ad-content {
      padding: 20px;
      cursor: pointer;
      transition: transform 0.2s;
    }

    .ad-content:hover {
      transform: scale(1.02);
    }

    .ad-media {
      margin-bottom: 16px;
    }

    .ad-image {
      width: 100%;
      height: 200px;
      object-fit: cover;
      border-radius: 8px;
    }

    .ad-text {
      font-size: 16px;
      line-height: 1.5;
      color: #333;
    }

    .ad-controls {
      padding: 16px;
      background: #f9f9f9;
      border-top: 1px solid #e0e0e0;
    }

    .ad-progress {
      margin-bottom: 12px;
    }

    .ad-progress-bar {
      height: 4px;
    }

    .ad-buttons {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .ad-click-button {
      background: #1976d2;
      color: white;
    }

    .skip-button {
      background: #ff4444;
      color: white;
    }

    .close-button {
      position: absolute;
      top: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.5);
      color: white;
    }

    .close-button:hover {
      background: rgba(0, 0, 0, 0.7);
    }

    @media (max-width: 768px) {
      .ad-container {
        width: 95%;
        margin: 20px;
      }
      
      .ad-content {
        padding: 16px;
      }
      
      .ad-image {
        height: 150px;
      }
    }
  `]
})
export class VideoAdOverlayComponent implements OnInit, OnDestroy {
  @Input() ad: VideoAd | null = null;
  @Input() isVisible = false;
  @Output() adCompleted = new EventEmitter<void>();
  @Output() adSkipped = new EventEmitter<void>();
  @Output() adClicked = new EventEmitter<void>();
  @Output() adClosed = new EventEmitter<void>();

  remainingTime = 0;
  canSkip = false;
  progressValue = 0;
  private timer?: number;

  ngOnInit() {
    if (this.ad) {
      this.remainingTime = this.ad.duration;
      this.canSkip = this.ad.skipAfter === 0;
      this.startTimer();
    }
  }

  ngOnDestroy() {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }

  private startTimer() {
    if (!this.ad) return;

    this.timer = window.setInterval(() => {
      this.remainingTime--;
      this.progressValue = ((this.ad!.duration - this.remainingTime) / this.ad!.duration) * 100;

      // Enable skip button after skipAfter seconds
      if (this.remainingTime <= this.ad!.duration - this.ad!.skipAfter) {
        this.canSkip = true;
      }

      // Auto-complete ad when time runs out
      if (this.remainingTime <= 0) {
        this.onAdCompleted();
      }
    }, 1000);
  }

  onAdClick() {
    this.adClicked.emit();
  }

  onSkipAd() {
    if (this.timer) {
      clearInterval(this.timer);
    }
    this.adSkipped.emit();
  }

  onCloseAd() {
    if (this.timer) {
      clearInterval(this.timer);
    }
    this.adClosed.emit();
  }

  private onAdCompleted() {
    if (this.timer) {
      clearInterval(this.timer);
    }
    this.adCompleted.emit();
  }
}
