import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { ReferralService } from '../../services/referral.service';
import { 
  Referral, 
  ReferralStats, 
  ReferralReward, 
  ReferralCode, 
  CreateReferralCode,
  ClaimReward,
  ReferralStatus,
  RewardStatus
} from '../../models/referral.model';

@Component({
  selector: 'app-referral-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe, DecimalPipe],
  templateUrl: './referral-management.component.html',
  styleUrls: ['./referral-management.component.scss']
})
export class ReferralManagementComponent implements OnInit {
  Math = Math; // Make Math available in template
  referralForm: FormGroup;
  codeForm: FormGroup;
  referrals: Referral[] = [];
  rewards: ReferralReward[] = [];
  stats: ReferralStats | null = null;
  referralCode: ReferralCode | null = null;
  loading = false;
  currentPage = 1;
  rewardsPage = 1;
  pageSize = 10;
  totalCount = 0;
  rewardsTotalCount = 0;
  activeTab = 'referrals';

  constructor(
    private fb: FormBuilder,
    private referralService: ReferralService
  ) {
    this.referralForm = this.fb.group({
      referredUserId: ['', Validators.required],
      referralCode: ['']
    });

    this.codeForm = this.fb.group({
      code: [''],
      maxUsage: [null],
      expiresAt: [''],
      rewardAmount: [10],
      rewardType: ['Points']
    });
  }

  ngOnInit(): void {
    this.loadReferrals();
    this.loadStats();
    this.loadReferralCode();
    this.loadRewards();
  }

  loadReferrals(): void {
    this.loading = true;
    this.referralService.getMyReferrals(this.currentPage, this.pageSize).subscribe({
      next: (result) => {
        this.referrals = result.items;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading referrals:', error);
        this.loading = false;
      }
    });
  }

  loadRewards(): void {
    this.referralService.getUserRewards(this.rewardsPage, this.pageSize).subscribe({
      next: (result) => {
        this.rewards = result.items;
        this.rewardsTotalCount = result.totalCount;
      },
      error: (error) => {
        console.error('Error loading rewards:', error);
      }
    });
  }

  loadStats(): void {
    this.referralService.getReferralStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Error loading stats:', error);
      }
    });
  }

  loadReferralCode(): void {
    this.referralService.getReferralCode().subscribe({
      next: (code) => {
        this.referralCode = code;
      },
      error: (error) => {
        console.error('Error loading referral code:', error);
      }
    });
  }

  onCreateReferral(): void {
    if (this.referralForm.valid) {
      this.loading = true;
      const referralData = this.referralForm.value;
      
      this.referralService.createReferral(referralData).subscribe({
        next: (referral) => {
          this.referrals.unshift(referral);
          this.referralForm.reset();
          this.loadStats();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error creating referral:', error);
          this.loading = false;
        }
      });
    }
  }

  onCreateReferralCode(): void {
    if (this.codeForm.valid) {
      this.loading = true;
      const codeData: CreateReferralCode = this.codeForm.value;
      
      this.referralService.createReferralCode(codeData).subscribe({
        next: (code) => {
          this.referralCode = code;
          this.codeForm.reset();
          this.codeForm.patchValue({
            rewardAmount: 10,
            rewardType: 'Points'
          });
          this.loading = false;
        },
        error: (error) => {
          console.error('Error creating referral code:', error);
          this.loading = false;
        }
      });
    }
  }

  claimReward(rewardId: number): void {
    const claimData: ClaimReward = { rewardId };
    
    this.referralService.claimReward(claimData).subscribe({
      next: (reward) => {
        this.loadRewards();
        this.loadStats();
      },
      error: (error) => {
        console.error('Error claiming reward:', error);
      }
    });
  }

  copyReferralCode(): void {
    if (this.referralCode?.code) {
      navigator.clipboard.writeText(this.referralCode.code).then(() => {
        // Show success message
        console.log('Referral code copied to clipboard');
      });
    }
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadReferrals();
  }

  onRewardsPageChange(page: number): void {
    this.rewardsPage = page;
    this.loadRewards();
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  getStatusClass(status: ReferralStatus): string {
    switch (status) {
      case ReferralStatus.Pending:
        return 'status-pending';
      case ReferralStatus.Completed:
        return 'status-completed';
      case ReferralStatus.Rewarded:
        return 'status-rewarded';
      case ReferralStatus.Cancelled:
        return 'status-cancelled';
      default:
        return '';
    }
  }

  getRewardStatusClass(status: RewardStatus): string {
    switch (status) {
      case RewardStatus.Pending:
        return 'status-pending';
      case RewardStatus.Claimed:
        return 'status-claimed';
      case RewardStatus.Expired:
        return 'status-expired';
      case RewardStatus.Cancelled:
        return 'status-cancelled';
      default:
        return '';
    }
  }

  getStatusText(status: ReferralStatus): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  getRewardStatusText(status: RewardStatus): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }
}