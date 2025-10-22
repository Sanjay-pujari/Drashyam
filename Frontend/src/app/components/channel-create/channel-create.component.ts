import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router } from '@angular/router';
import { ChannelService } from '../../services/channel.service';
import { AuthService } from '../../services/auth.service';
import { QuotaService } from '../../services/quota.service';
import { ChannelCreate } from '../../models/channel.model';

@Component({
  selector: 'app-channel-create',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatSelectModule, MatCardModule, MatIconModule, MatProgressBarModule
  ],
  templateUrl: './channel-create.component.html',
  styleUrls: ['./channel-create.component.scss']
})
export class ChannelCreateComponent implements OnInit {
  channelForm: FormGroup;
  isCreating = false;
  selectedBanner: File | null = null;
  selectedProfilePicture: File | null = null;

  constructor(
    private fb: FormBuilder,
    private channelService: ChannelService,
    private authService: AuthService,
    private quotaService: QuotaService,
    public router: Router
  ) {
    this.channelForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      type: [0, Validators.required], // 0 = Personal, 1 = Business, 2 = Creator, 3 = Brand
      customUrl: ['', [Validators.pattern(/^[a-zA-Z0-9-_]+$/)]],
      websiteUrl: ['', [Validators.pattern(/^https?:\/\/.+/)]],
      socialLinks: ['']
    });
  }

  ngOnInit() {
    // Generate suggested custom URL based on name
    this.channelForm.get('name')?.valueChanges.subscribe(name => {
      if (name && !this.channelForm.get('customUrl')?.touched) {
        const suggestedUrl = name.toLowerCase()
          .replace(/[^a-zA-Z0-9]/g, '-')
          .replace(/-+/g, '-')
          .replace(/^-|-$/g, '');
        this.channelForm.get('customUrl')?.setValue(suggestedUrl);
      }
    });
  }

  onBannerSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedBanner = file;
    }
  }

  onProfilePictureSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedProfilePicture = file;
    }
  }

  onSubmit() {
    if (this.channelForm.valid) {
      // Check quota before creating channel
      this.checkQuotaBeforeCreate();
    }
  }

  private checkQuotaBeforeCreate() {
    this.quotaService.canCreateChannel().subscribe({
      next: (canCreate) => {
        if (canCreate) {
          this.proceedWithChannelCreation();
        } else {
          alert('Channel quota exceeded. Please upgrade your plan to create more channels.');
        }
      },
      error: (error) => {
        console.error('Quota check failed:', error);
        alert('Unable to check quota. Please try again.');
      }
    });
  }

  private proceedWithChannelCreation() {
    this.isCreating = true;
      
    const channelData: ChannelCreate = {
      name: this.channelForm.value.name,
      description: this.channelForm.value.description,
      type: this.channelForm.value.type,
      customUrl: this.channelForm.value.customUrl,
      websiteUrl: this.channelForm.value.websiteUrl,
      socialLinks: this.channelForm.value.socialLinks
    };

    // Check if banner is required
    if (!this.selectedBanner) {
      alert('Banner image is required. Please select a banner image for your channel.');
      this.isCreating = false;
      return;
    }

    this.channelService.createChannel(channelData).subscribe({
      next: (channel) => {
        // Upload banner if selected
        if (this.selectedBanner) {
          // Ensure token is valid before uploading
          this.authService.ensureValidToken().subscribe({
            next: (isValid) => {
              if (!isValid) {
                alert('Authentication expired. Please log in again.');
                this.router.navigate(['/login']);
                return;
              }
              
              // Add a small delay to ensure the channel is fully created
              setTimeout(() => {
                this.uploadBannerWithRetry(channel.id, this.selectedBanner!, 3);
              }, 1000); // 1 second delay
            },
            error: (err) => {
              alert('Authentication error. Please log in again.');
              this.router.navigate(['/login']);
            }
          });
        } else {
          this.router.navigate(['/channels', channel.id]);
        }
      },
      error: (err) => {
        this.isCreating = false;
        if (err.status === 401) {
          alert('Authentication expired. Please log in again.');
          this.router.navigate(['/login']);
        } else {
          alert('Failed to create channel. Please try again.');
        }
      }
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  private uploadBannerWithRetry(channelId: number, bannerFile: File, retries: number): void {
    
    this.channelService.updateChannelBanner(channelId, bannerFile).subscribe({
      next: (result) => {
        // Upload profile picture if selected
        if (this.selectedProfilePicture) {
          this.channelService.updateChannelProfilePicture(channelId, this.selectedProfilePicture).subscribe({
            next: () => {
              this.router.navigate(['/channels', channelId]);
            },
            error: (err) => {
              this.router.navigate(['/channels', channelId]);
            }
          });
        } else {
          this.router.navigate(['/channels', channelId]);
        }
      },
      error: (err) => {
        
        if (err.status === 401) {
          alert('Authentication expired. Please log in again.');
          this.router.navigate(['/login']);
        } else if (retries > 0) {
          // Retry with exponential backoff
          const delay = Math.pow(2, 3 - retries) * 1000; // 1s, 2s, 4s
          setTimeout(() => {
            this.uploadBannerWithRetry(channelId, bannerFile, retries - 1);
          }, delay);
        } else {
          alert('Channel created but failed to upload banner after multiple attempts. Please try updating it later.');
          this.router.navigate(['/channels', channelId]);
        }
      }
    });
  }
}
