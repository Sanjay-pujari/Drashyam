import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../models/user.model';

export interface PrivacySettings {
  ProfilePublic: boolean;
  ShowEmail: boolean;
  AllowDataSharing: boolean;
}

export interface NotificationSettings {
  EmailNotifications: boolean;
  PushNotifications: boolean;
  NewVideoNotifications: boolean;
  CommentNotifications: boolean;
}

export interface ChangePasswordRequest {
  CurrentPassword: string;
  NewPassword: string;
  ConfirmPassword: string;
}

export interface DeleteAccountRequest {
  password: string;
  confirmationText: string;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private apiUrl = `${environment.apiUrl}/api/settings`;

  constructor(private http: HttpClient) {}

  // Profile Settings
  getProfile(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/profile`);
  }

  updateProfile(updateData: Partial<User>): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/profile`, updateData);
  }

  // Privacy Settings
  getPrivacySettings(): Observable<PrivacySettings> {
    return this.http.get<PrivacySettings>(`${this.apiUrl}/privacy`);
  }

  updatePrivacySettings(settings: PrivacySettings): Observable<PrivacySettings> {
    console.log('SettingsService - Sending privacy settings:', settings);
    return this.http.put<PrivacySettings>(`${this.apiUrl}/privacy`, settings);
  }

  // Notification Settings
  getNotificationSettings(): Observable<NotificationSettings> {
    return this.http.get<NotificationSettings>(`${this.apiUrl}/notifications`);
  }

  updateNotificationSettings(settings: NotificationSettings): Observable<NotificationSettings> {
    return this.http.put<NotificationSettings>(`${this.apiUrl}/notifications`, settings);
  }

  // Account Management
  changePassword(passwordData: ChangePasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/change-password`, passwordData);
  }

  exportData(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/export-data`, {});
  }

  deleteAccount(deleteData: DeleteAccountRequest): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/account`, { body: deleteData });
  }
}
