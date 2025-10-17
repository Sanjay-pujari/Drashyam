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
import { ChannelCreate } from '../../models/video.model';

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
    public router: Router
  ) {
    this.channelForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      type: ['Personal', Validators.required],
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
          console.log('Channel created successfully:', channel);
          
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
                  console.log('Uploading banner...');
                  this.uploadBannerWithRetry(channel.id, this.selectedBanner!, 3);
                }, 1000); // 1 second delay
              },
              error: (err) => {
                console.error('Failed to validate token:', err);
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
          console.error('Failed to create channel:', err);
          if (err.status === 401) {
            alert('Authentication expired. Please log in again.');
            this.router.navigate(['/login']);
          } else {
            alert('Failed to create channel. Please try again.');
          }
        }
      });
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  private uploadBannerWithRetry(channelId: number, bannerFile: File, retries: number): void {
    console.log(`Attempting banner upload for channel ${channelId}, file: ${bannerFile.name}, retries left: ${retries}`);
    
    this.channelService.updateChannelBanner(channelId, bannerFile).subscribe({
      next: (result) => {
        console.log('Banner uploaded successfully:', result);
        // Upload profile picture if selected
        if (this.selectedProfilePicture) {
          console.log('Uploading profile picture...');
          this.channelService.updateChannelProfilePicture(channelId, this.selectedProfilePicture).subscribe({
            next: () => {
              console.log('Profile picture uploaded successfully');
              this.router.navigate(['/channels', channelId]);
            },
            error: (err) => {
              console.error('Failed to upload profile picture:', err);
              this.router.navigate(['/channels', channelId]);
            }
          });
        } else {
          this.router.navigate(['/channels', channelId]);
        }
      },
      error: (err) => {
        console.error(`Failed to upload banner (${retries} retries left):`, err);
        console.error('Error details:', {
          status: err.status,
          statusText: err.statusText,
          message: err.message,
          error: err.error
        });
        
        if (err.status === 401) {
          alert('Authentication expired. Please log in again.');
          this.router.navigate(['/login']);
        } else if (retries > 0) {
          // Retry with exponential backoff
          const delay = Math.pow(2, 3 - retries) * 1000; // 1s, 2s, 4s
          console.log(`Retrying banner upload in ${delay}ms...`);
          setTimeout(() => {
            this.uploadBannerWithRetry(channelId, bannerFile, retries - 1);
          }, delay);
        } else {
          console.error('All retry attempts exhausted for banner upload');
          alert('Channel created but failed to upload banner after multiple attempts. Please try updating it later.');
          this.router.navigate(['/channels', channelId]);
        }
      }
    });
  }
}
