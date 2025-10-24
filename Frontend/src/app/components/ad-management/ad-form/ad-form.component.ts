import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatStepperModule } from '@angular/material/stepper';
import { MatRadioModule } from '@angular/material/radio';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, takeUntil } from 'rxjs';
import { MonetizationService, AdCampaignDto, AdDto } from '../../../services/monetization.service';

export interface AdFormData {
  mode: 'create' | 'edit';
  type: 'campaign' | 'ad';
  campaign?: AdCampaignDto;
  ad?: AdDto;
  campaignId?: number;
}

@Component({
  selector: 'app-ad-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatTabsModule,
    MatChipsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatStepperModule,
    MatRadioModule,
    MatCheckboxModule,
    MatTooltipModule
  ],
  templateUrl: './ad-form.component.html',
  styleUrls: ['./ad-form.component.scss']
})
export class AdFormComponent implements OnInit, OnDestroy {
  // Form groups
  campaignForm!: FormGroup;
  adForm!: FormGroup;
  
  // Form state
  isSubmitting = false;
  currentStep = 0;
  
  // Data
  campaigns: AdCampaignDto[] = [];
  selectedCampaign: AdCampaignDto | null = null;
  
  // Ad types and positions
  adTypes = [
    { value: 'video', label: 'Video Ad', description: 'Pre-roll, mid-roll, or post-roll video advertisement' },
    { value: 'banner', label: 'Banner Ad', description: 'Static or animated banner advertisement' },
    { value: 'overlay', label: 'Overlay Ad', description: 'Overlay advertisement on video content' }
  ];
  
  adPositions = [
    { value: 'pre-roll', label: 'Pre-roll', description: 'Before video starts' },
    { value: 'mid-roll', label: 'Mid-roll', description: 'During video playback' },
    { value: 'post-roll', label: 'Post-roll', description: 'After video ends' },
    { value: 'banner-top', label: 'Banner Top', description: 'Top of page banner' },
    { value: 'banner-side', label: 'Banner Side', description: 'Sidebar banner' },
    { value: 'banner-bottom', label: 'Banner Bottom', description: 'Bottom of page banner' }
  ];
  
  campaignStatuses = [
    { value: 'draft', label: 'Draft' },
    { value: 'active', label: 'Active' },
    { value: 'paused', label: 'Paused' },
    { value: 'completed', label: 'Completed' }
  ];
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AdFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AdFormData,
    @Inject(MonetizationService) private monetizationService: MonetizationService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initializeForms();
    this.loadCampaigns();
    
    if (this.data.mode === 'edit') {
      this.populateForms();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForms(): void {
    // Campaign form
    this.campaignForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      status: ['draft', Validators.required],
      budget: [0, [Validators.required, Validators.min(1)]],
      startDate: [new Date(), Validators.required],
      endDate: [null, Validators.required],
      targetAudience: [''],
      keywords: [[]],
      isActive: [true]
    });

    // Ad form
    this.adForm = this.fb.group({
      campaignId: [this.data.campaignId || null, Validators.required],
      type: ['', Validators.required],
      content: ['', [Validators.required, Validators.minLength(5)]],
      url: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
      thumbnailUrl: [''],
      costPerClick: [0, [Validators.required, Validators.min(0.01)]],
      costPerView: [0, [Validators.required, Validators.min(0.01)]],
      duration: [30, [Validators.required, Validators.min(5), Validators.max(300)]],
      skipAfter: [5, [Validators.required, Validators.min(0)]],
      position: ['', Validators.required],
      isActive: [true]
    });
  }

  private loadCampaigns(): void {
    this.monetizationService.getAdCampaigns().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (campaigns: AdCampaignDto[]) => {
        this.campaigns = campaigns;
        if (this.data.campaignId) {
          this.selectedCampaign = campaigns.find((c: AdCampaignDto) => c.id === this.data.campaignId) || null;
        }
      },
      error: (error: any) => {
        console.error('Error loading campaigns:', error);
        this.snackBar.open('Error loading campaigns', 'Close', { duration: 3000 });
      }
    });
  }

  private populateForms(): void {
    if (this.data.type === 'campaign' && this.data.campaign) {
      this.campaignForm.patchValue({
        name: this.data.campaign.name,
        description: '',
        status: this.data.campaign.status,
        budget: this.data.campaign.budget,
        startDate: new Date(this.data.campaign.startDate),
        endDate: new Date(this.data.campaign.endDate),
        targetAudience: '',
        keywords: [],
        isActive: this.data.campaign.status === 'active'
      });
    } else if (this.data.type === 'ad' && this.data.ad) {
      this.adForm.patchValue({
        campaignId: this.data.campaignId,
        type: this.data.ad.type,
        content: this.data.ad.content,
        url: this.data.ad.url,
        thumbnailUrl: this.data.ad.thumbnailUrl,
        costPerClick: this.data.ad.costPerClick,
        costPerView: this.data.ad.costPerView,
        duration: this.data.ad.duration,
        skipAfter: this.data.ad.skipAfter,
        position: this.data.ad.position,
        isActive: true
      });
    }
  }

  nextStep(): void {
    if (this.currentStep === 0 && this.campaignForm.valid) {
      this.currentStep = 1;
    } else if (this.currentStep === 1 && this.adForm.valid) {
      this.submitForm();
    }
  }

  previousStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  submitForm(): void {
    if (this.data.type === 'campaign') {
      this.submitCampaign();
    } else {
      this.submitAd();
    }
  }

  private submitCampaign(): void {
    if (this.campaignForm.invalid) {
      this.markFormGroupTouched(this.campaignForm);
      return;
    }

    this.isSubmitting = true;
    const formValue = this.campaignForm.value;
    
    const campaignData = {
      name: formValue.name,
      description: formValue.description,
      status: formValue.status,
      budget: formValue.budget,
      startDate: formValue.startDate.toISOString(),
      endDate: formValue.endDate.toISOString(),
      targetAudience: formValue.targetAudience,
      keywords: formValue.keywords,
      isActive: formValue.isActive
    };

    // Simulate API call
    setTimeout(() => {
      this.isSubmitting = false;
      this.snackBar.open('Campaign saved successfully', 'Close', { duration: 3000 });
      this.dialogRef.close(campaignData);
    }, 1000);
  }

  private submitAd(): void {
    if (this.adForm.invalid) {
      this.markFormGroupTouched(this.adForm);
      return;
    }

    this.isSubmitting = true;
    const formValue = this.adForm.value;
    
    const adData = {
      campaignId: formValue.campaignId,
      type: formValue.type,
      content: formValue.content,
      url: formValue.url,
      thumbnailUrl: formValue.thumbnailUrl,
      costPerClick: formValue.costPerClick,
      costPerView: formValue.costPerView,
      duration: formValue.duration,
      skipAfter: formValue.skipAfter,
      position: formValue.position,
      isActive: formValue.isActive
    };

    // Simulate API call
    setTimeout(() => {
      this.isSubmitting = false;
      this.snackBar.open('Ad saved successfully', 'Close', { duration: 3000 });
      this.dialogRef.close(adData);
    }, 1000);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  onCampaignSelect(campaignId: number): void {
    this.selectedCampaign = this.campaigns.find(c => c.id === campaignId) || null;
    this.adForm.patchValue({ campaignId });
  }

  onAdTypeChange(adType: string): void {
    // Update position options based on ad type
    const positionControl = this.adForm.get('position');
    if (positionControl) {
      positionControl.setValue('');
    }
  }

  getAdTypeDescription(type: string): string {
    const adType = this.adTypes.find(t => t.value === type);
    return adType?.description || '';
  }

  getPositionDescription(position: string): string {
    const pos = this.adPositions.find(p => p.value === position);
    return pos?.description || '';
  }

  getFilteredPositions(): any[] {
    const adType = this.adForm.get('type')?.value;
    
    if (adType === 'video') {
      return this.adPositions.filter(p => 
        ['pre-roll', 'mid-roll', 'post-roll'].includes(p.value)
      );
    } else if (adType === 'banner') {
      return this.adPositions.filter(p => 
        ['banner-top', 'banner-side', 'banner-bottom'].includes(p.value)
      );
    } else if (adType === 'overlay') {
      return this.adPositions.filter(p => 
        ['pre-roll', 'mid-roll', 'post-roll'].includes(p.value)
      );
    }
    
    return this.adPositions;
  }

  calculateEstimatedCost(): number {
    const costPerClick = this.adForm.get('costPerClick')?.value || 0;
    const costPerView = this.adForm.get('costPerView')?.value || 0;
    const budget = this.campaignForm.get('budget')?.value || 0;
    
    if (this.data.type === 'campaign') {
      return budget;
    } else {
      return Math.max(costPerClick, costPerView);
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  cancel(): void {
    this.dialogRef.close();
  }

  // Keyword management methods
  separatorKeysCodes: number[] = [13, 188]; // Enter and comma

  addKeyword(event: any): void {
    const value = event.value;
    if (value && value.trim()) {
      const keywords = this.campaignForm.get('keywords')?.value || [];
      if (!keywords.includes(value.trim())) {
        keywords.push(value.trim());
        this.campaignForm.patchValue({ keywords });
      }
      event.chipInput.clear();
    }
  }

  removeKeyword(keyword: string): void {
    const keywords = this.campaignForm.get('keywords')?.value || [];
    const index = keywords.indexOf(keyword);
    if (index >= 0) {
      keywords.splice(index, 1);
      this.campaignForm.patchValue({ keywords });
    }
  }
}
