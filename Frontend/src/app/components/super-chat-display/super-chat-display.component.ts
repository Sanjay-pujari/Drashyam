import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { SuperChatService } from '../../services/super-chat.service';
import { SuperChatDisplay, SUPER_CHAT_TIERS } from '../../models/super-chat.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-super-chat-display',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './super-chat-display.component.html',
  styleUrl: './super-chat-display.component.scss'
})
export class SuperChatDisplayComponent implements OnInit, OnDestroy {
  @Input() liveStreamId!: number;
  
  displayedSuperChats: SuperChatDisplay[] = [];
  isExpanded = true;
  private subscriptions: Subscription[] = [];
  private refreshInterval?: any;

  constructor(private superChatService: SuperChatService) {}

  ngOnInit() {
    this.loadSuperChats();
    // Refresh every 5 seconds to get new super chats
    this.refreshInterval = setInterval(() => {
      this.loadSuperChats();
    }, 5000);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  loadSuperChats() {
    const sub = this.superChatService.getLiveStreamSuperChats(this.liveStreamId).subscribe({
      next: (superChats) => {
        this.displayedSuperChats = superChats.slice(0, 10); // Show only recent 10
      },
      error: (error) => {
        console.error('Error loading super chats:', error);
      }
    });
    this.subscriptions.push(sub);
  }

  toggleDisplay() {
    this.isExpanded = !this.isExpanded;
  }

  trackBySuperChatId(index: number, superChat: SuperChatDisplay): number {
    return superChat.id;
  }

  getTierColor(amount: number): string {
    const tier = SUPER_CHAT_TIERS.find(t => amount >= t.amount) || SUPER_CHAT_TIERS[0];
    return tier.color;
  }

  formatTime(createdAt: string): string {
    const date = new Date(createdAt);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'now';
    if (diffMins < 60) return `${diffMins}m ago`;
    
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;
    
    return date.toLocaleDateString();
  }
}
