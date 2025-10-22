import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Store } from '@ngrx/store';
import { Observable, Subscription } from 'rxjs';

import { Comment, CommentCreate } from '../../models/comment.model';
import { User } from '../../models/user.model';
import { AppState } from '../../store/app.state';
import { selectCurrentUser } from '../../store/user/user.selectors';
import { CommentService } from '../../services/comment.service';
import { VideoSignalRService, CommentUpdate, CommentLikeUpdate } from '../../services/video-signalr.service';
import { PagedResult } from '../../services/video.service';

@Component({
  selector: 'app-comments',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './comments.component.html',
  styleUrls: ['./comments.component.scss']
})
export class CommentsComponent implements OnInit, OnDestroy {
  @Input() videoId!: number;
  @Input() commentCount!: number;

  comments: Comment[] = [];
  loading = false;
  submitting = false;
  error: string | null = null;
  
  newCommentContent = '';
  replyContents: { [key: number]: string } = {};
  showReplyForms: { [key: number]: boolean } = {};
  expandedReplies: { [key: number]: boolean } = {};
  
  currentUser$: Observable<User | null>;
  private subscriptions: Subscription[] = [];

  constructor(
    private store: Store<AppState>,
    private commentService: CommentService,
    private videoSignalRService: VideoSignalRService,
    private snackBar: MatSnackBar
  ) {
    this.currentUser$ = this.store.select(selectCurrentUser);
  }

  ngOnInit() {
    // Debug authentication state
    this.currentUser$.subscribe(user => {
      console.log('Comments Component - Current User:', user);
      console.log('Comments Component - User ID:', user?.id);
      console.log('Comments Component - User Name:', user?.firstName, user?.lastName);
    });
    
    this.loadComments();
    this.initializeRealTimeUpdates();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadComments() {
    this.loading = true;
    this.error = null;
    
    const sub = this.commentService.getVideoComments(this.videoId, { page: 1, pageSize: 50 })
      .subscribe({
        next: (result: PagedResult<Comment>) => {
          this.comments = result.items;
          this.loading = false;
        },
        error: (err) => {
          console.error('Detailed error loading comments:', err);
          
          // Provide more specific error messages
          if (err.status === 0) {
            this.error = 'Unable to connect to server. Please check if the backend is running.';
          } else if (err.status === 404) {
            this.error = 'Video not found.';
          } else if (err.status === 401) {
            this.error = 'Authentication required to load comments.';
          } else if (err.status === 500) {
            this.error = 'Server error occurred while loading comments.';
          } else {
            this.error = `Failed to load comments (Error ${err.status})`;
          }
          
          this.loading = false;
          
          // Don't show snackbar for 401 errors (not authenticated)
          if (err.status !== 401) {
            this.snackBar.open('Failed to load comments', 'Close', { duration: 3000 });
          }
        }
      });
    
    this.subscriptions.push(sub);
  }

  addComment() {
    if (!this.newCommentContent.trim()) return;
    
    this.submitting = true;
    const commentData: CommentCreate = {
      content: this.newCommentContent.trim(),
      videoId: this.videoId
    };

    const sub = this.commentService.addComment(commentData)
      .subscribe({
        next: (comment: Comment) => {
          this.comments.unshift(comment);
          this.newCommentContent = '';
          this.submitting = false;
          this.snackBar.open('Comment added successfully!', 'Close', { duration: 3000 });
        },
        error: (err) => {
          this.submitting = false;
          console.error('Error adding comment:', err);
          this.snackBar.open('Failed to add comment', 'Close', { duration: 3000 });
        }
      });
    
    this.subscriptions.push(sub);
  }

  addReply(parentCommentId: number) {
    const replyContent = this.replyContents[parentCommentId];
    if (!replyContent?.trim()) return;
    
    this.submitting = true;
    const commentData: CommentCreate = {
      content: replyContent.trim(),
      videoId: this.videoId,
      parentCommentId: parentCommentId
    };

    const sub = this.commentService.addComment(commentData)
      .subscribe({
        next: (reply: Comment) => {
          // Find the parent comment and add the reply
          const parentComment = this.comments.find(c => c.id === parentCommentId);
          if (parentComment) {
            if (!parentComment.replies) {
              parentComment.replies = [];
            }
            parentComment.replies.push(reply);
            parentComment.replyCount++;
          }
          
          this.replyContents[parentCommentId] = '';
          this.showReplyForms[parentCommentId] = false;
          this.submitting = false;
          this.snackBar.open('Reply added successfully!', 'Close', { duration: 3000 });
        },
        error: (err) => {
          this.submitting = false;
          console.error('Error adding reply:', err);
          this.snackBar.open('Failed to add reply', 'Close', { duration: 3000 });
        }
      });
    
    this.subscriptions.push(sub);
  }

  toggleReplyForm(commentId: number) {
    this.showReplyForms[commentId] = !this.showReplyForms[commentId];
    if (!this.showReplyForms[commentId]) {
      this.replyContents[commentId] = '';
    }
  }

  toggleReplies(commentId: number) {
    this.expandedReplies[commentId] = !this.expandedReplies[commentId];
    
    if (this.expandedReplies[commentId]) {
      this.loadReplies(commentId);
    }
  }

  loadReplies(commentId: number) {
    const comment = this.comments.find(c => c.id === commentId);
    if (!comment || comment.replyCount === 0) return;

    // Don't reload if already loaded
    if (comment.replies && comment.replies.length > 0) return;

    const sub = this.commentService.getCommentReplies(commentId, { page: 1, pageSize: 20 })
      .subscribe({
        next: (result: PagedResult<Comment>) => {
          comment.replies = result.items;
          console.log(`Loaded ${result.items.length} replies for comment ${commentId}`);
        },
        error: (err) => {
          console.error('Error loading replies:', err);
          this.snackBar.open('Failed to load replies', 'Close', { duration: 3000 });
        }
      });
    
    this.subscriptions.push(sub);
  }

  likeComment(comment: Comment) {
    const sub = this.commentService.likeComment(comment.id)
      .subscribe({
        next: (updatedComment: Comment) => {
          // Update the comment with new like count - create new object to avoid readonly error
          const commentIndex = this.comments.findIndex(c => c.id === comment.id);
          if (commentIndex !== -1) {
            this.comments[commentIndex] = {
              ...comment,
              likeCount: updatedComment.likeCount,
              dislikeCount: updatedComment.dislikeCount,
              isLiked: updatedComment.isLiked,
              isDisliked: updatedComment.isDisliked
            };
          }
          this.snackBar.open('Comment liked!', 'Close', { duration: 2000 });
        },
        error: (err) => {
          console.error('Error liking comment:', err);
          this.snackBar.open('Failed to like comment', 'Close', { duration: 3000 });
        }
      });
    
    this.subscriptions.push(sub);
  }

  unlikeComment(comment: Comment) {
    const sub = this.commentService.unlikeComment(comment.id)
      .subscribe({
        next: (updatedComment: Comment) => {
          // Update the comment with new like count - create new object to avoid readonly error
          const commentIndex = this.comments.findIndex(c => c.id === comment.id);
          if (commentIndex !== -1) {
            this.comments[commentIndex] = {
              ...comment,
              likeCount: updatedComment.likeCount,
              dislikeCount: updatedComment.dislikeCount,
              isLiked: updatedComment.isLiked,
              isDisliked: updatedComment.isDisliked
            };
          }
          this.snackBar.open('Comment unliked!', 'Close', { duration: 2000 });
        },
        error: (err) => {
          console.error('Error unliking comment:', err);
          this.snackBar.open('Failed to unlike comment', 'Close', { duration: 3000 });
        }
      });
    
    this.subscriptions.push(sub);
  }

  dislikeComment(comment: Comment) {
    const sub = this.commentService.dislikeComment(comment.id)
      .subscribe({
        next: (updatedComment: Comment) => {
          // Update the comment with new dislike count - create new object to avoid readonly error
          const commentIndex = this.comments.findIndex(c => c.id === comment.id);
          if (commentIndex !== -1) {
            this.comments[commentIndex] = {
              ...comment,
              likeCount: updatedComment.likeCount,
              dislikeCount: updatedComment.dislikeCount,
              isLiked: updatedComment.isLiked,
              isDisliked: updatedComment.isDisliked
            };
          }
          this.snackBar.open('Comment disliked!', 'Close', { duration: 2000 });
        },
        error: (err) => {
          console.error('Error disliking comment:', err);
          this.snackBar.open('Failed to dislike comment', 'Close', { duration: 3000 });
        }
      });
    
    this.subscriptions.push(sub);
  }

  deleteComment(comment: Comment) {
    if (!confirm('Are you sure you want to delete this comment?')) return;
    
    const sub = this.commentService.deleteComment(comment.id)
      .subscribe({
        next: () => {
          this.comments = this.comments.filter(c => c.id !== comment.id);
          this.snackBar.open('Comment deleted successfully!', 'Close', { duration: 3000 });
        },
        error: (err) => {
          console.error('Error deleting comment:', err);
          this.snackBar.open('Failed to delete comment', 'Close', { duration: 3000 });
        }
      });
    
    this.subscriptions.push(sub);
  }

  formatTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSeconds = Math.floor(diffMs / 1000);
    const diffMinutes = Math.floor(diffSeconds / 60);
    const diffHours = Math.floor(diffMinutes / 60);
    const diffDays = Math.floor(diffHours / 24);
    const diffMonths = Math.floor(diffDays / 30);
    const diffYears = Math.floor(diffDays / 365);

    if (diffYears > 0) return `${diffYears} year${diffYears > 1 ? 's' : ''} ago`;
    if (diffMonths > 0) return `${diffMonths} month${diffMonths > 1 ? 's' : ''} ago`;
    if (diffDays > 0) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    if (diffHours > 0) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffMinutes > 0) return `${diffMinutes} minute${diffMinutes > 1 ? 's' : ''} ago`;
    return 'Just now';
  }

  canDeleteComment(comment: Comment, currentUser: User | null): boolean {
    return currentUser ? comment.userId === currentUser.id : false;
  }

  canLikeComment(currentUser: User | null): boolean {
    return currentUser !== null;
  }

  // Real-time update methods
  private initializeRealTimeUpdates(): void {
    // Subscribe to comment updates
    const commentUpdateSub = this.videoSignalRService.commentUpdate$.subscribe(update => {
      if (update && update.videoId === this.videoId) {
        this.handleCommentUpdate(update);
      }
    });

    // Subscribe to comment like updates
    const commentLikeUpdateSub = this.videoSignalRService.commentLikeUpdate$.subscribe(update => {
      if (update) {
        this.handleCommentLikeUpdate(update);
      }
    });

    this.subscriptions.push(commentUpdateSub, commentLikeUpdateSub);
  }

  private handleCommentUpdate(update: CommentUpdate): void {
    if (update.action === 'added') {
      // Add new comment to the list
      this.comments.unshift(update.comment);
      this.commentCount++;
      this.snackBar.open('New comment added!', 'Close', { duration: 2000 });
    } else if (update.action === 'updated') {
      // Update existing comment
      const index = this.comments.findIndex(c => c.id === update.comment.id);
      if (index !== -1) {
        this.comments[index] = update.comment;
      }
    } else if (update.action === 'deleted') {
      // Remove comment from list
      this.comments = this.comments.filter(c => c.id !== update.comment.id);
      this.commentCount--;
    }
  }

  private handleCommentLikeUpdate(update: CommentLikeUpdate): void {
    // Find and update the comment - create new object to avoid readonly error
    const commentIndex = this.comments.findIndex(c => c.id === update.commentId);
    if (commentIndex !== -1) {
      this.comments[commentIndex] = {
        ...this.comments[commentIndex],
        likeCount: update.likeCount,
        dislikeCount: update.dislikeCount,
        isLiked: update.isLiked,
        isDisliked: update.isDisliked
      };
    }
  }
}
