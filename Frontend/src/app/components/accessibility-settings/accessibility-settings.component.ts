import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AccessibilityService, AccessibilitySettings } from '../../services/accessibility.service';

@Component({
  selector: 'app-accessibility-settings',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDividerModule,
    MatTooltipModule,
    MatExpansionModule
  ],
  template: `
    <div class="accessibility-settings">
      <mat-card class="settings-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>accessibility</mat-icon>
            Accessibility Settings
          </mat-card-title>
          <mat-card-subtitle>
            Customize your experience for better accessibility
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <!-- Motion Settings -->
          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>
                <mat-icon>motion_photos_on</mat-icon>
                Motion & Animation
              </mat-panel-title>
            </mat-expansion-panel-header>
            
            <div class="setting-group">
              <div class="setting-item">
                <mat-slide-toggle
                  [checked]="settings.reducedMotion"
                  (change)="updateSetting('reducedMotion', $event.checked)"
                  [attr.aria-label]="'Reduce motion and animations'">
                  <div class="setting-content">
                    <span class="setting-label">Reduce Motion</span>
                    <span class="setting-description">
                      Minimize animations and transitions for users with vestibular disorders
                    </span>
                  </div>
                </mat-slide-toggle>
              </div>
            </div>
          </mat-expansion-panel>

          <!-- Visual Settings -->
          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>
                <mat-icon>visibility</mat-icon>
                Visual Settings
              </mat-panel-title>
            </mat-expansion-panel-header>
            
            <div class="setting-group">
              <div class="setting-item">
                <mat-slide-toggle
                  [checked]="settings.highContrast"
                  (change)="updateSetting('highContrast', $event.checked)"
                  [attr.aria-label]="'Enable high contrast mode'">
                  <div class="setting-content">
                    <span class="setting-label">High Contrast</span>
                    <span class="setting-description">
                      Increase contrast for better visibility
                    </span>
                  </div>
                </mat-slide-toggle>
              </div>

              <div class="setting-item">
                <mat-slide-toggle
                  [checked]="settings.largeText"
                  (change)="updateSetting('largeText', $event.checked)"
                  [attr.aria-label]="'Enable large text mode'">
                  <div class="setting-content">
                    <span class="setting-label">Large Text</span>
                    <span class="setting-description">
                      Increase text size for better readability
                    </span>
                  </div>
                </mat-slide-toggle>
              </div>
            </div>
          </mat-expansion-panel>

          <!-- Navigation Settings -->
          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>
                <mat-icon>keyboard</mat-icon>
                Navigation
              </mat-panel-title>
            </mat-expansion-panel-header>
            
            <div class="setting-group">
              <div class="setting-item">
                <mat-slide-toggle
                  [checked]="settings.keyboardNavigation"
                  (change)="updateSetting('keyboardNavigation', $event.checked)"
                  [attr.aria-label]="'Enable enhanced keyboard navigation'">
                  <div class="setting-content">
                    <span class="setting-label">Enhanced Keyboard Navigation</span>
                    <span class="setting-description">
                      Improved keyboard navigation with arrow keys and shortcuts
                    </span>
                  </div>
                </mat-slide-toggle>
              </div>

              <div class="setting-item">
                <mat-slide-toggle
                  [checked]="settings.focusVisible"
                  (change)="updateSetting('focusVisible', $event.checked)"
                  [attr.aria-label]="'Show focus indicators'">
                  <div class="setting-content">
                    <span class="setting-label">Focus Indicators</span>
                    <span class="setting-description">
                      Always show focus indicators for keyboard navigation
                    </span>
                  </div>
                </mat-slide-toggle>
              </div>
            </div>
          </mat-expansion-panel>

          <!-- Screen Reader Settings -->
          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>
                <mat-icon>screen_reader</mat-icon>
                Screen Reader
              </mat-panel-title>
            </mat-expansion-panel-header>
            
            <div class="setting-group">
              <div class="setting-item">
                <mat-slide-toggle
                  [checked]="settings.screenReader"
                  (change)="updateSetting('screenReader', $event.checked)"
                  [attr.aria-label]="'Enable screen reader optimizations'">
                  <div class="setting-content">
                    <span class="setting-label">Screen Reader Optimizations</span>
                    <span class="setting-description">
                      Optimize interface for screen readers and assistive technologies
                    </span>
                  </div>
                </mat-slide-toggle>
              </div>
            </div>
          </mat-expansion-panel>

          <mat-divider></mat-divider>

          <!-- Quick Actions -->
          <div class="quick-actions">
            <h3>Quick Actions</h3>
            
            <div class="action-buttons">
              <button 
                mat-button 
                (click)="resetToDefaults()"
                [attr.aria-label]="'Reset all accessibility settings to defaults'">
                <mat-icon>refresh</mat-icon>
                Reset to Defaults
              </button>

              <button 
                mat-button 
                (click)="testScreenReader()"
                [attr.aria-label]="'Test screen reader announcement'">
                <mat-icon>volume_up</mat-icon>
                Test Screen Reader
              </button>

              <button 
                mat-button 
                (click)="showKeyboardShortcuts()"
                [attr.aria-label]="'Show keyboard shortcuts help'">
                <mat-icon>help</mat-icon>
                Keyboard Shortcuts
              </button>
            </div>
          </div>

          <!-- Accessibility Info -->
          <div class="accessibility-info">
            <h3>Accessibility Features</h3>
            <ul>
              <li>WCAG 2.1 AA compliant color schemes</li>
              <li>Keyboard navigation support</li>
              <li>Screen reader optimization</li>
              <li>High contrast mode</li>
              <li>Reduced motion support</li>
              <li>Focus management</li>
            </ul>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .accessibility-settings {
      max-width: 800px;
      margin: 0 auto;
      padding: 24px;
    }

    .settings-card {
      background-color: var(--color-surface);
      color: var(--color-on-surface);
    }

    .settings-card mat-card-header {
      margin-bottom: 24px;
    }

    .settings-card mat-card-title {
      display: flex;
      align-items: center;
      gap: 12px;
      font-size: 1.5rem;
      font-weight: 500;
    }

    .settings-card mat-card-subtitle {
      color: var(--color-on-surface-variant);
      font-size: 1rem;
    }

    .setting-group {
      padding: 16px 0;
    }

    .setting-item {
      margin-bottom: 16px;
    }

    .setting-item:last-child {
      margin-bottom: 0;
    }

    .setting-content {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .setting-label {
      font-weight: 500;
      font-size: 1rem;
    }

    .setting-description {
      font-size: 0.9rem;
      color: var(--color-on-surface-variant);
      line-height: 1.4;
    }

    .quick-actions {
      margin-top: 24px;
    }

    .quick-actions h3 {
      margin: 0 0 16px 0;
      font-size: 1.1rem;
      font-weight: 500;
    }

    .action-buttons {
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
    }

    .action-buttons button {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .accessibility-info {
      margin-top: 24px;
      padding: 16px;
      background-color: var(--color-surface-variant);
      border-radius: var(--border-radius-md);
    }

    .accessibility-info h3 {
      margin: 0 0 12px 0;
      font-size: 1.1rem;
      font-weight: 500;
    }

    .accessibility-info ul {
      margin: 0;
      padding-left: 20px;
    }

    .accessibility-info li {
      margin-bottom: 4px;
      font-size: 0.9rem;
      color: var(--color-on-surface-variant);
    }

    /* Focus styles */
    .setting-item:focus-within {
      outline: 2px solid var(--color-primary);
      outline-offset: 2px;
      border-radius: var(--border-radius-sm);
    }

    /* High contrast adjustments */
    [data-high-contrast="true"] .setting-item:focus-within {
      outline-width: 3px;
    }

    /* Responsive design */
    @media (max-width: 768px) {
      .accessibility-settings {
        padding: 16px;
      }

      .action-buttons {
        flex-direction: column;
      }

      .action-buttons button {
        width: 100%;
        justify-content: flex-start;
      }
    }
  `]
})
export class AccessibilitySettingsComponent implements OnInit, OnDestroy {
  settings: AccessibilitySettings = {
    reducedMotion: false,
    highContrast: false,
    largeText: false,
    screenReader: false,
    keyboardNavigation: true,
    focusVisible: true
  };

  private destroy$ = new Subject<void>();

  constructor(private accessibilityService: AccessibilityService) {}

  ngOnInit(): void {
    this.accessibilityService.settings$
      .pipe(takeUntil(this.destroy$))
      .subscribe(settings => {
        this.settings = settings;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  updateSetting<K extends keyof AccessibilitySettings>(
    key: K, 
    value: AccessibilitySettings[K]
  ): void {
    this.accessibilityService.updateSetting(key, value);
  }

  resetToDefaults(): void {
    this.accessibilityService.resetToDefaults();
  }

  testScreenReader(): void {
    this.accessibilityService.announceToScreenReader(
      'Accessibility settings test announcement. Screen reader is working correctly.',
      'assertive'
    );
  }

  showKeyboardShortcuts(): void {
    const shortcuts = [
      'Tab - Navigate between elements',
      'Enter/Space - Activate buttons and links',
      'Escape - Close modals and menus',
      'Arrow keys - Navigate within components',
      'Alt + T - Toggle theme',
      'Alt + A - Open accessibility settings'
    ];

    const message = 'Keyboard shortcuts: ' + shortcuts.join('. ');
    this.accessibilityService.announceToScreenReader(message, 'polite');
  }
}
