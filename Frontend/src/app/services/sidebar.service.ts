import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SidebarService {
  private isCollapsedSubject = new BehaviorSubject<boolean>(false);
  public isCollapsed$ = this.isCollapsedSubject.asObservable();

  constructor() {
    // Load saved state from localStorage
    const savedState = localStorage.getItem('sidebar-collapsed');
    if (savedState !== null) {
      this.isCollapsedSubject.next(JSON.parse(savedState));
    }
  }

  toggle(): void {
    const newState = !this.isCollapsedSubject.value;
    this.isCollapsedSubject.next(newState);
    localStorage.setItem('sidebar-collapsed', JSON.stringify(newState));
  }

  collapse(): void {
    this.isCollapsedSubject.next(true);
    localStorage.setItem('sidebar-collapsed', JSON.stringify(true));
  }

  expand(): void {
    this.isCollapsedSubject.next(false);
    localStorage.setItem('sidebar-collapsed', JSON.stringify(false));
  }

  get isCollapsed(): boolean {
    return this.isCollapsedSubject.value;
  }
}
