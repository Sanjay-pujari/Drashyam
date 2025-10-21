import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatRadioModule } from '@angular/material/radio';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ThemeService, Theme } from '../../services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    MatSlideToggleModule,
    MatDividerModule,
    MatListModule,
    MatRadioModule,
    MatFormFieldModule,
    MatSelectModule
  ],
  template: `
    <div class="theme-toggle-container">
      <!-- Compact Toggle Button -->
      <button 
        mat-icon-button 
        [matTooltip]="getToggleTooltip()"
        [attr.aria-label]="getToggleAriaLabel()"
        (click)="toggleTheme()"
        class="theme-toggle-button"
        [class.dark-mode]="currentTheme.isDark">
        <mat-icon>{{ getToggleIcon() }}</mat-icon>
      </button>

      <!-- Full Theme Selector Menu -->
      <button 
        mat-icon-button 
        [matMenuTriggerFor]="themeMenu"
        [matTooltip]="'Theme Options'"
        aria-label="Open theme options menu"
        class="theme-options-button">
        <mat-icon>palette</mat-icon>
      </button>

      <mat-menu #themeMenu="matMenu" class="theme-menu">
        <div class="theme-menu-header">
          <h3>Theme Settings</h3>
        </div>

        <mat-divider></mat-divider>

        <!-- Theme Selection -->
        <div class="theme-selection">
          <h4>Color Scheme</h4>
          <mat-radio-group 
            [value]="currentTheme.name" 
            (change)="onThemeChange($event)"
            class="theme-radio-group">
            <mat-radio-button 
              *ngFor="let theme of availableThemes" 
              [value]="theme.name"
              [attr.aria-describedby]="'theme-desc-' + theme.name">
              <div class="theme-option">
                <div class="theme-preview" [class]="'preview-' + theme.name"></div>
                <div class="theme-info">
                  <span class="theme-name">{{ theme.displayName }}</span>
                  <span class="theme-description" [id]="'theme-desc-' + theme.name">
                    {{ getThemeDescription(theme) }}
                  </span>
                </div>
              </div>
            </mat-radio-button>
          </mat-radio-group>
        </div>

        <mat-divider></mat-divider>

        <!-- Quick Toggles -->
        <div class="quick-toggles">
          <h4>Quick Settings</h4>
          
          <div class="toggle-item">
            <mat-slide-toggle
              [checked]="currentTheme.isDark"
              (change)="toggleDarkMode()"
              [attr.aria-label]="'Toggle dark mode'">
              <span class="toggle-label">
                <mat-icon>dark_mode</mat-icon>
                Dark Mode
              </span>
            </mat-slide-toggle>
          </div>

          <div class="toggle-item">
            <mat-slide-toggle
              [checked]="currentTheme.contrast === 'high'"
              (change)="toggleHighContrast()"
              [attr.aria-label]="'Toggle high contrast mode'">
              <span class="toggle-label">
                <mat-icon>contrast</mat-icon>
                High Contrast
              </span>
            </mat-slide-toggle>
          </div>

          <div class="toggle-item">
            <button 
              mat-button 
              (click)="resetToSystemPreference()"
              [attr.aria-label]="'Reset to system preference'">
              <mat-icon>refresh</mat-icon>
              Reset to System
            </button>
          </div>
        </div>

        <mat-divider></mat-divider>

        <!-- Accessibility Info -->
        <div class="accessibility-info">
          <h4>Accessibility</h4>
          <p class="accessibility-text">
            Current theme meets WCAG {{ currentTheme.contrast === 'high' ? 'AAA' : 'AA' }} standards.
            High contrast mode provides enhanced visibility for users with visual impairments.
          </p>
        </div>
      </mat-menu>
    </div>
  `,
  styles: [`
    .theme-toggle-container {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .theme-toggle-button,
    .theme-options-button {
      transition: all 0.3s ease;
    }

    .theme-toggle-button:hover,
    .theme-options-button:hover {
      transform: scale(1.1);
    }

    .theme-toggle-button.dark-mode {
      background-color: var(--color-surface-variant);
    }

    .theme-menu {
      min-width: 320px;
      max-width: 400px;
    }

    .theme-menu-header {
      padding: 16px 16px 8px;
    }

    .theme-menu-header h3 {
      margin: 0;
      font-size: 1.1rem;
      font-weight: 500;
    }

    .theme-selection,
    .quick-toggles,
    .accessibility-info {
      padding: 16px;
    }

    .theme-selection h4,
    .quick-toggles h4,
    .accessibility-info h4 {
      margin: 0 0 12px 0;
      font-size: 0.9rem;
      font-weight: 500;
      color: var(--color-on-surface-variant);
    }

    .theme-radio-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .theme-option {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 8px;
    }

    .theme-preview {
      width: 32px;
      height: 32px;
      border-radius: 6px;
      border: 2px solid var(--color-outline);
      position: relative;
      overflow: hidden;
    }

    .theme-preview::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: linear-gradient(45deg, 
        var(--color-primary) 0%, 
        var(--color-secondary) 50%, 
        var(--color-accent) 100%);
    }

    .preview-light::before {
      background: linear-gradient(45deg, #1976d2 0%, #424242 50%, #ff4081 100%);
    }

    .preview-dark::before {
      background: linear-gradient(45deg, #90caf9 0%, #b0bec5 50%, #f48fb1 100%);
    }

    .preview-light-high-contrast::before {
      background: linear-gradient(45deg, #0000ff 0%, #000000 50%, #ff0000 100%);
    }

    .preview-dark-high-contrast::before {
      background: linear-gradient(45deg, #00bfff 0%, #ffffff 50%, #ffff00 100%);
    }

    .theme-info {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .theme-name {
      font-weight: 500;
      font-size: 0.9rem;
    }

    .theme-description {
      font-size: 0.8rem;
      color: var(--color-on-surface-variant);
    }

    .toggle-item {
      display: flex;
      align-items: center;
      padding: 8px 0;
    }

    .toggle-label {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 0.9rem;
    }

    .toggle-label mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    .accessibility-text {
      font-size: 0.8rem;
      color: var(--color-on-surface-variant);
      line-height: 1.4;
      margin: 0;
    }

    /* Focus styles for accessibility */
    .theme-toggle-button:focus,
    .theme-options-button:focus,
    button:focus {
      outline: 2px solid var(--color-primary);
      outline-offset: 2px;
    }

    /* High contrast mode adjustments */
    [data-high-contrast="true"] .theme-preview {
      border-width: 3px;
    }

    [data-high-contrast="true"] .theme-toggle-button:focus,
    [data-high-contrast="true"] .theme-options-button:focus {
      outline-width: 3px;
    }
  `]
})
export class ThemeToggleComponent implements OnInit, OnDestroy {
  currentTheme: Theme = this.getDefaultTheme();
  availableThemes: Theme[] = [];
  
  private destroy$ = new Subject<void>();

  constructor(private themeService: ThemeService) {}

  ngOnInit(): void {
    this.availableThemes = this.themeService.getAvailableThemes();
    
    this.themeService.currentTheme$
      .pipe(takeUntil(this.destroy$))
      .subscribe(theme => {
        this.currentTheme = theme;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  onThemeChange(event: any): void {
    this.themeService.setTheme(event.value);
  }

  toggleDarkMode(): void {
    this.themeService.toggleTheme();
  }

  toggleHighContrast(): void {
    this.themeService.toggleContrast();
  }

  resetToSystemPreference(): void {
    this.themeService.resetToSystemPreference();
  }

  getToggleIcon(): string {
    return this.currentTheme.isDark ? 'dark_mode' : 'light_mode';
  }

  getToggleTooltip(): string {
    return this.currentTheme.isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode';
  }

  getToggleAriaLabel(): string {
    return this.currentTheme.isDark ? 'Switch to light mode' : 'Switch to dark mode';
  }

  getThemeDescription(theme: Theme): string {
    const descriptions: { [key: string]: string } = {
      'light': 'Standard light theme with comfortable contrast',
      'dark': 'Dark theme for low-light environments',
      'light-high-contrast': 'High contrast light theme for better visibility',
      'dark-high-contrast': 'High contrast dark theme for accessibility'
    };
    return descriptions[theme.name] || 'Custom theme';
  }

  private getDefaultTheme(): Theme {
    return {
      name: 'light',
      displayName: 'Light Mode',
      isDark: false,
      contrast: 'normal',
      colors: {
        primary: '#1976d2',
        secondary: '#424242',
        accent: '#ff4081',
        background: '#ffffff',
        surface: '#ffffff',
        surfaceVariant: '#f5f5f5',
        onPrimary: '#ffffff',
        onSecondary: '#ffffff',
        onAccent: '#ffffff',
        onBackground: '#212121',
        onSurface: '#212121',
        onSurfaceVariant: '#424242',
        error: '#d32f2f',
        onError: '#ffffff',
        warning: '#f57c00',
        onWarning: '#ffffff',
        success: '#388e3c',
        onSuccess: '#ffffff',
        outline: '#e0e0e0',
        outlineVariant: '#f5f5f5',
        shadow: 'rgba(0, 0, 0, 0.1)',
        elevation: 'rgba(0, 0, 0, 0.12)'
      }
    };
  }
}
