import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { MerchandiseStore, MerchandiseStoreCreate, MerchandiseStoreUpdate } from '../models/merchandise.model';

@Injectable({
  providedIn: 'root'
})
export class MerchandiseService {
  private apiUrl = `${environment.apiUrl}/api/channel`;

  constructor(private http: HttpClient) { }

  // Create a new merchandise store for a channel
  createStore(channelId: number, store: MerchandiseStoreCreate): Observable<MerchandiseStore> {
    return this.http.post<MerchandiseStore>(`${this.apiUrl}/${channelId}/merchandise`, store);
  }

  // Get a specific merchandise store
  getStore(storeId: number): Observable<MerchandiseStore> {
    return this.http.get<MerchandiseStore>(`${this.apiUrl}/merchandise/${storeId}`);
  }

  // Get all merchandise stores for a channel
  getChannelStores(channelId: number): Observable<MerchandiseStore[]> {
    return this.http.get<MerchandiseStore[]>(`${this.apiUrl}/${channelId}/merchandise`);
  }

  // Update a merchandise store
  updateStore(storeId: number, store: MerchandiseStoreUpdate): Observable<MerchandiseStore> {
    return this.http.put<MerchandiseStore>(`${this.apiUrl}/merchandise/${storeId}`, store);
  }

  // Delete a merchandise store
  deleteStore(storeId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/merchandise/${storeId}`);
  }

  // Reorder merchandise stores for a channel
  reorderStores(channelId: number, storeIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${channelId}/merchandise/reorder`, storeIds);
  }

  // Toggle store active status
  toggleStoreStatus(storeId: number, isActive: boolean): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/merchandise/${storeId}/toggle-status`, isActive);
  }

  // Toggle store featured status
  toggleStoreFeatured(storeId: number, isFeatured: boolean): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/merchandise/${storeId}/toggle-featured`, isFeatured);
  }
}
