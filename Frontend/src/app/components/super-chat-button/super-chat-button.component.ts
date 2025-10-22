import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { SuperChatDialogComponent } from '../super-chat-dialog/super-chat-dialog.component';

@Component({
  selector: 'app-super-chat-button',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <button 
      mat-raised-button 
      color="warn" 
      class="super-chat-button"
      (click)="openSuperChatDialog()">
      <mat-icon>favorite</mat-icon>
      Super Chat
    </button>
  `,
  styles: [`
    .super-chat-button {
      background: linear-gradient(45deg, #ff6b6b, #ff8e8e);
      color: white;
      font-weight: bold;
      padding: 12px 24px;
      border-radius: 25px;
      box-shadow: 0 4px 12px rgba(255, 107, 107, 0.3);
      transition: all 0.3s ease;
    }

    .super-chat-button:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(255, 107, 107, 0.4);
    }

    .super-chat-button mat-icon {
      margin-right: 8px;
    }
  `]
})
export class SuperChatButtonComponent {
  @Input() liveStreamId!: number;
  @Input() liveStreamTitle: string = 'Live Stream';
  @Output() superChatSent = new EventEmitter<any>();

  constructor(private dialog: MatDialog) {}

  openSuperChatDialog() {
    const dialogRef = this.dialog.open(SuperChatDialogComponent, {
      width: '600px',
      data: {
        liveStreamId: this.liveStreamId,
        liveStreamTitle: this.liveStreamTitle
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.superChatSent.emit(result);
      }
    });
  }
}
