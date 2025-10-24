import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription } from 'rxjs';
import { RecommendationService, UserPreference } from '../../services/recommendation.service';

@Component({
  selector: 'app-recommendation-preferences',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="preferences-container">
      <div class="preferences-header">
        <h2>Recommendation Preferences</h2>
        <p>Customize your video recommendations to discover content you'll love</p>
      </div>

      <mat-card class="preferences-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>tune</mat-icon>
            Content Preferences
          </mat-card-title>
          <mat-card-subtitle>Tell us what you're interested in</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="preferencesForm" (ngSubmit)="savePreferences()">
            <!-- Categories -->
            <div class="preference-section">
              <h3>Favorite Categories</h3>
              <p>Select categories you're most interested in</p>
              <mat-chip-listbox formControlName="categories" multiple>
                <mat-chip-option *ngFor="let category of availableCategories" [value]="category">
                  <mat-icon>{{ getCategoryIcon(category) }}</mat-icon>
                  {{ category }}
                </mat-chip-option>
              </mat-chip-listbox>
            </div>

            <!-- Tags -->
            <div class="preference-section">
              <h3>Interest Tags</h3>
              <p>Add specific topics or tags you're interested in</p>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Add interest tags</mat-label>
                <input matInput 
                       placeholder="e.g., cooking, travel, programming" 
                       (keyup.enter)="addTag($event)"
                       #tagInput>
                <mat-hint>Press Enter to add a tag</mat-hint>
              </mat-form-field>
              <div class="tags-container" *ngIf="selectedTags.length > 0">
                <mat-chip *ngFor="let tag of selectedTags; let i = index" 
                          [removable]="true" 
                          (removed)="removeTag(i)">
                  {{ tag }}
                  <mat-icon matChipRemove>cancel</mat-icon>
                </mat-chip>
              </div>
            </div>

            <!-- Recommendation Settings -->
            <div class="preference-section">
              <h3>Recommendation Settings</h3>
              <div class="setting-row">
                <div class="setting-info">
                  <h4>Include Trending Videos</h4>
                  <p>Show trending videos in your recommendations</p>
                </div>
                <mat-slide-toggle formControlName="includeTrending">
                  {{ preferencesForm.get('includeTrending')?.value ? 'Enabled' : 'Disabled' }}
                </mat-slide-toggle>
              </div>

              <div class="setting-row">
                <div class="setting-info">
                  <h4>Include Personalized Recommendations</h4>
                  <p>Show videos based on your viewing history and preferences</p>
                </div>
                <mat-slide-toggle formControlName="includePersonalized">
                  {{ preferencesForm.get('includePersonalized')?.value ? 'Enabled' : 'Disabled' }}
                </mat-slide-toggle>
              </div>

              <div class="setting-row">
                <div class="setting-info">
                  <h4>Include Similar User Recommendations</h4>
                  <p>Show videos liked by users with similar tastes</p>
                </div>
                <mat-slide-toggle formControlName="includeCollaborative">
                  {{ preferencesForm.get('includeCollaborative')?.value ? 'Enabled' : 'Disabled' }}
                </mat-slide-toggle>
              </div>
            </div>

            <!-- Recommendation Frequency -->
            <div class="preference-section">
              <h3>Update Frequency</h3>
              <p>How often should we update your recommendations?</p>
              <mat-form-field appearance="outline">
                <mat-label>Update Frequency</mat-label>
                <mat-select formControlName="updateFrequency">
                  <mat-option value="realtime">Real-time (immediate updates)</mat-option>
                  <mat-option value="hourly">Hourly</mat-option>
                  <mat-option value="daily">Daily</mat-option>
                  <mat-option value="weekly">Weekly</mat-option>
                </mat-select>
              </mat-form-field>
            </div>

            <!-- Privacy Settings -->
            <div class="preference-section">
              <h3>Privacy Settings</h3>
              <div class="setting-row">
                <div class="setting-info">
                  <h4>Allow Data Collection</h4>
                  <p>Allow us to collect data to improve recommendations</p>
                </div>
                <mat-slide-toggle formControlName="allowDataCollection">
                  {{ preferencesForm.get('allowDataCollection')?.value ? 'Enabled' : 'Disabled' }}
                </mat-slide-toggle>
              </div>
            </div>

            <div class="form-actions">
              <button mat-raised-button color="primary" type="submit" [disabled]="isSaving">
                <mat-icon *ngIf="isSaving">hourglass_empty</mat-icon>
                <mat-icon *ngIf="!isSaving">save</mat-icon>
                {{ isSaving ? 'Saving...' : 'Save Preferences' }}
              </button>
              <button mat-button type="button" (click)="resetPreferences()">
                <mat-icon>refresh</mat-icon>
                Reset
              </button>
              <button mat-button type="button" (click)="clearAllData()" color="warn">
                <mat-icon>delete_forever</mat-icon>
                Clear All Data
              </button>
              <button mat-button type="button" (click)="testApiConnection()" color="accent">
                <mat-icon>wifi</mat-icon>
                Test API
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>

      <!-- Current Preferences Display -->
      <mat-card class="current-preferences-card" *ngIf="currentPreferences.length > 0">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>history</mat-icon>
            Current Preferences
          </mat-card-title>
          <mat-card-subtitle>Your saved recommendation preferences</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div class="preferences-list">
            <div class="preference-item" *ngFor="let preference of currentPreferences">
              <div class="preference-info">
                <mat-icon>{{ preference.category ? getCategoryIcon(preference.category) : 'tag' }}</mat-icon>
                <span class="preference-text">
                  {{ preference.category || preference.tag }}
                </span>
                <span class="preference-weight">Weight: {{ preference.weight }}</span>
              </div>
              <button mat-icon-button (click)="removePreference(preference.id)">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .preferences-container {
      max-width: 800px;
      margin: 0 auto;
      padding: 2rem;
    }

    .preferences-header {
      text-align: center;
      margin-bottom: 2rem;
    }

    .preferences-header h2 {
      color: #333;
      margin-bottom: 0.5rem;
    }

    .preferences-header p {
      color: #666;
      margin: 0;
    }

    .preferences-card,
    .current-preferences-card {
      margin-bottom: 2rem;
    }

    .preference-section {
      margin-bottom: 2rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid #e0e0e0;
    }

    .preference-section:last-child {
      border-bottom: none;
    }

    .preference-section h3 {
      color: #333;
      margin-bottom: 0.5rem;
    }

    .preference-section p {
      color: #666;
      margin-bottom: 1rem;
    }

    .setting-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 0;
      border-bottom: 1px solid #f0f0f0;
    }

    .setting-row:last-child {
      border-bottom: none;
    }

    .setting-info h4 {
      margin: 0 0 0.25rem 0;
      color: #333;
    }

    .setting-info p {
      margin: 0;
      color: #666;
      font-size: 14px;
    }

    .full-width {
      width: 100%;
    }

    .tags-container {
      margin-top: 1rem;
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .form-actions {
      display: flex;
      gap: 1rem;
      margin-top: 2rem;
    }

    .preferences-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .preference-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem;
      background: #f5f5f5;
      border-radius: 4px;
    }

    .preference-info {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .preference-text {
      font-weight: 500;
    }

    .preference-weight {
      color: #666;
      font-size: 12px;
      margin-left: 0.5rem;
    }

    @media (max-width: 768px) {
      .preferences-container {
        padding: 1rem;
      }

      .setting-row {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }

      .form-actions {
        flex-direction: column;
      }
    }
  `]
})
export class RecommendationPreferencesComponent implements OnInit, OnDestroy {
  preferencesForm: FormGroup;
  currentPreferences: UserPreference[] = [];
  selectedTags: string[] = [];
  availableCategories: string[] = [
    'Gaming', 'Music', 'Sports', 'Education', 'Entertainment', 
    'Technology', 'Lifestyle', 'News', 'Comedy', 'Science', 
    'Travel', 'Food', 'Fashion', 'Art', 'History'
  ];
  
  isSaving = false;
  private subscriptions: Subscription[] = [];

  constructor(
    private fb: FormBuilder,
    private recommendationService: RecommendationService,
    private snackBar: MatSnackBar
  ) {
    this.preferencesForm = this.fb.group({
      categories: [[]],
      includeTrending: [true],
      includePersonalized: [true],
      includeCollaborative: [true],
      updateFrequency: ['daily'],
      allowDataCollection: [true]
    });
  }

  ngOnInit(): void {
    this.loadCurrentPreferences();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadCurrentPreferences(): void {
    this.subscriptions.push(
      this.recommendationService.getPreferences().subscribe({
        next: (preferences) => {
          this.currentPreferences = preferences;
          this.updateFormFromPreferences(preferences);
        },
        error: (error) => {
          console.error('Error loading preferences:', error);
          this.snackBar.open('Failed to load preferences', 'Close', { duration: 3000 });
        }
      })
    );
  }

  updateFormFromPreferences(preferences: UserPreference[]): void {
    const categories = preferences
      .filter(p => p.category)
      .map(p => p.category!);
    
    const tags = preferences
      .filter(p => p.tag)
      .map(p => p.tag!);

    this.preferencesForm.patchValue({
      categories: categories
    });

    this.selectedTags = tags;
  }

  addTag(event: Event): void {
    const input = event.target as HTMLInputElement;
    const tag = input.value.trim();
    
    if (tag && !this.selectedTags.includes(tag)) {
      this.selectedTags.push(tag);
      input.value = '';
    }
  }

  removeTag(index: number): void {
    this.selectedTags.splice(index, 1);
  }

  savePreferences(): void {
    console.log('Form valid:', this.preferencesForm.valid);
    console.log('Form value:', this.preferencesForm.value);
    console.log('Selected tags:', this.selectedTags);
    
    if (this.preferencesForm.valid) {
      this.isSaving = true;
      
      const formValue = this.preferencesForm.value;
      const preferences: UserPreference[] = [];

      // Add category preferences
      if (formValue.categories && formValue.categories.length > 0) {
        formValue.categories.forEach((category: string) => {
          preferences.push({
            id: 0,
            category: category,
            weight: 1.0,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString()
          });
        });
      }

      // Add tag preferences
      if (this.selectedTags && this.selectedTags.length > 0) {
        this.selectedTags.forEach(tag => {
          preferences.push({
            id: 0,
            tag: tag,
            weight: 1.0,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString()
          });
        });
      }

      console.log('Saving preferences:', preferences);

      this.subscriptions.push(
        this.recommendationService.updatePreferences(preferences).subscribe({
          next: (response) => {
            console.log('Preferences saved successfully:', response);
            this.snackBar.open('Preferences saved successfully!', 'Close', { duration: 3000 });
            this.isSaving = false;
            this.loadCurrentPreferences();
            
            // Emit event to notify other components that preferences have been updated
            this.onPreferencesUpdated();
          },
          error: (error) => {
            console.error('Error saving preferences:', error);
            console.error('Error details:', {
              status: error.status,
              statusText: error.statusText,
              message: error.message,
              url: error.url
            });
            this.snackBar.open('Failed to save preferences: ' + (error.message || 'Unknown error'), 'Close', { duration: 5000 });
            this.isSaving = false;
          }
        })
      );
    } else {
      console.error('Form is invalid:', this.preferencesForm.errors);
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
    }
  }

  resetPreferences(): void {
    this.preferencesForm.reset({
      categories: [],
      includeTrending: true,
      includePersonalized: true,
      includeCollaborative: true,
      updateFrequency: 'daily',
      allowDataCollection: true
    });
    this.selectedTags = [];
  }

  clearAllData(): void {
    if (confirm('Are you sure you want to clear all recommendation data? This action cannot be undone.')) {
      this.subscriptions.push(
        this.recommendationService.updatePreferences([]).subscribe({
          next: () => {
            this.snackBar.open('All recommendation data cleared', 'Close', { duration: 3000 });
            this.loadCurrentPreferences();
          },
          error: (error) => {
            console.error('Error clearing data:', error);
            this.snackBar.open('Failed to clear data', 'Close', { duration: 3000 });
          }
        })
      );
    }
  }

  removePreference(preferenceId: number): void {
    const updatedPreferences = this.currentPreferences.filter(p => p.id !== preferenceId);
    
    this.subscriptions.push(
      this.recommendationService.updatePreferences(updatedPreferences).subscribe({
        next: () => {
          this.snackBar.open('Preference removed', 'Close', { duration: 2000 });
          this.loadCurrentPreferences();
        },
        error: (error) => {
          console.error('Error removing preference:', error);
          this.snackBar.open('Failed to remove preference', 'Close', { duration: 3000 });
        }
      })
    );
  }

  getCategoryIcon(category: string): string {
    const iconMap: { [key: string]: string } = {
      'Gaming': 'sports_esports',
      'Music': 'music_note',
      'Sports': 'sports',
      'Education': 'school',
      'Entertainment': 'movie',
      'Technology': 'computer',
      'Lifestyle': 'favorite',
      'News': 'newspaper',
      'Comedy': 'sentiment_very_satisfied',
      'Science': 'science',
      'Travel': 'flight_takeoff',
      'Food': 'restaurant',
      'Fashion': 'checkroom',
      'Art': 'palette',
      'History': 'history_edu'
    };
    return iconMap[category] || 'category';
  }

  private onPreferencesUpdated(): void {
    // Dispatch a custom event to notify other components
    const event = new CustomEvent('preferencesUpdated', {
      detail: { timestamp: new Date().toISOString() }
    });
    window.dispatchEvent(event);
    
    console.log('Preferences updated event dispatched');
  }

  // Test method to verify API connectivity
  testApiConnection(): void {
    console.log('Testing API connection...');
    this.subscriptions.push(
      this.recommendationService.getPreferences().subscribe({
        next: (preferences) => {
          console.log('API connection successful, current preferences:', preferences);
          this.snackBar.open('API connection successful!', 'Close', { duration: 2000 });
        },
        error: (error) => {
          console.error('API connection failed:', error);
          this.snackBar.open('API connection failed: ' + error.message, 'Close', { duration: 5000 });
        }
      })
    );
  }
}
