import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { RecommendationService, Interaction } from './recommendation.service';

export interface VideoInteraction {
  videoId: number;
  type: 'view' | 'like' | 'dislike' | 'share' | 'comment' | 'watch_later' | 'complete' | 'skip';
  timestamp: Date;
  duration?: number;
  position?: number;
}

@Injectable({
  providedIn: 'root'
})
export class VideoInteractionService {
  private interactionSubject = new BehaviorSubject<VideoInteraction[]>([]);
  public interactions$ = this.interactionSubject.asObservable();
  
  private currentVideoId: number | null = null;
  private watchStartTime: Date | null = null;
  private lastPosition: number = 0;

  constructor(private recommendationService: RecommendationService) {
    // Track interactions every 30 seconds
    setInterval(() => {
      this.trackPeriodicInteraction();
    }, 30000);
  }

  // Start watching a video
  startWatching(videoId: number): void {
    this.currentVideoId = videoId;
    this.watchStartTime = new Date();
    this.lastPosition = 0;
    
    this.trackInteraction({
      videoId,
      type: 'view',
      timestamp: new Date()
    });
  }

  // Stop watching a video
  stopWatching(): void {
    if (this.currentVideoId && this.watchStartTime) {
      const duration = Date.now() - this.watchStartTime.getTime();
      
      this.trackInteraction({
        videoId: this.currentVideoId,
        type: 'complete',
        timestamp: new Date(),
        duration: Math.floor(duration / 1000) // Convert to seconds
      });
    }
    
    this.currentVideoId = null;
    this.watchStartTime = null;
    this.lastPosition = 0;
  }

  // Track video position
  updatePosition(position: number): void {
    this.lastPosition = position;
  }

  // Track like
  trackLike(videoId: number): void {
    this.trackInteraction({
      videoId,
      type: 'like',
      timestamp: new Date()
    });
  }

  // Track dislike
  trackDislike(videoId: number): void {
    this.trackInteraction({
      videoId,
      type: 'dislike',
      timestamp: new Date()
    });
  }

  // Track share
  trackShare(videoId: number): void {
    this.trackInteraction({
      videoId,
      type: 'share',
      timestamp: new Date()
    });
  }

  // Track comment
  trackComment(videoId: number): void {
    this.trackInteraction({
      videoId,
      type: 'comment',
      timestamp: new Date()
    });
  }

  // Track watch later
  trackWatchLater(videoId: number): void {
    this.trackInteraction({
      videoId,
      type: 'watch_later',
      timestamp: new Date()
    });
  }

  // Track skip
  trackSkip(videoId: number): void {
    this.trackInteraction({
      videoId,
      type: 'skip',
      timestamp: new Date()
    });
  }

  // Get interaction history
  getInteractionHistory(): VideoInteraction[] {
    return this.interactionSubject.value;
  }

  // Clear interaction history
  clearHistory(): void {
    this.interactionSubject.next([]);
  }

  private trackInteraction(interaction: VideoInteraction): void {
    const currentInteractions = this.interactionSubject.value;
    this.interactionSubject.next([...currentInteractions, interaction]);

    // Send to recommendation service
    this.sendToRecommendationService(interaction);
  }

  private trackPeriodicInteraction(): void {
    if (this.currentVideoId && this.watchStartTime) {
      const duration = Date.now() - this.watchStartTime.getTime();
      
      // Only track if user has been watching for more than 30 seconds
      if (duration > 30000) {
        this.trackInteraction({
          videoId: this.currentVideoId,
          type: 'view',
          timestamp: new Date(),
          duration: Math.floor(duration / 1000),
          position: this.lastPosition
        });
      }
    }
  }

  private sendToRecommendationService(interaction: VideoInteraction): void {
    const recommendationInteraction: Interaction = {
      videoId: interaction.videoId,
      type: interaction.type,
      score: this.getInteractionScore(interaction.type),
      watchDuration: interaction.duration ? `${interaction.duration}` : undefined
    };

    this.recommendationService.trackInteraction(recommendationInteraction).subscribe({
      next: () => {
        // Interaction tracked successfully
      },
      error: (error) => {
        console.error('Error tracking interaction:', error);
      }
    });
  }

  private getInteractionScore(type: string): number {
    switch (type) {
      case 'like': return 1.0;
      case 'dislike': return -0.5;
      case 'share': return 0.8;
      case 'comment': return 0.9;
      case 'watch_later': return 0.7;
      case 'complete': return 1.0;
      case 'skip': return -0.3;
      case 'view': return 0.5;
      default: return 0.1;
    }
  }
}
