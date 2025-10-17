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
        type: this.channelForm.value.type as any, // Cast to match backend enum
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
            this.channelService.updateChannelBanner(channel.id, this.selectedBanner).subscribe({
              next: () => {
                // Upload profile picture if selected
                if (this.selectedProfilePicture) {
                  this.channelService.updateChannelProfilePicture(channel.id, this.selectedProfilePicture).subscribe({
                    next: () => {
                      this.router.navigate(['/channels', channel.id]);
                    },
                    error: (err) => {
                      console.error('Failed to upload profile picture:', err);
                      this.router.navigate(['/channels', channel.id]);
                    }
                  });
                } else {
                  this.router.navigate(['/channels', channel.id]);
                }
              },
              error: (err) => {
                console.error('Failed to upload banner:', err);
                alert('Channel created but failed to upload banner. Please try updating it later.');
                this.router.navigate(['/channels', channel.id]);
              }
            });
          } else {
            this.router.navigate(['/channels', channel.id]);
          }
        },
        error: (err) => {
          this.isCreating = false;
          console.error('Failed to create channel:', err);
          alert('Failed to create channel. Please try again.');
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
}
