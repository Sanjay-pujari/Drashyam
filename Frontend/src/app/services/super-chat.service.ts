import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SuperChat, SuperChatRequest, SuperChatDisplay } from '../models/super-chat.model';

@Injectable({
  providedIn: 'root'
})
export class SuperChatService {
  private apiUrl = `${environment.apiUrl}/api/superchat`;

  constructor(private http: HttpClient) {}

  processSuperChat(request: SuperChatRequest): Observable<SuperChat> {
    return this.http.post<SuperChat>(`${this.apiUrl}`, request);
  }

  getLiveStreamSuperChats(liveStreamId: number): Observable<SuperChatDisplay[]> {
    return this.http.get<SuperChatDisplay[]>(`${this.apiUrl}/live-stream/${liveStreamId}`);
  }

  getSuperChatById(superChatId: number): Observable<SuperChat> {
    return this.http.get<SuperChat>(`${this.apiUrl}/${superChatId}`);
  }

  getUserSuperChats(page: number = 1, pageSize: number = 20): Observable<SuperChat[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<SuperChat[]>(`${this.apiUrl}/user`, { params });
  }

  getCreatorSuperChats(page: number = 1, pageSize: number = 20): Observable<SuperChat[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<SuperChat[]>(`${this.apiUrl}/creator`, { params });
  }

  getSuperChatRevenue(startDate?: string, endDate?: string): Observable<number> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<number>(`${this.apiUrl}/revenue`, { params });
  }
}
