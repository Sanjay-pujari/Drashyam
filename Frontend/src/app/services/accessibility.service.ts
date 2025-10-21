import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface AccessibilitySettings {
  reducedMotion: boolean;
  highContrast: boolean;
  largeText: boolean;
  screenReader: boolean;
  keyboardNavigation: boolean;
  focusVisible: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AccessibilityService {
  private settingsSubject = new BehaviorSubject<AccessibilitySettings>(this.getDefaultSettings());
  public settings$ = this.settingsSubject.asObservable();

  private readonly SETTINGS_KEY = 'drashyam-accessibility-settings';

  constructor() {
    this.initializeAccessibility();
    this.setupMediaQueryListeners();
  }

  private initializeAccessibility(): void {
    const savedSettings = localStorage.getItem(this.SETTINGS_KEY);
    if (savedSettings) {
      try {
        const settings = JSON.parse(savedSettings);
        this.settingsSubject.next({ ...this.getDefaultSettings(), ...settings });
      } catch (error) {
        console.warn('Failed to parse accessibility settings:', error);
        this.settingsSubject.next(this.getDefaultSettings());
      }
    } else {
      this.settingsSubject.next(this.getDefaultSettings());
    }

    this.applySettings(this.settingsSubject.value);
  }

  private setupMediaQueryListeners(): void {
    // Listen for reduced motion preference
    const reducedMotionQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    reducedMotionQuery.addEventListener('change', (e) => {
      this.updateSetting('reducedMotion', e.matches);
    });

    // Listen for high contrast preference
    const highContrastQuery = window.matchMedia('(prefers-contrast: high)');
    highContrastQuery.addEventListener('change', (e) => {
      this.updateSetting('highContrast', e.matches);
    });

    // Listen for screen reader detection
    this.detectScreenReader();
  }

  private detectScreenReader(): void {
    // Simple screen reader detection
    const hasScreenReader = 
      window.speechSynthesis !== undefined ||
      navigator.userAgent.includes('NVDA') ||
      navigator.userAgent.includes('JAWS') ||
      navigator.userAgent.includes('VoiceOver');
    
    this.updateSetting('screenReader', hasScreenReader);
  }

  getCurrentSettings(): AccessibilitySettings {
    return this.settingsSubject.value;
  }

  updateSetting<K extends keyof AccessibilitySettings>(
    key: K, 
    value: AccessibilitySettings[K]
  ): void {
    const currentSettings = this.settingsSubject.value;
    const newSettings = { ...currentSettings, [key]: value };
    
    this.settingsSubject.next(newSettings);
    this.applySettings(newSettings);
    this.saveSettings(newSettings);
  }

  updateSettings(settings: Partial<AccessibilitySettings>): void {
    const currentSettings = this.settingsSubject.value;
    const newSettings = { ...currentSettings, ...settings };
    
    this.settingsSubject.next(newSettings);
    this.applySettings(newSettings);
    this.saveSettings(newSettings);
  }

  resetToDefaults(): void {
    const defaultSettings = this.getDefaultSettings();
    this.settingsSubject.next(defaultSettings);
    this.applySettings(defaultSettings);
    this.saveSettings(defaultSettings);
  }

  private applySettings(settings: AccessibilitySettings): void {
    const root = document.documentElement;
    
    // Apply reduced motion
    if (settings.reducedMotion) {
      root.style.setProperty('--transition-fast', '0.01s');
      root.style.setProperty('--transition-normal', '0.01s');
      root.style.setProperty('--transition-slow', '0.01s');
    } else {
      root.style.setProperty('--transition-fast', '0.15s ease');
      root.style.setProperty('--transition-normal', '0.3s ease');
      root.style.setProperty('--transition-slow', '0.5s ease');
    }

    // Apply high contrast
    if (settings.highContrast) {
      root.setAttribute('data-high-contrast', 'true');
    } else {
      root.removeAttribute('data-high-contrast');
    }

    // Apply large text
    if (settings.largeText) {
      root.style.setProperty('--font-size-base', '18px');
      root.style.setProperty('--font-size-sm', '16px');
      root.style.setProperty('--font-size-lg', '20px');
    } else {
      root.style.setProperty('--font-size-base', '16px');
      root.style.setProperty('--font-size-sm', '14px');
      root.style.setProperty('--font-size-lg', '18px');
    }

    // Apply focus visibility
    if (settings.focusVisible) {
      root.setAttribute('data-focus-visible', 'true');
    } else {
      root.removeAttribute('data-focus-visible');
    }

    // Apply keyboard navigation
    if (settings.keyboardNavigation) {
      root.setAttribute('data-keyboard-nav', 'true');
    } else {
      root.removeAttribute('data-keyboard-nav');
    }
  }

  private saveSettings(settings: AccessibilitySettings): void {
    localStorage.setItem(this.SETTINGS_KEY, JSON.stringify(settings));
  }

  private getDefaultSettings(): AccessibilitySettings {
    return {
      reducedMotion: window.matchMedia('(prefers-reduced-motion: reduce)').matches,
      highContrast: window.matchMedia('(prefers-contrast: high)').matches,
      largeText: false,
      screenReader: false,
      keyboardNavigation: true,
      focusVisible: true
    };
  }

  // Keyboard navigation helpers
  handleKeyboardNavigation(event: KeyboardEvent): void {
    const settings = this.getCurrentSettings();
    if (!settings.keyboardNavigation) return;

    switch (event.key) {
      case 'Tab':
        this.handleTabNavigation(event);
        break;
      case 'Escape':
        this.handleEscapeKey(event);
        break;
      case 'Enter':
      case ' ':
        this.handleActivationKey(event);
        break;
      case 'ArrowUp':
      case 'ArrowDown':
      case 'ArrowLeft':
      case 'ArrowRight':
        this.handleArrowKeys(event);
        break;
    }
  }

  private handleTabNavigation(event: KeyboardEvent): void {
    // Ensure focus is visible when navigating with Tab
    document.documentElement.setAttribute('data-keyboard-nav', 'true');
    
    // Remove keyboard nav attribute on mouse interaction
    document.addEventListener('mousedown', () => {
      document.documentElement.removeAttribute('data-keyboard-nav');
    }, { once: true });
  }

  private handleEscapeKey(event: KeyboardEvent): void {
    // Close any open modals, dropdowns, etc.
    const escapeEvent = new CustomEvent('accessibility-escape');
    document.dispatchEvent(escapeEvent);
  }

  private handleActivationKey(event: KeyboardEvent): void {
    const target = event.target as HTMLElement;
    if (target && (target.tagName === 'BUTTON' || target.getAttribute('role') === 'button')) {
      event.preventDefault();
      target.click();
    }
  }

  private handleArrowKeys(event: KeyboardEvent): void {
    // Handle arrow key navigation for custom components
    const target = event.target as HTMLElement;
    if (target && target.getAttribute('data-navigable') === 'true') {
      event.preventDefault();
      this.navigateWithArrows(target, event.key);
    }
  }

  private navigateWithArrows(element: HTMLElement, direction: string): void {
    const navigableElements = Array.from(
      document.querySelectorAll('[data-navigable="true"]')
    ) as HTMLElement[];

    const currentIndex = navigableElements.indexOf(element);
    let nextIndex = currentIndex;

    switch (direction) {
      case 'ArrowUp':
      case 'ArrowLeft':
        nextIndex = currentIndex > 0 ? currentIndex - 1 : navigableElements.length - 1;
        break;
      case 'ArrowDown':
      case 'ArrowRight':
        nextIndex = currentIndex < navigableElements.length - 1 ? currentIndex + 1 : 0;
        break;
    }

    if (nextIndex !== currentIndex) {
      navigableElements[nextIndex].focus();
    }
  }

  // Screen reader helpers
  announceToScreenReader(message: string, priority: 'polite' | 'assertive' = 'polite'): void {
    const settings = this.getCurrentSettings();
    if (!settings.screenReader) return;

    const announcement = document.createElement('div');
    announcement.setAttribute('aria-live', priority);
    announcement.setAttribute('aria-atomic', 'true');
    announcement.className = 'sr-only';
    announcement.textContent = message;

    document.body.appendChild(announcement);

    // Remove after announcement
    setTimeout(() => {
      document.body.removeChild(announcement);
    }, 1000);
  }

  // Focus management
  trapFocus(element: HTMLElement): void {
    const focusableElements = element.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    ) as NodeListOf<HTMLElement>;

    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    element.addEventListener('keydown', (event) => {
      if (event.key === 'Tab') {
        if (event.shiftKey) {
          if (document.activeElement === firstElement) {
            event.preventDefault();
            lastElement.focus();
          }
        } else {
          if (document.activeElement === lastElement) {
            event.preventDefault();
            firstElement.focus();
          }
        }
      }
    });
  }

  // Color contrast helpers
  getContrastRatio(color1: string, color2: string): number {
    // Simplified contrast ratio calculation
    // In a real implementation, you'd use a proper color contrast library
    const l1 = this.getLuminance(color1);
    const l2 = this.getLuminance(color2);
    const lighter = Math.max(l1, l2);
    const darker = Math.min(l1, l2);
    return (lighter + 0.05) / (darker + 0.05);
  }

  private getLuminance(color: string): number {
    // Simplified luminance calculation
    // In a real implementation, you'd parse the color properly
    return 0.5; // Placeholder
  }

  isAccessibleContrast(foreground: string, background: string, level: 'AA' | 'AAA' = 'AA'): boolean {
    const ratio = this.getContrastRatio(foreground, background);
    return level === 'AA' ? ratio >= 4.5 : ratio >= 7;
  }
}
