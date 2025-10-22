import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SuperChatService } from '../../services/super-chat.service';
import { SuperChatRequest, SUPER_CHAT_TIERS } from '../../models/super-chat.model';

export interface SuperChatDialogData {
  liveStreamId: number;
  liveStreamTitle: string;
}

@Component({
  selector: 'app-super-chat-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatIconModule
  ],
  templateUrl: './super-chat-dialog.component.html',
  styleUrl: './super-chat-dialog.component.scss'
})
export class SuperChatDialogComponent implements OnInit {
  superChatForm: FormGroup;
  selectedTier: any = null;
  isProcessing = false;
  SUPER_CHAT_TIERS = SUPER_CHAT_TIERS;

  constructor(
    public dialogRef: MatDialogRef<SuperChatDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SuperChatDialogData,
    private fb: FormBuilder,
    private superChatService: SuperChatService,
    private snackBar: MatSnackBar
  ) {
    this.superChatForm = this.fb.group({
      amount: [1, [Validators.required, Validators.min(1)]],
      donorName: ['', [Validators.required]],
      donorMessage: ['', [Validators.maxLength(200)]],
      isAnonymous: [false]
    });
  }

  ngOnInit() {
    // Select first tier by default
    this.selectTier(SUPER_CHAT_TIERS[0]);
  }

  selectTier(tier: any) {
    this.selectedTier = tier;
    this.superChatForm.patchValue({ amount: tier.amount });
  }

  sendSuperChat() {
    if (this.superChatForm.valid) {
      this.isProcessing = true;
      
      const request: SuperChatRequest = {
        liveStreamId: this.data.liveStreamId,
        donorName: this.superChatForm.value.donorName,
        donorMessage: this.superChatForm.value.donorMessage,
        amount: this.superChatForm.value.amount,
        currency: 'USD',
        paymentMethodId: 'pm_card_visa', // TODO: Integrate with Stripe Elements
        isAnonymous: this.superChatForm.value.isAnonymous
      };

      this.superChatService.processSuperChat(request).subscribe({
        next: (superChat) => {
          this.snackBar.open('Super Chat sent successfully!', 'Close', { duration: 3000 });
          this.dialogRef.close(superChat);
        },
        error: (error) => {
          this.snackBar.open('Failed to send Super Chat. Please try again.', 'Close', { duration: 3000 });
          this.isProcessing = false;
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
