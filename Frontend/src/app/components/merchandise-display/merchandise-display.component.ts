import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription } from 'rxjs';
import { MerchandiseService } from '../../services/merchandise.service';
import { MerchandiseStore, StorePlatform, STORE_PLATFORM_OPTIONS } from '../../models/merchandise.model';

@Component({
  selector: 'app-merchandise-display',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="merchandise-display" *ngIf="stores.length > 0">
      <div class="section-header">
        <h3>
          <mat-icon>store</mat-icon>
          Merchandise
        </h3>
        <p>Check out our official merchandise stores</p>
      </div>

      <div class="stores-grid" *ngIf="!isLoading; else loading">
        <mat-card 
          class="store-card" 
          *ngFor="let store of activeStores"
          [class.featured]="store.isFeatured"
        >
          <div class="store-header">
            <div class="store-logo" *ngIf="store.logoUrl">
              <img [src]="store.logoUrl" [alt]="store.storeName">
            </div>
            <div class="store-info">
              <h4>{{ store.storeName }}</h4>
              <div class="platform-chip">
                <mat-icon>{{ getPlatformIcon(store.platform) }}</mat-icon>
                {{ getPlatformLabel(store.platform) }}
              </div>
            </div>
            <div class="store-badge" *ngIf="store.isFeatured">
              <mat-icon>star</mat-icon>
              Featured
            </div>
          </div>

          <div class="store-description" *ngIf="store.description">
            <p>{{ store.description }}</p>
          </div>

          <div class="store-actions">
            <a 
              mat-raised-button 
              color="primary" 
              [href]="store.storeUrl" 
              target="_blank" 
              rel="noopener noreferrer"
              [matTooltip]="'Visit ' + store.storeName"
            >
              <mat-icon>open_in_new</mat-icon>
              Visit Store
            </a>
          </div>
        </mat-card>
      </div>

      <ng-template #loading>
        <div class="loading">
          <mat-spinner diameter="24"></mat-spinner>
          <span>Loading merchandise...</span>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .merchandise-display {
      margin: 24px 0;
    }

    .section-header {
      margin-bottom: 20px;
    }

    .section-header h3 {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0 0 8px 0;
      color: #333;
      font-size: 1.2rem;
    }

    .section-header p {
      margin: 0;
      color: #666;
      font-size: 0.9rem;
    }

    .stores-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 16px;
    }

    .store-card {
      transition: transform 0.2s ease, box-shadow 0.2s ease;
      border: 1px solid #e0e0e0;
    }

    .store-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }

    .store-card.featured {
      border-color: #ff9800;
      background: linear-gradient(135deg, #fff8e1 0%, #ffffff 100%);
    }

    .store-header {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      margin-bottom: 16px;
      position: relative;
    }

    .store-logo {
      width: 48px;
      height: 48px;
      border-radius: 8px;
      overflow: hidden;
      background: #f5f5f5;
      flex-shrink: 0;
    }

    .store-logo img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .store-info {
      flex: 1;
      min-width: 0;
    }

    .store-info h4 {
      margin: 0 0 8px 0;
      color: #333;
      font-size: 1.1rem;
      font-weight: 500;
      word-wrap: break-word;
    }

    .platform-chip {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 4px 8px;
      background: #f0f0f0;
      border-radius: 16px;
      font-size: 0.8rem;
      color: #666;
    }

    .store-badge {
      position: absolute;
      top: -8px;
      right: -8px;
      background: #ff9800;
      color: white;
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 0.7rem;
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: 4px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.2);
    }

    .store-description {
      margin-bottom: 16px;
    }

    .store-description p {
      margin: 0;
      color: #666;
      font-size: 0.9rem;
      line-height: 1.4;
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .store-actions {
      display: flex;
      justify-content: flex-end;
    }

    .store-actions a {
      text-decoration: none;
    }

    .loading {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 20px;
      color: #666;
    }

    @media (max-width: 768px) {
      .stores-grid {
        grid-template-columns: 1fr;
      }

      .store-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }

      .store-badge {
        position: static;
        align-self: flex-end;
      }
    }

    @media (max-width: 480px) {
      .section-header h3 {
        font-size: 1.1rem;
      }

      .store-card {
        padding: 16px;
      }

      .store-info h4 {
        font-size: 1rem;
      }
    }
  `]
})
export class MerchandiseDisplayComponent implements OnInit, OnDestroy {
  @Input() channelId!: number;
  
  stores: MerchandiseStore[] = [];
  isLoading = false;
  private subscriptions = new Subscription();

  constructor(private merchandiseService: MerchandiseService) {}

  ngOnInit() {
    if (this.channelId) {
      this.loadStores();
    }
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  get activeStores(): MerchandiseStore[] {
    return this.stores.filter(store => store.isActive);
  }

  loadStores() {
    this.isLoading = true;
    this.subscriptions.add(
      this.merchandiseService.getChannelStores(this.channelId).subscribe({
        next: (stores: MerchandiseStore[]) => {
          this.stores = stores;
          this.isLoading = false;
        },
        error: (error: any) => {
          this.isLoading = false;
          console.error('Failed to load merchandise stores:', error);
        }
      })
    );
  }

  getPlatformLabel(platform: StorePlatform): string {
    const option = STORE_PLATFORM_OPTIONS.find((opt: any) => opt.value === platform);
    return option ? option.label : platform;
  }

  getPlatformIcon(platform: StorePlatform): string {
    const option = STORE_PLATFORM_OPTIONS.find((opt: any) => opt.value === platform);
    return option ? option.label.toLowerCase() : 'store';
  }
}
