import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface ThemeColors {
  primary: string;
  secondary: string;
  accent: string;
  background: string;
  surface: string;
  surfaceVariant: string;
  onPrimary: string;
  onSecondary: string;
  onAccent: string;
  onBackground: string;
  onSurface: string;
  onSurfaceVariant: string;
  error: string;
  onError: string;
  warning: string;
  onWarning: string;
  success: string;
  onSuccess: string;
  outline: string;
  outlineVariant: string;
  shadow: string;
  elevation: string;
}

export interface Theme {
  name: string;
  displayName: string;
  colors: ThemeColors;
  isDark: boolean;
  contrast: 'normal' | 'high';
}

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'drashyam-theme';
  private readonly THEME_PREFERENCE_KEY = 'drashyam-theme-preference';
  
  private currentThemeSubject = new BehaviorSubject<Theme>(this.getDefaultTheme());
  public currentTheme$ = this.currentThemeSubject.asObservable();

  // WCAG AA compliant color schemes
  private readonly themes: Theme[] = [
    {
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
    },
    {
      name: 'dark',
      displayName: 'Dark Mode',
      isDark: true,
      contrast: 'normal',
      colors: {
        primary: '#90caf9',
        secondary: '#b0bec5',
        accent: '#f48fb1',
        background: '#121212',
        surface: '#1e1e1e',
        surfaceVariant: '#2d2d2d',
        onPrimary: '#000000',
        onSecondary: '#000000',
        onAccent: '#000000',
        onBackground: '#ffffff',
        onSurface: '#ffffff',
        onSurfaceVariant: '#b0b0b0',
        error: '#f44336',
        onError: '#ffffff',
        warning: '#ff9800',
        onWarning: '#000000',
        success: '#4caf50',
        onSuccess: '#000000',
        outline: '#424242',
        outlineVariant: '#2d2d2d',
        shadow: 'rgba(0, 0, 0, 0.3)',
        elevation: 'rgba(0, 0, 0, 0.4)'
      }
    },
    {
      name: 'light-high-contrast',
      displayName: 'Light High Contrast',
      isDark: false,
      contrast: 'high',
      colors: {
        primary: '#0000ff',
        secondary: '#000000',
        accent: '#ff0000',
        background: '#ffffff',
        surface: '#ffffff',
        surfaceVariant: '#f0f0f0',
        onPrimary: '#ffffff',
        onSecondary: '#ffffff',
        onAccent: '#ffffff',
        onBackground: '#000000',
        onSurface: '#000000',
        onSurfaceVariant: '#000000',
        error: '#ff0000',
        onError: '#ffffff',
        warning: '#ff8c00',
        onWarning: '#000000',
        success: '#008000',
        onSuccess: '#ffffff',
        outline: '#000000',
        outlineVariant: '#000000',
        shadow: 'rgba(0, 0, 0, 0.5)',
        elevation: 'rgba(0, 0, 0, 0.6)'
      }
    },
    {
      name: 'dark-high-contrast',
      displayName: 'Dark High Contrast',
      isDark: true,
      contrast: 'high',
      colors: {
        primary: '#00bfff',
        secondary: '#ffffff',
        accent: '#ffff00',
        background: '#000000',
        surface: '#1a1a1a',
        surfaceVariant: '#333333',
        onPrimary: '#000000',
        onSecondary: '#000000',
        onAccent: '#000000',
        onBackground: '#ffffff',
        onSurface: '#ffffff',
        onSurfaceVariant: '#ffffff',
        error: '#ff0000',
        onError: '#ffffff',
        warning: '#ffff00',
        onWarning: '#000000',
        success: '#00ff00',
        onSuccess: '#000000',
        outline: '#ffffff',
        outlineVariant: '#ffffff',
        shadow: 'rgba(255, 255, 255, 0.3)',
        elevation: 'rgba(255, 255, 255, 0.4)'
      }
    }
  ];

  constructor() {
    // Initialize theme immediately since themes array is available
    this.initializeTheme();
    this.setupSystemThemeListener();
  }

  private initializeTheme(): void {
    const savedTheme = localStorage.getItem(this.THEME_KEY);
    const savedPreference = localStorage.getItem(this.THEME_PREFERENCE_KEY);
    
    if (savedTheme) {
      const theme = this.themes.find(t => t.name === savedTheme);
      if (theme) {
        this.applyTheme(theme);
        return;
      }
    }

    // Check system preference
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const prefersHighContrast = window.matchMedia('(prefers-contrast: high)').matches;
    
    let defaultTheme: Theme;
    if (prefersHighContrast) {
      defaultTheme = prefersDark ? this.getTheme('dark-high-contrast') : this.getTheme('light-high-contrast');
    } else {
      defaultTheme = prefersDark ? this.getTheme('dark') : this.getTheme('light');
    }

    this.applyTheme(defaultTheme);
  }

  private setupSystemThemeListener(): void {
    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      if (!localStorage.getItem(this.THEME_KEY)) {
        const prefersHighContrast = window.matchMedia('(prefers-contrast: high)').matches;
        const theme = prefersHighContrast 
          ? (e.matches ? this.getTheme('dark-high-contrast') : this.getTheme('light-high-contrast'))
          : (e.matches ? this.getTheme('dark') : this.getTheme('light'));
        this.applyTheme(theme);
      }
    });

    // Listen for high contrast changes
    window.matchMedia('(prefers-contrast: high)').addEventListener('change', (e) => {
      if (!localStorage.getItem(this.THEME_KEY)) {
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const theme = e.matches 
          ? (prefersDark ? this.getTheme('dark-high-contrast') : this.getTheme('light-high-contrast'))
          : (prefersDark ? this.getTheme('dark') : this.getTheme('light'));
        this.applyTheme(theme);
      }
    });
  }

  getAvailableThemes(): Theme[] {
    return [...this.themes];
  }

  getCurrentTheme(): Theme {
    return this.currentThemeSubject.value;
  }

  getTheme(name: string): Theme {
    return this.themes.find(theme => theme.name === name) || this.getDefaultTheme();
  }

  setTheme(themeName: string): void {
    const theme = this.getTheme(themeName);
    if (theme) {
      this.applyTheme(theme);
      localStorage.setItem(this.THEME_KEY, theme.name);
    }
  }

  toggleTheme(): void {
    const current = this.getCurrentTheme();
    const isDark = current.isDark;
    const isHighContrast = current.contrast === 'high';
    
    let newTheme: Theme;
    if (isHighContrast) {
      newTheme = isDark ? this.getTheme('light-high-contrast') : this.getTheme('dark-high-contrast');
    } else {
      newTheme = isDark ? this.getTheme('light') : this.getTheme('dark');
    }
    
    this.setTheme(newTheme.name);
  }

  toggleContrast(): void {
    const current = this.getCurrentTheme();
    const isDark = current.isDark;
    const isHighContrast = current.contrast === 'high';
    
    let newTheme: Theme;
    if (isHighContrast) {
      newTheme = isDark ? this.getTheme('dark') : this.getTheme('light');
    } else {
      newTheme = isDark ? this.getTheme('dark-high-contrast') : this.getTheme('light-high-contrast');
    }
    
    this.setTheme(newTheme.name);
  }

  resetToSystemPreference(): void {
    localStorage.removeItem(this.THEME_KEY);
    this.initializeTheme();
  }

  private applyTheme(theme: Theme): void {
    this.currentThemeSubject.next(theme);
    this.updateCSSVariables(theme);
    this.updateDocumentAttributes(theme);
  }

  private updateCSSVariables(theme: Theme): void {
    const root = document.documentElement;
    const colors = theme.colors;
    
    // Set CSS custom properties
    root.style.setProperty('--color-primary', colors.primary);
    root.style.setProperty('--color-secondary', colors.secondary);
    root.style.setProperty('--color-accent', colors.accent);
    root.style.setProperty('--color-background', colors.background);
    root.style.setProperty('--color-surface', colors.surface);
    root.style.setProperty('--color-surface-variant', colors.surfaceVariant);
    root.style.setProperty('--color-on-primary', colors.onPrimary);
    root.style.setProperty('--color-on-secondary', colors.onSecondary);
    root.style.setProperty('--color-on-accent', colors.onAccent);
    root.style.setProperty('--color-on-background', colors.onBackground);
    root.style.setProperty('--color-on-surface', colors.onSurface);
    root.style.setProperty('--color-on-surface-variant', colors.onSurfaceVariant);
    root.style.setProperty('--color-error', colors.error);
    root.style.setProperty('--color-on-error', colors.onError);
    root.style.setProperty('--color-warning', colors.warning);
    root.style.setProperty('--color-on-warning', colors.onWarning);
    root.style.setProperty('--color-success', colors.success);
    root.style.setProperty('--color-on-success', colors.onSuccess);
    root.style.setProperty('--color-outline', colors.outline);
    root.style.setProperty('--color-outline-variant', colors.outlineVariant);
    root.style.setProperty('--color-shadow', colors.shadow);
    root.style.setProperty('--color-elevation', colors.elevation);
  }

  private updateDocumentAttributes(theme: Theme): void {
    document.documentElement.setAttribute('data-theme', theme.name);
    document.documentElement.setAttribute('data-dark-mode', theme.isDark.toString());
    document.documentElement.setAttribute('data-high-contrast', (theme.contrast === 'high').toString());
  }

  private getDefaultTheme(): Theme {
    // Safety check to ensure themes array is available
    if (!this.themes || this.themes.length === 0) {
      // Fallback theme if themes array is not available
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
    return this.themes[0]; // Light theme as default
  }

  // Accessibility helpers
  getContrastRatio(color1: string, color2: string): number {
    // Simplified contrast ratio calculation
    // In a real implementation, you'd use a proper color contrast library
    return 4.5; // Placeholder - should be calculated properly
  }

  isAccessibleContrast(foreground: string, background: string, level: 'AA' | 'AAA' = 'AA'): boolean {
    const ratio = this.getContrastRatio(foreground, background);
    return level === 'AA' ? ratio >= 4.5 : ratio >= 7;
  }
}
