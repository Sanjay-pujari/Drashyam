import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { VideoProcessingProgress } from '../models/video-processing-progress.model';

@Injectable({
  providedIn: 'root'
})
export class VideoProcessingService {
  private apiUrl = `${environment.apiUrl}/api/videoprocessing`;

  constructor(private http: HttpClient) { }

  getProcessingProgress(videoId: number): Observable<VideoProcessingProgress> {
    return this.http.get<VideoProcessingProgress>(`${this.apiUrl}/progress/${videoId}`);
  }

  getProcessingQueue(): Observable<VideoProcessingProgress[]> {
    return this.http.get<VideoProcessingProgress[]>(`${this.apiUrl}/queue`);
  }

  isVideoProcessing(videoId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/status/${videoId}`);
  }
}
