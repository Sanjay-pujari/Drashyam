import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AdService, AdCampaign, AdCampaignCreate, AdCampaignUpdate, AdAnalytics } from '../../services/ad.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-manage-campaigns',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDividerModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="container">
      <div class="header">
        <h1>Manage Ad Campaigns</h1>
        <p>Create, edit, activate/pause and track campaigns</p>
      </div>

      <mat-card class="campaign-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>add_circle</mat-icon>
            Create Campaign
          </mat-card-title>
        </mat-card-header>
        <mat-divider></mat-divider>
        <mat-card-content>
          <form [formGroup]="createForm" (ngSubmit)="create()" class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Name</mat-label>
              <input matInput formControlName="name" placeholder="Campaign name">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Type</mat-label>
              <mat-select formControlName="type">
                <mat-option value="Banner">Banner</mat-option>
                <mat-option value="Video">Video</mat-option>
                <mat-option value="Overlay">Overlay</mat-option>
                <mat-option value="Sponsored">Sponsored</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Budget</mat-label>
              <input matInput type="number" formControlName="budget" placeholder="100.00">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>CPV</mat-label>
              <input matInput type="number" formControlName="costPerView" placeholder="0.01">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>CPC</mat-label>
              <input matInput type="number" formControlName="costPerClick" placeholder="0.05">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Start Date</mat-label>
              <input matInput type="date" formControlName="startDate">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>End Date</mat-label>
              <input matInput type="date" formControlName="endDate">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full">
              <mat-label>Ad URL</mat-label>
              <input matInput formControlName="adUrl" placeholder="https://example.com">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full">
              <mat-label>Ad Content (HTML allowed)</mat-label>
              <textarea matInput rows="3" formControlName="adContent" placeholder="<div>Ad</div>"></textarea>
            </mat-form-field>

            <div class="actions">
              <button mat-raised-button color="primary" type="submit" [disabled]="createForm.invalid || creating">
                <mat-icon *ngIf="creating">hourglass_empty</mat-icon>
                <mat-icon *ngIf="!creating">save</mat-icon>
                {{ creating ? 'Creating...' : 'Create Campaign' }}
              </button>
              <button mat-button type="button" (click)="createForm.reset()">
                <mat-icon>refresh</mat-icon>
                Reset
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>

      <mat-card class="campaign-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>list</mat-icon>
            Campaigns
          </mat-card-title>
        </mat-card-header>
        <mat-divider></mat-divider>
        <mat-card-content>
          <div class="table">
            <div class="table-header">
              <div>Name</div>
              <div>Type</div>
              <div>Status</div>
              <div>Budget</div>
              <div>CPV</div>
              <div>CPC</div>
              <div>Actions</div>
            </div>
            <div class="table-row" *ngFor="let c of campaigns">
              <div>{{ c.name }}</div>
              <div>{{ c.type }}</div>
              <div><span class="status" [class.active]="c.status==='Active'" [class.paused]="c.status==='Paused'">{{ c.status }}</span></div>
              <div>{{ c.budget | number:'1.2-2' }}</div>
              <div>{{ c.costPerView | number:'1.2-4' }}</div>
              <div>{{ c.costPerClick | number:'1.2-4' }}</div>
              <div class="row-actions">
                <button mat-icon-button color="primary" aria-label="Activate" (click)="activate(c)" [disabled]="c.status==='Active'">
                  <mat-icon>play_circle</mat-icon>
                </button>
                <button mat-icon-button color="warn" aria-label="Pause" (click)="pause(c)" [disabled]="c.status==='Paused'">
                  <mat-icon>pause_circle</mat-icon>
                </button>
                <button mat-icon-button aria-label="Edit" (click)="startEdit(c)">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button color="warn" aria-label="Delete" (click)="remove(c)">
                  <mat-icon>delete</mat-icon>
                </button>
                <button mat-icon-button aria-label="Analytics" (click)="openAnalytics(c)">
                  <mat-icon>insights</mat-icon>
                </button>
              </div>
            </div>
            <div class="empty" *ngIf="!loading && campaigns.length===0">No campaigns yet</div>
            <div class="loading" *ngIf="loading">
              <mat-spinner diameter="32"></mat-spinner>
              <span>Loading campaigns...</span>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="campaign-card" *ngIf="analyticsCampaign">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>insights</mat-icon>
            Analytics — {{ analyticsCampaign.name }}
          </mat-card-title>
          <span class="spacer"></span>
          <button mat-button (click)="closeAnalytics()">
            <mat-icon>close</mat-icon>
            Close
          </button>
        </mat-card-header>
        <mat-divider></mat-divider>
        <mat-card-content>
          <div *ngIf="analyticsLoading" class="loading">
            <mat-spinner diameter="32"></mat-spinner>
            <span>Loading analytics...</span>
          </div>
          <div *ngIf="!analyticsLoading && analytics">
            <div class="analytics-grid">
              <div class="metric">
                <div class="label">Impressions</div>
                <div class="value">{{ analytics.totalImpressions | number }}</div>
              </div>
              <div class="metric">
                <div class="label">Clicks</div>
                <div class="value">{{ analytics.totalClicks | number }}</div>
              </div>
              <div class="metric">
                <div class="label">CTR</div>
                <div class="value">{{ analytics.clickThroughRate | percent:'1.2-2' }}</div>
              </div>
              <div class="metric">
                <div class="label">Revenue</div>
                <div class="value">{{ analytics.totalRevenue | currency }}</div>
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="campaign-card" *ngIf="editing">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>edit</mat-icon>
            Edit Campaign
          </mat-card-title>
        </mat-card-header>
        <mat-divider></mat-divider>
        <mat-card-content>
          <form [formGroup]="editForm" (ngSubmit)="update()" class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Name</mat-label>
              <input matInput formControlName="name">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Status</mat-label>
              <mat-select formControlName="status">
                <mat-option value="Draft">Draft</mat-option>
                <mat-option value="Active">Active</mat-option>
                <mat-option value="Paused">Paused</mat-option>
                <mat-option value="Completed">Completed</mat-option>
                <mat-option value="Cancelled">Cancelled</mat-option>
              </mat-select>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Budget</mat-label>
              <input matInput type="number" formControlName="budget">
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Ad URL</mat-label>
              <input matInput formControlName="adUrl">
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Ad Content</mat-label>
              <textarea matInput rows="3" formControlName="adContent"></textarea>
            </mat-form-field>
            <div class="actions">
              <button mat-raised-button color="primary" type="submit" [disabled]="editForm.invalid || updating">
                <mat-icon *ngIf="updating">hourglass_empty</mat-icon>
                <mat-icon *ngIf="!updating">save</mat-icon>
                {{ updating ? 'Updating...' : 'Update Campaign' }}
              </button>
              <button mat-button type="button" (click)="cancelEdit()">
                <mat-icon>close</mat-icon>
                Cancel
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .container { max-width: 1200px; margin: 0 auto; padding: 24px; }
    .header { margin-bottom: 16px; }
    .campaign-card { margin-bottom: 16px; }
    .form-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; }
    .form-grid .full { grid-column: 1 / -1; }
    .actions { display: flex; gap: 8px; margin-top: 8px; }
    .table { width: 100%; }
    .table-header, .table-row { display: grid; grid-template-columns: 2fr 1fr 1fr 1fr 1fr 1fr 2fr; align-items: center; gap: 8px; padding: 10px 0; }
    .table-header { font-weight: 600; border-bottom: 1px solid #e0e0e0; }
    .row-actions { display: flex; gap: 6px; }
    .loading { display: flex; align-items: center; gap: 8px; padding: 12px 0; }
    .empty { padding: 12px 0; color: #666; }
    .status.active { color: #2e7d32; }
    .status.paused { color: #ef6c00; }
    .spacer { flex: 1 1 auto; }
    .analytics-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
    .metric { background: #fafafa; border: 1px solid #eee; border-radius: 8px; padding: 12px; }
    .metric .label { color: #666; font-size: 12px; }
    .metric .value { font-size: 18px; font-weight: 600; }
    @media (max-width: 900px) { .form-grid { grid-template-columns: 1fr 1fr; } .table-header, .table-row { grid-template-columns: 1.5fr 1fr 1fr 1fr 1fr 1fr 2fr; } }
    @media (max-width: 600px) { .form-grid { grid-template-columns: 1fr; } .table-header, .table-row { grid-template-columns: 1.5fr 1fr 1fr 1fr 1fr 1fr 2fr; } }
  `]
})
export class ManageCampaignsComponent implements OnInit, OnDestroy {
  campaigns: AdCampaign[] = [];
  loading = false;
  creating = false;
  updating = false;
  editing: AdCampaign | null = null;
  analyticsCampaign: AdCampaign | null = null;
  analytics: AdAnalytics | null = null;
  analyticsLoading = false;

  createForm!: FormGroup;
  editForm!: FormGroup;

  private subs = new Subscription();

  constructor(private adService: AdService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.createForm = this.fb.group({
      name: ['', Validators.required],
      type: ['Banner', Validators.required],
      budget: [100, [Validators.required, Validators.min(0)]],
      costPerView: [0.01, [Validators.required, Validators.min(0)]],
      costPerClick: [0.05, [Validators.required, Validators.min(0)]],
      startDate: [this.today()],
      endDate: [this.todayPlus(30)],
      adUrl: [''],
      adContent: ['']
    });

    this.editForm = this.fb.group({
      name: ['', Validators.required],
      status: ['Draft', Validators.required],
      budget: [0, [Validators.required, Validators.min(0)]],
      adUrl: [''],
      adContent: ['']
    });

    this.load();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  load(page: number = 1): void {
    this.loading = true;
    this.subs.add(
      this.adService.getCampaigns(page, 50).subscribe({
        next: res => { this.campaigns = res.items; this.loading = false; },
        error: _ => { this.loading = false; }
      })
    );
  }

  create(): void {
    if (this.createForm.invalid) return;
    this.creating = true;
    const payload: AdCampaignCreate = {
      name: this.createForm.value.name,
      type: this.createForm.value.type,
      budget: this.createForm.value.budget,
      costPerView: this.createForm.value.costPerView,
      costPerClick: this.createForm.value.costPerClick,
      startDate: this.createForm.value.startDate,
      endDate: this.createForm.value.endDate,
      adUrl: this.createForm.value.adUrl,
      adContent: this.createForm.value.adContent
    };
    this.subs.add(
      this.adService.createCampaign(payload).subscribe({
        next: _ => { this.creating = false; this.createForm.reset({ type: 'Banner', budget: 100, costPerView: 0.01, costPerClick: 0.05, startDate: this.today(), endDate: this.todayPlus(30) }); this.load(); },
        error: _ => { this.creating = false; }
      })
    );
  }

  startEdit(c: AdCampaign): void {
    this.editing = c;
    this.editForm.patchValue({
      name: c.name,
      status: c.status,
      budget: c.budget,
      adUrl: c.adUrl || '',
      adContent: c.adContent || ''
    });
  }

  cancelEdit(): void { this.editing = null; }

  update(): void {
    if (!this.editing || this.editForm.invalid) return;
    this.updating = true;
    const payload: AdCampaignUpdate = { ...this.editForm.value };
    this.subs.add(
      this.adService.updateCampaign(this.editing.id, payload).subscribe({
        next: updated => {
          this.updating = false;
          this.editing = null;
          this.campaigns = this.campaigns.map(x => x.id === updated.id ? updated : x);
        },
        error: _ => { this.updating = false; }
      })
    );
  }

  activate(c: AdCampaign): void {
    this.subs.add(this.adService.activateCampaign(c.id).subscribe({ next: _ => this.load(), error: _ => {} }));
  }

  pause(c: AdCampaign): void {
    this.subs.add(this.adService.pauseCampaign(c.id).subscribe({ next: _ => this.load(), error: _ => {} }));
  }

  remove(c: AdCampaign): void {
    if (!confirm(`Delete campaign "${c.name}"?`)) return;
    this.subs.add(this.adService.deleteCampaign(c.id).subscribe({ next: _ => this.load(), error: _ => {} }));
  }

  openAnalytics(c: AdCampaign): void {
    this.analyticsCampaign = c;
    this.analytics = null;
    this.analyticsLoading = true;
    this.subs.add(
      this.adService.getCampaignAnalytics(c.id).subscribe({
        next: a => { this.analytics = a; this.analyticsLoading = false; },
        error: _ => { this.analyticsLoading = false; }
      })
    );
  }

  closeAnalytics(): void {
    this.analyticsCampaign = null;
    this.analytics = null;
    this.analyticsLoading = false;
  }

  private today(): string {
    const d = new Date();
    return d.toISOString().slice(0, 10);
  }

  private todayPlus(days: number): string {
    const d = new Date();
    d.setDate(d.getDate() + days);
    return d.toISOString().slice(0, 10);
  }
}


