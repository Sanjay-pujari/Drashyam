import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Comment, CommentCreate, CommentUpdate } from '../models/comment.model';
import { PagedResult } from './video.service';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private apiUrl = `${environment.apiUrl}/api/comment`;

  constructor(private http: HttpClient) {}

  getVideoComments(videoId: number, filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Comment>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Comment>>(`${this.apiUrl}/video/${videoId}`, { params });
  }

  getCommentReplies(parentCommentId: number, filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Comment>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Comment>>(`${this.apiUrl}/${parentCommentId}/replies`, { params });
  }

  addComment(commentData: CommentCreate): Observable<Comment> {
    return this.http.post<Comment>(this.apiUrl, commentData);
  }

  updateComment(commentId: number, commentData: CommentUpdate): Observable<Comment> {
    return this.http.put<Comment>(`${this.apiUrl}/${commentId}`, commentData);
  }

  deleteComment(commentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${commentId}`);
  }

  likeComment(commentId: number): Observable<Comment> {
    return this.http.post<Comment>(`${this.apiUrl}/${commentId}/like`, {});
  }

  unlikeComment(commentId: number): Observable<Comment> {
    return this.http.post<Comment>(`${this.apiUrl}/${commentId}/unlike`, {});
  }

  dislikeComment(commentId: number): Observable<Comment> {
    return this.http.post<Comment>(`${this.apiUrl}/${commentId}/dislike`, {});
  }

  isCommentLiked(commentId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/${commentId}/is-liked`);
  }

  getUserComments(userId: string, filter: { page?: number; pageSize?: number } = {}): Observable<PagedResult<Comment>> {
    let params = new HttpParams();
    
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());

    return this.http.get<PagedResult<Comment>>(`${this.apiUrl}/user/${userId}`, { params });
  }

  reportComment(commentId: number, reason: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${commentId}/report`, { reason });
  }
}
