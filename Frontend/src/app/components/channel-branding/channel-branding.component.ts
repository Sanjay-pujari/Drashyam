import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { ChannelBrandingService, ChannelBranding } from '../../services/channel-branding.service';

@Component({
  selector: 'app-channel-branding',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule
  ],
  providers: [ChannelBrandingService],
  template: `
    <div class="channel-branding-container">
      <div class="header">
        <h1>Channel Branding</h1>
        <p>Customize your channel's appearance and branding</p>
      </div>

      @if (isLoading) {
        <div class="loading-container">
          <mat-spinner diameter="50"></mat-spinner>
          <p>Loading branding settings...</p>
        </div>
      } @else {
        <div class="branding-content">
          <!-- Branding Form -->
          <mat-card class="branding-form-card">
            <mat-card-header>
              <mat-card-title>
                <mat-icon>palette</mat-icon>
                Branding Settings
              </mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <form [formGroup]="brandingForm" (ngSubmit)="onSubmit()">
                <div class="form-section">
                  <h3>Visual Identity</h3>
                  <div class="form-row">
                    <mat-form-field appearance="outline" class="half-width">
                      <mat-label>Logo URL</mat-label>
                      <input matInput formControlName="logoUrl" placeholder="https://example.com/logo.png">
                    </mat-form-field>
                    <mat-form-field appearance="outline" class="half-width">
                      <mat-label>Banner URL</mat-label>
                      <input matInput formControlName="bannerUrl" placeholder="https://example.com/banner.jpg">
                    </mat-form-field>
                  </div>
                </div>

                <div class="form-section">
                  <h3>Color Scheme</h3>
                  <div class="form-row">
                    <mat-form-field appearance="outline" class="third-width">
                      <mat-label>Primary Color</mat-label>
                      <input matInput formControlName="primaryColor" placeholder="#FF5722" type="color">
                    </mat-form-field>
                    <mat-form-field appearance="outline" class="third-width">
                      <mat-label>Secondary Color</mat-label>
                      <input matInput formControlName="secondaryColor" placeholder="#2196F3" type="color">
                    </mat-form-field>
                    <mat-form-field appearance="outline" class="third-width">
                      <mat-label>Accent Color</mat-label>
                      <input matInput formControlName="accentColor" placeholder="#4CAF50" type="color">
                    </mat-form-field>
                  </div>
                </div>

                <div class="form-section">
                  <h3>Custom Domain</h3>
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Custom Domain</mat-label>
                    <input matInput formControlName="customDomain" placeholder="mybrand.drashyam.com">
                    <mat-hint>Optional: Use your own domain for your channel</mat-hint>
                  </mat-form-field>
                </div>

                <div class="form-section">
                  <h3>About Section</h3>
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>About Text</mat-label>
                    <textarea matInput formControlName="aboutText" rows="4" placeholder="Tell viewers about your channel..."></textarea>
                  </mat-form-field>
                </div>

                <div class="form-section">
                  <h3>Links</h3>
                  <div class="form-row">
                    <mat-form-field appearance="outline" class="half-width">
                      <mat-label>Website URL</mat-label>
                      <input matInput formControlName="websiteUrl" placeholder="https://mywebsite.com">
                    </mat-form-field>
                    <mat-form-field appearance="outline" class="half-width">
                      <mat-label>Social Media Links (JSON)</mat-label>
                      <input matInput formControlName="socialMediaLinks" placeholder='{"twitter": "https://twitter.com/username", "instagram": "https://instagram.com/username"}'>
                    </mat-form-field>
                  </div>
                </div>

                <div class="form-section">
                  <h3>Custom CSS</h3>
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Custom CSS</mat-label>
                    <textarea matInput formControlName="customCss" rows="6" placeholder="/* Add your custom CSS here */"></textarea>
                    <mat-hint>Advanced: Add custom CSS to further customize your channel appearance</mat-hint>
                  </mat-form-field>
                </div>

                <div class="form-actions">
                  <button mat-raised-button color="primary" type="submit" [disabled]="isSubmitting">
                    @if (isSubmitting) {
                      <mat-spinner diameter="20"></mat-spinner>
                    } @else {
                      <mat-icon>save</mat-icon>
                    }
                    {{ branding ? 'Update' : 'Create' }} Branding
                  </button>
                  @if (branding) {
                    <button mat-stroked-button type="button" (click)="toggleActive()" [disabled]="isSubmitting">
                      <mat-icon>{{ branding.isActive ? 'visibility_off' : 'visibility' }}</mat-icon>
                      {{ branding.isActive ? 'Deactivate' : 'Activate' }}
                    </button>
                    <button mat-stroked-button color="warn" type="button" (click)="deleteBranding()" [disabled]="isSubmitting">
                      <mat-icon>delete</mat-icon>
                      Delete
                    </button>
                  }
                </div>
              </form>
            </mat-card-content>
          </mat-card>

          <!-- Preview Card -->
          @if (branding) {
            <mat-card class="preview-card">
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>preview</mat-icon>
                  Preview
                </mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="channel-preview" [style]="getPreviewStyles()">
                  @if (branding.bannerUrl) {
                    <div class="channel-banner" [style.background-image]="'url(' + branding.bannerUrl + ')'"></div>
                  }
                  <div class="channel-info">
                    @if (branding.logoUrl) {
                      <img [src]="branding.logoUrl" alt="Channel Logo" class="channel-logo">
                    }
                    <div class="channel-details">
                      <h2>Channel Name</h2>
                      @if (branding.aboutText) {
                        <p>{{ branding.aboutText }}</p>
                      }
                    </div>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .channel-branding-container {
      max-width: 1000px;
      margin: 0 auto;
      padding: 20px;
    }

    .header {
      text-align: center;
      margin-bottom: 30px;
    }

    .header h1 {
      font-size: 2.5rem;
      margin-bottom: 10px;
      color: #333;
    }

    .header p {
      font-size: 1.1rem;
      color: #666;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 20px;
      padding: 40px;
    }

    .branding-content {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 30px;
    }

    .branding-form-card {
      margin-bottom: 20px;
    }

    .form-section {
      margin-bottom: 30px;
    }

    .form-section h3 {
      margin: 0 0 15px 0;
      color: #333;
      font-size: 1.2rem;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    .form-row.three-columns {
      grid-template-columns: 1fr 1fr 1fr;
    }

    .half-width {
      width: 100%;
    }

    .third-width {
      width: 100%;
    }

    .full-width {
      width: 100%;
    }

    .form-actions {
      display: flex;
      gap: 15px;
      margin-top: 30px;
    }

    .preview-card {
      height: fit-content;
    }

    .channel-preview {
      border: 1px solid #ddd;
      border-radius: 8px;
      overflow: hidden;
      background: #f8f9fa;
    }

    .channel-banner {
      height: 120px;
      background-size: cover;
      background-position: center;
      background-color: #e0e0e0;
    }

    .channel-info {
      padding: 20px;
      display: flex;
      align-items: center;
      gap: 15px;
    }

    .channel-logo {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      object-fit: cover;
    }

    .channel-details h2 {
      margin: 0 0 10px 0;
      color: #333;
    }

    .channel-details p {
      margin: 0;
      color: #666;
      line-height: 1.4;
    }

    @media (max-width: 768px) {
      .branding-content {
        grid-template-columns: 1fr;
      }
      
      .form-row {
        grid-template-columns: 1fr;
      }
      
      .form-actions {
        flex-direction: column;
      }
    }
  `]
})
export class ChannelBrandingComponent implements OnInit {
  brandingForm: FormGroup;
  branding: ChannelBranding | null = null;
  isLoading = false;
  isSubmitting = false;
  channelId: number;

  constructor(
    private fb: FormBuilder,
    private brandingService: ChannelBrandingService,
    private snackBar: MatSnackBar,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.brandingForm = this.fb.group({
      logoUrl: [''],
      bannerUrl: [''],
      primaryColor: ['#FF5722'],
      secondaryColor: ['#2196F3'],
      accentColor: ['#4CAF50'],
      customDomain: [''],
      aboutText: [''],
      websiteUrl: [''],
      socialMediaLinks: [''],
      customCss: ['']
    });

    this.channelId = +this.route.snapshot.paramMap.get('id')!;
  }

  ngOnInit() {
    this.loadBranding();
  }

  loadBranding() {
    this.isLoading = true;
    this.brandingService.getChannelBranding(this.channelId).subscribe({
      next: (branding) => {
        this.branding = branding;
        if (branding) {
          this.brandingForm.patchValue(branding);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading branding:', error);
        this.snackBar.open('Failed to load branding settings', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  onSubmit() {
    if (this.brandingForm.valid) {
      this.isSubmitting = true;
      const formData = this.brandingForm.value;

      if (this.branding) {
        this.brandingService.updateChannelBranding(this.channelId, formData).subscribe({
          next: (branding) => {
            this.branding = branding;
            this.snackBar.open('Branding updated successfully', 'Close', { duration: 3000 });
            this.isSubmitting = false;
          },
          error: (error) => {
            console.error('Error updating branding:', error);
            this.snackBar.open('Failed to update branding', 'Close', { duration: 3000 });
            this.isSubmitting = false;
          }
        });
      } else {
        this.brandingService.createChannelBranding(this.channelId, formData).subscribe({
          next: (branding) => {
            this.branding = branding;
            this.snackBar.open('Branding created successfully', 'Close', { duration: 3000 });
            this.isSubmitting = false;
          },
          error: (error) => {
            console.error('Error creating branding:', error);
            this.snackBar.open('Failed to create branding', 'Close', { duration: 3000 });
            this.isSubmitting = false;
          }
        });
      }
    }
  }

  toggleActive() {
    if (this.branding) {
      this.isSubmitting = true;
      if (this.branding.isActive) {
        this.brandingService.deactivateChannelBranding(this.channelId).subscribe({
          next: () => {
            this.branding!.isActive = false;
            this.snackBar.open('Branding deactivated', 'Close', { duration: 3000 });
            this.isSubmitting = false;
          },
          error: (error) => {
            console.error('Error deactivating branding:', error);
            this.snackBar.open('Failed to deactivate branding', 'Close', { duration: 3000 });
            this.isSubmitting = false;
          }
        });
      } else {
        this.brandingService.activateChannelBranding(this.channelId).subscribe({
          next: () => {
            this.branding!.isActive = true;
            this.snackBar.open('Branding activated', 'Close', { duration: 3000 });
            this.isSubmitting = false;
          },
          error: (error) => {
            console.error('Error activating branding:', error);
            this.snackBar.open('Failed to activate branding', 'Close', { duration: 3000 });
            this.isSubmitting = false;
          }
        });
      }
    }
  }

  deleteBranding() {
    if (confirm('Are you sure you want to delete the branding settings?')) {
      this.isSubmitting = true;
      this.brandingService.deleteChannelBranding(this.channelId).subscribe({
        next: () => {
          this.branding = null;
          this.brandingForm.reset();
          this.snackBar.open('Branding deleted successfully', 'Close', { duration: 3000 });
          this.isSubmitting = false;
        },
        error: (error) => {
          console.error('Error deleting branding:', error);
          this.snackBar.open('Failed to delete branding', 'Close', { duration: 3000 });
          this.isSubmitting = false;
        }
      });
    }
  }

  getPreviewStyles(): string {
    if (!this.branding) return '';

    let styles = '';
    if (this.branding.primaryColor) {
      styles += `--primary-color: ${this.branding.primaryColor};`;
    }
    if (this.branding.secondaryColor) {
      styles += `--secondary-color: ${this.branding.secondaryColor};`;
    }
    if (this.branding.accentColor) {
      styles += `--accent-color: ${this.branding.accentColor};`;
    }
    if (this.branding.customCss) {
      styles += this.branding.customCss;
    }

    return styles;
  }
}
