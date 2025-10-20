import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, NavigationEnd } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { SettingsService, PrivacySettings, NotificationSettings, ChangePasswordRequest } from '../../services/settings.service';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTabsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatSlideToggleModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDialogModule
  ],
  template: `
    <div class="settings-container">
      <div class="settings-header">
        <h1>Settings</h1>
        <p>Manage your account settings and preferences</p>
        <!-- Debug info (remove in production) -->
        <div class="debug-info" style="font-size: 12px; color: #666; margin-top: 10px;">
          Current Tab: {{ getCurrentTabName() }} (Index: {{ selectedTabIndex }})
          <button mat-button (click)="testTabNavigation()" style="margin-left: 10px;">Debug Tabs</button>
        </div>
      </div>

      <mat-tab-group class="settings-tabs" animationDuration="300ms" [(selectedIndex)]="selectedTabIndex" (selectedIndexChange)="onTabChange($event)">
        <!-- Profile Settings Tab -->
        <mat-tab label="Profile">
          <div class="tab-content">
            <mat-card class="settings-card">
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>person</mat-icon>
                  Profile Information
                </mat-card-title>
                <mat-card-subtitle>Update your personal information</mat-card-subtitle>
              </mat-card-header>
              <mat-card-content>
                <form [formGroup]="profileForm" (ngSubmit)="updateProfile()" class="settings-form">
                  <div class="form-row">
                    <mat-form-field appearance="outline" class="form-field">
                      <mat-label>First Name</mat-label>
                      <input matInput formControlName="firstName" placeholder="Enter your first name">
                      <mat-error *ngIf="profileForm.get('firstName')?.hasError('required')">
                        First name is required
                      </mat-error>
                    </mat-form-field>
                    
                    <mat-form-field appearance="outline" class="form-field">
                      <mat-label>Last Name</mat-label>
                      <input matInput formControlName="lastName" placeholder="Enter your last name">
                      <mat-error *ngIf="profileForm.get('lastName')?.hasError('required')">
                        Last name is required
                      </mat-error>
                    </mat-form-field>
                  </div>

                  <mat-form-field appearance="outline" class="form-field full-width">
                    <mat-label>Email</mat-label>
                    <input matInput formControlName="email" type="email" placeholder="Enter your email">
                    <mat-error *ngIf="profileForm.get('email')?.hasError('required')">
                      Email is required
                    </mat-error>
                    <mat-error *ngIf="profileForm.get('email')?.hasError('email')">
                      Please enter a valid email
                    </mat-error>
                  </mat-form-field>

                  <mat-form-field appearance="outline" class="form-field full-width">
                    <mat-label>Bio</mat-label>
                    <textarea matInput formControlName="bio" placeholder="Tell us about yourself" rows="3"></textarea>
                  </mat-form-field>

                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="profileForm.invalid || isUpdatingProfile">
                      <mat-icon *ngIf="isUpdatingProfile">hourglass_empty</mat-icon>
                      <mat-icon *ngIf="!isUpdatingProfile">save</mat-icon>
                      {{ isUpdatingProfile ? 'Updating...' : 'Update Profile' }}
                    </button>
                    <button mat-button type="button" (click)="resetProfileForm()">
                      <mat-icon>refresh</mat-icon>
                      Reset
                    </button>
                  </div>
                </form>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Privacy Settings Tab -->
        <mat-tab label="Privacy">
          <div class="tab-content">
            <mat-card class="settings-card">
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>security</mat-icon>
                  Privacy Settings
                </mat-card-title>
                <mat-card-subtitle>Control your privacy and visibility</mat-card-subtitle>
              </mat-card-header>
              <mat-card-content>
                <form [formGroup]="privacyForm" (ngSubmit)="updatePrivacySettings()" class="settings-form">
                  <div class="privacy-option">
                    <div class="option-info">
                      <h3>Profile Visibility</h3>
                      <p>Control who can see your profile information</p>
                    </div>
                    <mat-slide-toggle formControlName="ProfilePublic">
                      {{ privacyForm.get('ProfilePublic')?.value ? 'Public' : 'Private' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="privacy-option">
                    <div class="option-info">
                      <h3>Show Email</h3>
                      <p>Allow other users to see your email address</p>
                    </div>
                    <mat-slide-toggle formControlName="ShowEmail">
                      {{ privacyForm.get('ShowEmail')?.value ? 'Visible' : 'Hidden' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="privacy-option">
                    <div class="option-info">
                      <h3>Data Sharing</h3>
                      <p>Allow analytics and usage data to improve the platform</p>
                    </div>
                    <mat-slide-toggle formControlName="AllowDataSharing">
                      {{ privacyForm.get('AllowDataSharing')?.value ? 'Enabled' : 'Disabled' }}
                    </mat-slide-toggle>
                  </div>

                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="isUpdatingPrivacy">
                      <mat-icon *ngIf="isUpdatingPrivacy">hourglass_empty</mat-icon>
                      <mat-icon *ngIf="!isUpdatingPrivacy">save</mat-icon>
                      {{ isUpdatingPrivacy ? 'Updating...' : 'Save Privacy Settings' }}
                    </button>
                  </div>
                </form>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Notification Settings Tab -->
        <mat-tab label="Notifications">
          <div class="tab-content">
            <mat-card class="settings-card">
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>notifications</mat-icon>
                  Notification Preferences
                </mat-card-title>
                <mat-card-subtitle>Manage your notification settings</mat-card-subtitle>
              </mat-card-header>
              <mat-card-content>
                <form [formGroup]="notificationForm" (ngSubmit)="updateNotificationSettings()" class="settings-form">
                  <div class="notification-option">
                    <div class="option-info">
                      <h3>Email Notifications</h3>
                      <p>Receive notifications via email</p>
                    </div>
                    <mat-slide-toggle formControlName="EmailNotifications">
                      {{ notificationForm.get('EmailNotifications')?.value ? 'Enabled' : 'Disabled' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="notification-option">
                    <div class="option-info">
                      <h3>Push Notifications</h3>
                      <p>Receive push notifications in your browser</p>
                    </div>
                    <mat-slide-toggle formControlName="PushNotifications">
                      {{ notificationForm.get('PushNotifications')?.value ? 'Enabled' : 'Disabled' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="notification-option">
                    <div class="option-info">
                      <h3>New Video Notifications</h3>
                      <p>Get notified when channels you subscribe to upload new videos</p>
                    </div>
                    <mat-slide-toggle formControlName="NewVideoNotifications">
                      {{ notificationForm.get('NewVideoNotifications')?.value ? 'Enabled' : 'Disabled' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="notification-option">
                    <div class="option-info">
                      <h3>Comment Notifications</h3>
                      <p>Get notified when someone comments on your videos</p>
                    </div>
                    <mat-slide-toggle formControlName="CommentNotifications">
                      {{ notificationForm.get('CommentNotifications')?.value ? 'Enabled' : 'Disabled' }}
                    </mat-slide-toggle>
                  </div>

                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="isUpdatingNotifications">
                      <mat-icon *ngIf="isUpdatingNotifications">hourglass_empty</mat-icon>
                      <mat-icon *ngIf="!isUpdatingNotifications">save</mat-icon>
                      {{ isUpdatingNotifications ? 'Updating...' : 'Save Notification Settings' }}
                    </button>
                  </div>
                </form>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <!-- Account Settings Tab -->
        <mat-tab label="Account">
          <div class="tab-content">
            <mat-card class="settings-card">
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>account_circle</mat-icon>
                  Account Management
                </mat-card-title>
                <mat-card-subtitle>Manage your account security and preferences</mat-card-subtitle>
              </mat-card-header>
              <mat-card-content>
                <!-- Change Password Section -->
                <div class="account-section">
                  <h3>Change Password</h3>
                  <form [formGroup]="passwordForm" (ngSubmit)="changePassword()" class="settings-form">
                    <mat-form-field appearance="outline" class="form-field full-width">
                      <mat-label>Current Password</mat-label>
                      <input matInput formControlName="CurrentPassword" type="password" placeholder="Enter current password">
                      <mat-error *ngIf="passwordForm.get('CurrentPassword')?.hasError('required')">
                        Current password is required
                      </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="form-field full-width">
                      <mat-label>New Password</mat-label>
                      <input matInput formControlName="NewPassword" type="password" placeholder="Enter new password">
                      <mat-error *ngIf="passwordForm.get('NewPassword')?.hasError('required')">
                        New password is required
                      </mat-error>
                      <mat-error *ngIf="passwordForm.get('NewPassword')?.hasError('minlength')">
                        Password must be at least 6 characters
                      </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="form-field full-width">
                      <mat-label>Confirm New Password</mat-label>
                      <input matInput formControlName="ConfirmPassword" type="password" placeholder="Confirm new password">
                      <mat-error *ngIf="passwordForm.get('ConfirmPassword')?.hasError('required')">
                        Password confirmation is required
                      </mat-error>
                      <mat-error *ngIf="passwordForm.get('ConfirmPassword')?.hasError('mismatch')">
                        Passwords do not match
                      </mat-error>
                    </mat-form-field>

                    <div class="form-actions">
                      <button mat-raised-button color="primary" type="submit" [disabled]="passwordForm.invalid || isChangingPassword">
                        <mat-icon *ngIf="isChangingPassword">hourglass_empty</mat-icon>
                        <mat-icon *ngIf="!isChangingPassword">lock</mat-icon>
                        {{ isChangingPassword ? 'Changing...' : 'Change Password' }}
                      </button>
                    </div>
                  </form>
                </div>

                <mat-divider></mat-divider>

                <!-- Account Actions Section -->
                <div class="account-section">
                  <h3>Account Actions</h3>
                  <div class="account-actions">
                    <button mat-button color="warn" (click)="openDeleteAccountDialog()">
                      <mat-icon>delete_forever</mat-icon>
                      Delete Account
                    </button>
                    <button mat-button (click)="exportData()">
                      <mat-icon>download</mat-icon>
                      Export My Data
                    </button>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>
      </mat-tab-group>
    </div>
  `,
  styles: [`
    .settings-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem;
    }

    .settings-header {
      margin-bottom: 2rem;
      text-align: center;
    }

    .settings-header h1 {
      color: #333;
      margin-bottom: 0.5rem;
    }

    .settings-header p {
      color: #666;
      margin: 0;
    }

    .settings-tabs {
      margin-top: 2rem;
    }

    .settings-tabs ::ng-deep .mat-tab-group {
      width: 100%;
    }

    .settings-tabs ::ng-deep .mat-tab-header {
      border-bottom: 1px solid #e0e0e0;
    }

    .settings-tabs ::ng-deep .mat-tab-label {
      min-width: 120px;
      padding: 0 16px;
    }

    .settings-tabs ::ng-deep .mat-tab-label-active {
      color: #1976d2;
    }

    .tab-content {
      padding: 1rem 0;
    }

    .settings-card {
      margin-bottom: 2rem;
    }

    .settings-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    .form-field {
      width: 100%;
    }

    .form-field.full-width {
      grid-column: 1 / -1;
    }

    .form-actions {
      display: flex;
      gap: 1rem;
      margin-top: 1rem;
    }

    .privacy-option,
    .notification-option {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 0;
    }

    .option-info h3 {
      margin: 0 0 0.5rem 0;
      color: #333;
    }

    .option-info p {
      margin: 0;
      color: #666;
      font-size: 0.9rem;
    }

    .account-section {
      margin-bottom: 2rem;
    }

    .account-section h3 {
      color: #333;
      margin-bottom: 1rem;
    }

    .account-actions {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .account-actions button {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    @media (max-width: 768px) {
      .settings-container {
        padding: 1rem;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .privacy-option,
      .notification-option {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }

      .form-actions {
        flex-direction: column;
      }

      .account-actions {
        flex-direction: column;
      }
    }
  `]
})
export class SettingsComponent implements OnInit, OnDestroy {
  // Form groups
  profileForm!: FormGroup;
  privacyForm!: FormGroup;
  notificationForm!: FormGroup;
  passwordForm!: FormGroup;

  // Loading states
  isUpdatingProfile = false;
  isUpdatingPrivacy = false;
  isUpdatingNotifications = false;
  isChangingPassword = false;

  // Current user
  currentUser: User | null = null;
  private subscriptions: Subscription[] = [];

  // Tab navigation
  selectedTabIndex = 0;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private userService: UserService,
    private settingsService: SettingsService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private router: Router
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.loadCurrentUser();
    this.initializeTabFromUrl();
    this.setupFragmentListener();
    console.log('Settings component initialized with selectedTabIndex:', this.selectedTabIndex);
  }

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    console.log('Tab changed to index:', index);
    this.updateUrlForTab(index);
  }

  // Method to get current tab name for debugging
  getCurrentTabName(): string {
    const tabNames = ['profile', 'privacy', 'notifications', 'account'];
    return tabNames[this.selectedTabIndex] || 'profile';
  }

  private initializeTabFromUrl(): void {
    const url = this.router.url;
    if (url.includes('#privacy')) {
      this.selectedTabIndex = 1;
    } else if (url.includes('#notifications')) {
      this.selectedTabIndex = 2;
    } else if (url.includes('#account')) {
      this.selectedTabIndex = 3;
    } else {
      this.selectedTabIndex = 0; // Default to Profile tab
    }
  }

  private setupFragmentListener(): void {
    this.subscriptions.push(
      this.router.events.subscribe(event => {
        if (event instanceof NavigationEnd) {
          const url = this.router.url;
          if (url.includes('#privacy')) {
            this.selectedTabIndex = 1;
          } else if (url.includes('#notifications')) {
            this.selectedTabIndex = 2;
          } else if (url.includes('#account')) {
            this.selectedTabIndex = 3;
          } else if (url.includes('#profile')) {
            this.selectedTabIndex = 0;
          }
        }
      })
    );
  }

  private updateUrlForTab(index: number): void {
    const tabNames = ['profile', 'privacy', 'notifications', 'account'];
    const tabName = tabNames[index] || 'profile';
    this.router.navigate([], { 
      relativeTo: this.router.routerState.root,
      fragment: tabName,
      replaceUrl: true 
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private initializeForms(): void {
    // Profile form
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      bio: ['']
    });

    // Privacy form
    this.privacyForm = this.fb.group({
      ProfilePublic: [true],
      ShowEmail: [false],
      AllowDataSharing: [true]
    });

    // Notification form
    this.notificationForm = this.fb.group({
      EmailNotifications: [true],
      PushNotifications: [true],
      NewVideoNotifications: [true],
      CommentNotifications: [true]
    });

    // Password form
    this.passwordForm = this.fb.group({
      CurrentPassword: ['', [Validators.required]],
      NewPassword: ['', [Validators.required, Validators.minLength(6)]],
      ConfirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('NewPassword');
    const confirmPassword = form.get('ConfirmPassword');
    
    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true });
      return { mismatch: true };
    }
    
    return null;
  }

  private loadCurrentUser(): void {
    this.subscriptions.push(
      this.authService.currentUser$.subscribe(user => {
        this.currentUser = user;
        if (user) {
          this.populateForms(user);
          this.loadUserSettings();
        }
      })
    );
  }

  private populateForms(user: User): void {
    // Populate profile form
    this.profileForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      bio: user.bio || ''
    });
  }

  private loadUserSettings(): void {
    console.log('Loading user settings from API...');
    
    // Load privacy settings
    this.subscriptions.push(
      this.settingsService.getPrivacySettings().subscribe({
        next: (privacySettings) => {
          console.log('Loaded privacy settings:', privacySettings);
          this.privacyForm.patchValue({
            ProfilePublic: privacySettings.ProfilePublic,
            ShowEmail: privacySettings.ShowEmail,
            AllowDataSharing: privacySettings.AllowDataSharing
          });
        },
        error: (error) => {
          console.error('Failed to load privacy settings:', error);
          // Use default values if loading fails
          this.privacyForm.patchValue({
            ProfilePublic: true,
            ShowEmail: false,
            AllowDataSharing: true
          });
        }
      })
    );

    // Load notification settings
    this.subscriptions.push(
      this.settingsService.getNotificationSettings().subscribe({
        next: (notificationSettings) => {
          console.log('Loaded notification settings:', notificationSettings);
          this.notificationForm.patchValue({
            EmailNotifications: notificationSettings.EmailNotifications,
            PushNotifications: notificationSettings.PushNotifications,
            NewVideoNotifications: notificationSettings.NewVideoNotifications,
            CommentNotifications: notificationSettings.CommentNotifications
          });
        },
        error: (error) => {
          console.error('Failed to load notification settings:', error);
          // Use default values if loading fails
          this.notificationForm.patchValue({
            EmailNotifications: true,
            PushNotifications: true,
            NewVideoNotifications: true,
            CommentNotifications: true
          });
        }
      })
    );
  }

  updateProfile(): void {
    if (this.profileForm.valid && this.currentUser) {
      this.isUpdatingProfile = true;
      
      const updateData = {
        firstName: this.profileForm.value.firstName,
        lastName: this.profileForm.value.lastName,
        email: this.profileForm.value.email,
        bio: this.profileForm.value.bio
      };

      this.subscriptions.push(
        this.settingsService.updateProfile(updateData).subscribe({
          next: (updatedUser) => {
            this.snackBar.open('Profile updated successfully!', 'Close', { duration: 3000 });
            this.isUpdatingProfile = false;
            // Update the current user in auth service
            this.authService.getCurrentUser().subscribe();
          },
          error: (error) => {
            this.snackBar.open('Failed to update profile. Please try again.', 'Close', { duration: 3000 });
            this.isUpdatingProfile = false;
          }
        })
      );
    }
  }

  updatePrivacySettings(): void {
    console.log('Privacy form valid:', this.privacyForm.valid);
    console.log('Privacy form value:', this.privacyForm.value);
    
    if (this.privacyForm.valid) {
      this.isUpdatingPrivacy = true;
      
      const privacySettings: PrivacySettings = {
        ProfilePublic: this.privacyForm.value.ProfilePublic,
        ShowEmail: this.privacyForm.value.ShowEmail,
        AllowDataSharing: this.privacyForm.value.AllowDataSharing
      };

      console.log('Sending privacy settings:', privacySettings);

      this.subscriptions.push(
        this.settingsService.updatePrivacySettings(privacySettings).subscribe({
          next: (response) => {
            console.log('Privacy settings updated successfully:', response);
            this.snackBar.open('Privacy settings updated successfully!', 'Close', { duration: 3000 });
            this.isUpdatingPrivacy = false;
            // Reload the settings to ensure UI is updated
            this.loadUserSettings();
          },
          error: (error) => {
            console.error('Failed to update privacy settings:', error);
            this.snackBar.open('Failed to update privacy settings. Please try again.', 'Close', { duration: 3000 });
            this.isUpdatingPrivacy = false;
          }
        })
      );
    } else {
      console.log('Privacy form is invalid');
      this.snackBar.open('Please check your privacy settings form.', 'Close', { duration: 3000 });
    }
  }

  updateNotificationSettings(): void {
    console.log('Notification form valid:', this.notificationForm.valid);
    console.log('Notification form value:', this.notificationForm.value);
    
    if (this.notificationForm.valid) {
      this.isUpdatingNotifications = true;
      
      const notificationSettings: NotificationSettings = {
        EmailNotifications: this.notificationForm.value.EmailNotifications,
        PushNotifications: this.notificationForm.value.PushNotifications,
        NewVideoNotifications: this.notificationForm.value.NewVideoNotifications,
        CommentNotifications: this.notificationForm.value.CommentNotifications
      };

      console.log('Sending notification settings:', notificationSettings);

      this.subscriptions.push(
        this.settingsService.updateNotificationSettings(notificationSettings).subscribe({
          next: (response) => {
            console.log('Notification settings updated successfully:', response);
            this.snackBar.open('Notification settings updated successfully!', 'Close', { duration: 3000 });
            this.isUpdatingNotifications = false;
            // Reload the settings to ensure UI is updated
            this.loadUserSettings();
          },
          error: (error) => {
            console.error('Failed to update notification settings:', error);
            this.snackBar.open('Failed to update notification settings. Please try again.', 'Close', { duration: 3000 });
            this.isUpdatingNotifications = false;
          }
        })
      );
    } else {
      console.log('Notification form is invalid');
      this.snackBar.open('Please check your notification settings form.', 'Close', { duration: 3000 });
    }
  }

  changePassword(): void {
    console.log('Password form valid:', this.passwordForm.valid);
    console.log('Password form value:', this.passwordForm.value);
    console.log('Current user:', this.currentUser);
    
    if (this.passwordForm.valid && this.currentUser) {
      this.isChangingPassword = true;
      
      const passwordData: ChangePasswordRequest = {
        CurrentPassword: this.passwordForm.value.CurrentPassword,
        NewPassword: this.passwordForm.value.NewPassword,
        ConfirmPassword: this.passwordForm.value.ConfirmPassword
      };

      console.log('Sending password change request:', passwordData);

      this.subscriptions.push(
        this.settingsService.changePassword(passwordData).subscribe({
          next: (response) => {
            console.log('Password changed successfully:', response);
            this.snackBar.open('Password changed successfully!', 'Close', { duration: 3000 });
            this.passwordForm.reset();
            this.isChangingPassword = false;
          },
          error: (error) => {
            console.error('Failed to change password:', error);
            this.snackBar.open('Failed to change password. Please check your current password.', 'Close', { duration: 3000 });
            this.isChangingPassword = false;
          }
        })
      );
    } else {
      console.log('Password form is invalid or user not found');
      this.snackBar.open('Please check your password form.', 'Close', { duration: 3000 });
    }
  }

  resetProfileForm(): void {
    if (this.currentUser) {
      this.populateForms(this.currentUser);
    }
  }

  openDeleteAccountDialog(): void {
    // TODO: Implement delete account dialog
    this.snackBar.open('Delete account functionality coming soon!', 'Close', { duration: 3000 });
  }

  exportData(): void {
    // TODO: Implement data export functionality
    this.snackBar.open('Data export functionality coming soon!', 'Close', { duration: 3000 });
  }

  // Method to programmatically switch tabs (useful for debugging)
  switchToTab(tabName: string): void {
    const tabIndex = this.getTabIndexByName(tabName);
    if (tabIndex !== -1) {
      this.selectedTabIndex = tabIndex;
      this.updateUrlForTab(tabIndex);
    }
  }

  private getTabIndexByName(tabName: string): number {
    const tabNames = ['profile', 'privacy', 'notifications', 'account'];
    return tabNames.indexOf(tabName.toLowerCase());
  }

  // Debug method to test tab functionality
  testTabNavigation(): void {
    console.log('Current tab index:', this.selectedTabIndex);
    console.log('Current tab name:', this.getCurrentTabName());
    console.log('All tab names:', ['profile', 'privacy', 'notifications', 'account']);
  }
}
