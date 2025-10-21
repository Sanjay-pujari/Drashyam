import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { InviteService } from '../../services/invite.service';
import { UserInvite, CreateInvite, InviteStats, InviteType, InviteStatus } from '../../models/invite.model';

@Component({
  selector: 'app-invite-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe, DecimalPipe],
  templateUrl: './invite-management.component.html',
  styleUrls: ['./invite-management.component.scss']
})
export class InviteManagementComponent implements OnInit {
  Math = Math; // Make Math available in template
  InviteType = InviteType; // Make InviteType enum available in template
  InviteStatus = InviteStatus; // Make InviteStatus enum available in template
  inviteForm: FormGroup;
  invites: UserInvite[] = [];
  stats: InviteStats | null = null;
  loading = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  constructor(
    private fb: FormBuilder,
    private inviteService: InviteService
  ) {
    this.inviteForm = this.fb.group({
      inviteeEmail: ['', [Validators.required, Validators.email]],
      inviteeFirstName: [''],
      inviteeLastName: [''],
      personalMessage: [''],
      type: [InviteType.Email],
      expirationDays: [7]
    });
  }

  ngOnInit(): void {
    this.loadInvites();
    this.loadStats();
  }

  loadInvites(): void {
    this.loading = true;
    this.inviteService.getMyInvites(this.currentPage, this.pageSize).subscribe({
      next: (result) => {
        this.invites = result.items;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading invites:', error);
        this.loading = false;
        alert('Failed to load invites. Please try again.');
      }
    });
  }

  loadStats(): void {
    this.inviteService.getInviteStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Error loading stats:', error);
        alert('Failed to load invite statistics. Please try again.');
      }
    });
  }

  onSubmit(): void {
    if (this.inviteForm.valid) {
      this.loading = true;
      const inviteData: CreateInvite = this.inviteForm.value;
      
      this.inviteService.createInvite(inviteData).subscribe({
        next: (invite) => {
          this.invites.unshift(invite);
          this.inviteForm.reset();
          this.inviteForm.patchValue({
            type: InviteType.Email,
            expirationDays: 7
          });
          this.loadStats();
          this.loading = false;
          alert('Invitation sent successfully!');
        },
        error: (error) => {
          console.error('Error creating invite:', error);
          this.loading = false;
          alert('Failed to send invitation. Please try again.');
        }
      });
    }
  }

  resendInvite(inviteId: number): void {
    this.inviteService.resendInvite(inviteId).subscribe({
      next: () => {
        this.loadInvites();
        this.loadStats();
        alert('Invitation resent successfully!');
      },
      error: (error) => {
        console.error('Error resending invite:', error);
        alert('Failed to resend invitation. Please try again.');
      }
    });
  }

  cancelInvite(inviteId: number): void {
    if (confirm('Are you sure you want to cancel this invite?')) {
      this.inviteService.cancelInvite(inviteId).subscribe({
        next: () => {
          this.loadInvites();
          this.loadStats();
          alert('Invitation cancelled successfully!');
        },
        error: (error) => {
          console.error('Error cancelling invite:', error);
          alert('Failed to cancel invitation. Please try again.');
        }
      });
    }
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadInvites();
  }

  getStatusClass(status: InviteStatus): string {
    switch (status) {
      case InviteStatus.Pending:
        return 'status-pending';
      case InviteStatus.Accepted:
        return 'status-accepted';
      case InviteStatus.Expired:
        return 'status-expired';
      case InviteStatus.Cancelled:
        return 'status-cancelled';
      default:
        return '';
    }
  }

  getStatusText(status: InviteStatus): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }
}
