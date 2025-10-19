import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
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
      </div>

      <mat-tab-group class="settings-tabs" animationDuration="300ms">
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
                    <mat-slide-toggle formControlName="profilePublic">
                      {{ privacyForm.get('profilePublic')?.value ? 'Public' : 'Private' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="privacy-option">
                    <div class="option-info">
                      <h3>Show Email</h3>
                      <p>Allow other users to see your email address</p>
                    </div>
                    <mat-slide-toggle formControlName="showEmail">
                      {{ privacyForm.get('showEmail')?.value ? 'Visible' : 'Hidden' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="privacy-option">
                    <div class="option-info">
                      <h3>Data Sharing</h3>
                      <p>Allow analytics and usage data to improve the platform</p>
                    </div>
                    <mat-slide-toggle formControlName="allowDataSharing">
                      {{ privacyForm.get('allowDataSharing')?.value ? 'Enabled' : 'Disabled' }}
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
                    <mat-slide-toggle formControlName="emailNotifications">
                      {{ notificationForm.get('emailNotifications')?.value ? 'Enabled' : 'Disabled' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="notification-option">
                    <div class="option-info">
                      <h3>Push Notifications</h3>
                      <p>Receive push notifications in your browser</p>
                    </div>
                    <mat-slide-toggle formControlName="pushNotifications">
                      {{ notificationForm.get('pushNotifications')?.value ? 'Enabled' : 'Disabled' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="notification-option">
                    <div class="option-info">
                      <h3>New Video Notifications</h3>
                      <p>Get notified when channels you subscribe to upload new videos</p>
                    </div>
                    <mat-slide-toggle formControlName="newVideoNotifications">
                      {{ notificationForm.get('newVideoNotifications')?.value ? 'Enabled' : 'Disabled' }}
                    </mat-slide-toggle>
                  </div>

                  <mat-divider></mat-divider>

                  <div class="notification-option">
                    <div class="option-info">
                      <h3>Comment Notifications</h3>
                      <p>Get notified when someone comments on your videos</p>
                    </div>
                    <mat-slide-toggle formControlName="commentNotifications">
                      {{ notificationForm.get('commentNotifications')?.value ? 'Enabled' : 'Disabled' }}
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
                      <input matInput formControlName="currentPassword" type="password" placeholder="Enter current password">
                      <mat-error *ngIf="passwordForm.get('currentPassword')?.hasError('required')">
                        Current password is required
                      </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="form-field full-width">
                      <mat-label>New Password</mat-label>
                      <input matInput formControlName="newPassword" type="password" placeholder="Enter new password">
                      <mat-error *ngIf="passwordForm.get('newPassword')?.hasError('required')">
                        New password is required
                      </mat-error>
                      <mat-error *ngIf="passwordForm.get('newPassword')?.hasError('minlength')">
                        Password must be at least 6 characters
                      </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="form-field full-width">
                      <mat-label>Confirm New Password</mat-label>
                      <input matInput formControlName="confirmPassword" type="password" placeholder="Confirm new password">
                      <mat-error *ngIf="passwordForm.get('confirmPassword')?.hasError('required')">
                        Password confirmation is required
                      </mat-error>
                      <mat-error *ngIf="passwordForm.get('confirmPassword')?.hasError('mismatch')">
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
      profilePublic: [true],
      showEmail: [false],
      allowDataSharing: [true]
    });

    // Notification form
    this.notificationForm = this.fb.group({
      emailNotifications: [true],
      pushNotifications: [true],
      newVideoNotifications: [true],
      commentNotifications: [true]
    });

    // Password form
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
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

    // Populate privacy form (using default values for now)
    this.privacyForm.patchValue({
      profilePublic: true,
      showEmail: false,
      allowDataSharing: true
    });

    // Populate notification form (using default values for now)
    this.notificationForm.patchValue({
      emailNotifications: true,
      pushNotifications: true,
      newVideoNotifications: true,
      commentNotifications: true
    });
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
    if (this.privacyForm.valid) {
      this.isUpdatingPrivacy = true;
      
      const privacySettings: PrivacySettings = {
        profilePublic: this.privacyForm.value.profilePublic,
        showEmail: this.privacyForm.value.showEmail,
        allowDataSharing: this.privacyForm.value.allowDataSharing
      };

      this.subscriptions.push(
        this.settingsService.updatePrivacySettings(privacySettings).subscribe({
          next: () => {
            this.snackBar.open('Privacy settings updated successfully!', 'Close', { duration: 3000 });
            this.isUpdatingPrivacy = false;
          },
          error: (error) => {
            this.snackBar.open('Failed to update privacy settings. Please try again.', 'Close', { duration: 3000 });
            this.isUpdatingPrivacy = false;
          }
        })
      );
    }
  }

  updateNotificationSettings(): void {
    if (this.notificationForm.valid) {
      this.isUpdatingNotifications = true;
      
      const notificationSettings: NotificationSettings = {
        emailNotifications: this.notificationForm.value.emailNotifications,
        pushNotifications: this.notificationForm.value.pushNotifications,
        newVideoNotifications: this.notificationForm.value.newVideoNotifications,
        commentNotifications: this.notificationForm.value.commentNotifications
      };

      this.subscriptions.push(
        this.settingsService.updateNotificationSettings(notificationSettings).subscribe({
          next: () => {
            this.snackBar.open('Notification settings updated successfully!', 'Close', { duration: 3000 });
            this.isUpdatingNotifications = false;
          },
          error: (error) => {
            this.snackBar.open('Failed to update notification settings. Please try again.', 'Close', { duration: 3000 });
            this.isUpdatingNotifications = false;
          }
        })
      );
    }
  }

  changePassword(): void {
    if (this.passwordForm.valid && this.currentUser) {
      this.isChangingPassword = true;
      
      const passwordData: ChangePasswordRequest = {
        currentPassword: this.passwordForm.value.currentPassword,
        newPassword: this.passwordForm.value.newPassword,
        confirmPassword: this.passwordForm.value.confirmPassword
      };

      this.subscriptions.push(
        this.settingsService.changePassword(passwordData).subscribe({
          next: () => {
            this.snackBar.open('Password changed successfully!', 'Close', { duration: 3000 });
            this.passwordForm.reset();
            this.isChangingPassword = false;
          },
          error: (error) => {
            this.snackBar.open('Failed to change password. Please check your current password.', 'Close', { duration: 3000 });
            this.isChangingPassword = false;
          }
        })
      );
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
}
